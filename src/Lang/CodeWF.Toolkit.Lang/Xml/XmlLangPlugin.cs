using LibraryResources = global::CodeWF.Toolkit.Lang.I18n.ResourceKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CodeWF.Toolkit.Lang.Xml;

/// <summary>
/// XML 语言资源插件。支持从输出目录扫描 XML 文件，也支持从额外程序集中读取嵌入资源。
/// </summary>
public class XmlLangPlugin<TLanguage> : ILangPlugin where TLanguage : LocalizationLanguage, new()
{
    private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private CultureInfo _culture = CultureInfo.InvariantCulture;
    private readonly object _syncRoot = new();
    private readonly List<string> _loadDiagnostics = new();
    private readonly List<Assembly> _resourceAssemblies = new();
    private readonly Dictionary<string, TLanguage> _fileResources = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 已加载的语言资源缓存，Key 为文化名称。
    /// </summary>
    public Dictionary<string, TLanguage> Resources { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// XML 文件扫描目录，默认使用应用程序输出目录。
    /// </summary>
    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 最近一次加载资源时产生的诊断信息。
    /// </summary>
    public IReadOnlyList<string> LoadDiagnostics
    {
        get
        {
            lock (_syncRoot)
            {
                return _loadDiagnostics.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public CultureInfo Culture
    {
        get
        {
            lock (_syncRoot)
            {
                return _culture;
            }
        }
        set
        {
            lock (_syncRoot)
            {
                _culture = value;
            }
        }
    }

    /// <inheritdoc />
    public void Load(CultureInfo cultureInfo)
    {
        lock (_syncRoot)
        {
            _defaultCulture = cultureInfo;
            _culture = cultureInfo;
            Resources.Clear();
            _fileResources.Clear();
            _loadDiagnostics.Clear();

            if (!Directory.Exists(ResourceFolder))
            {
                _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.FolderMissing, ResourceFolder));
            }
            else
            {
                foreach (var xmlFile in Directory.GetFiles(ResourceFolder, "*.xml", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
                {
                    TryAddLanguageFile(xmlFile);
                }
            }

            LoadEmbeddedResources(_resourceAssemblies);
            ApplyFileOverrides();

            if (Resources.Count == 0)
            {
                _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Get(LibraryResources.Diagnostics.XmlResourcesRequired));
            }
        }
    }

    /// <summary>按内层到外层的顺序添加库资源；后添加的库覆盖同键，宿主输出资源始终优先。</summary>
    public void AddResource(params Assembly[] assemblies)
    {
        lock (_syncRoot)
        {
            var newAssemblies = assemblies
                .Where(assembly => assembly != null)
                .Distinct()
                .Where(assembly => !_resourceAssemblies.Contains(assembly))
                .ToArray();

            if (newAssemblies.Length == 0) return;
            _resourceAssemblies.AddRange(newAssemblies);
            LoadEmbeddedResources(newAssemblies);
            ApplyFileOverrides();
        }
    }

    private void ApplyFileOverrides()
    {
        foreach (var language in _fileResources.Values) LocalizationLanguage.MergeInto(Resources, language);
    }

    private void LoadEmbeddedResources(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var resourceName in assembly.GetManifestResourceNames()
                         .Where(name => name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).OrderBy(name => name, StringComparer.Ordinal))
            {
                TryAddLanguageResource(assembly, resourceName);
            }
        }
    }

    /// <inheritdoc />
    public List<TLanguage>? GetLanguages()
    {
        lock (_syncRoot)
        {
            return Resources.Values.Select(language => language.Snapshot<TLanguage>()).ToList();
        }
    }

    List<LocalizationLanguage>? ILangPlugin.GetLanguages() => GetLanguages()?.Cast<LocalizationLanguage>().ToList();

    /// <inheritdoc />
    public string GetResource(string key, string? cultureName = null)
    {
        lock (_syncRoot)
        {
            var culture = _culture;
            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                culture = CultureFallback.TryCreateCulture(cultureName, out var explicitCulture)
                    ? explicitCulture
                    : _defaultCulture;
            }

            foreach (var candidate in CultureFallback.Enumerate(culture, _defaultCulture))
            {
                if (Resources.TryGetValue(candidate.Name, out var currentLanguages)
                    && currentLanguages.Languages.TryGetValue(key, out var resource))
                {
                    return resource;
                }
            }

            return key;
        }
    }

    private bool TryAddLanguageFile(string filePath)
    {
        try
        {
            if (TryAddLanguage(XDocument.Load(filePath), fromFile: true))
            {
                return true;
            }

            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.XmlMetadataSkipped, filePath));
            return false;
        }
        catch
        {
            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.XmlFileSkipped, filePath));
            return false;
        }
    }

    private bool TryAddLanguageResource(Assembly assembly, string resourceName)
    {
        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return false;
            }

            if (TryAddLanguage(XDocument.Load(stream)))
            {
                return true;
            }

            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.XmlEmbeddedMetadataSkipped, assembly.GetName().Name, resourceName));
            return false;
        }
        catch
        {
            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.XmlEmbeddedSkipped, assembly.GetName().Name, resourceName));
            return false;
        }
    }

    private bool TryAddLanguage(XDocument xmlDoc, bool fromFile = false)
    {
        if (!TryReadLanguage(xmlDoc.Root, out var language))
        {
            return false;
        }

        var propertyNodes = xmlDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
            .Where(e => e.Descendants().Any() != true);

        foreach (var propertyNode in propertyNodes)
        {
            var ancestorsNodeNames = propertyNode.AncestorsAndSelf().Reverse().Select(node => node.Name.LocalName);
            var key = string.Join(".", ancestorsNodeNames);
            language.Languages[key] = propertyNode.Value;
        }

        LocalizationLanguage.MergeInto(Resources, language);
        if (fromFile) LocalizationLanguage.MergeInto(_fileResources, language);
        return true;
    }

    private static bool TryReadLanguage(XElement? element, out TLanguage language)
    {
        language = new TLanguage();
        if (element == null)
        {
            return false;
        }

        var languageName = element.Attribute(Consts.LanguageKey)?.Value;
        var description = element.Attribute(Consts.DescriptionKey)?.Value;
        var cultureName = element.Attribute(Consts.CultureNameKey)?.Value;
        if (string.IsNullOrWhiteSpace(languageName)
            || string.IsNullOrWhiteSpace(description)
            || string.IsNullOrWhiteSpace(cultureName)
            || !CultureFallback.TryCreateCulture(cultureName, out var culture))
        {
            return false;
        }

        language = new TLanguage
        {
            Language = languageName,
            Description = description,
            CultureName = culture.Name
        };
        return true;
    }
}

/// <summary>使用默认语言元数据类型的 XML 资源加载器。</summary>
public class XmlLangPlugin : XmlLangPlugin<LocalizationLanguage>
{
}
