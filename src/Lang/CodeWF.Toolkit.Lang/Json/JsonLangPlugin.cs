using LibraryResources = global::CodeWF.Toolkit.Lang.I18n.ResourceKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace CodeWF.Toolkit.Lang.Json;

/// <summary>
/// JSON 语言资源插件。支持从输出目录扫描 JSON 文件，也支持从额外程序集中读取嵌入资源。
/// </summary>
public class JsonLangPlugin<TLanguage> : ILangPlugin where TLanguage : LocalizationLanguage, new()
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
    /// JSON 文件扫描目录，默认使用应用程序输出目录。
    /// </summary>
    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>除已添加的程序集外，是否同时扫描宿主资源目录。</summary>
    public bool ScanResourceFolder { get; set; } = true;

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

            if (ScanResourceFolder && !Directory.Exists(ResourceFolder))
            {
                _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.FolderMissing, ResourceFolder));
            }
            else if (ScanResourceFolder)
            {
                foreach (var jsonFile in Directory.GetFiles(ResourceFolder, "*.json", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
                {
                    TryAddLanguageFile(jsonFile);
                }
            }

            LoadEmbeddedResources(_resourceAssemblies);
            ApplyFileOverrides();

            if (Resources.Count == 0)
            {
                _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Get(LibraryResources.Diagnostics.JsonResourcesRequired));
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
                         .Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).OrderBy(name => name, StringComparer.Ordinal))
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
            using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
            if (TryAddLanguage(doc.RootElement, fromFile: true))
            {
                return true;
            }

            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.JsonMetadataSkipped, filePath));
            return false;
        }
        catch
        {
            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.JsonFileSkipped, filePath));
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

            using var doc = JsonDocument.Parse(stream);
            if (TryAddLanguage(doc.RootElement))
            {
                return true;
            }

            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.JsonEmbeddedMetadataSkipped, assembly.GetName().Name, resourceName));
            return false;
        }
        catch
        {
            _loadDiagnostics.Add(global::CodeWF.Toolkit.Lang.LibraryText.Format(LibraryResources.Diagnostics.JsonEmbeddedSkipped, assembly.GetName().Name, resourceName));
            return false;
        }
    }

    private bool TryAddLanguage(JsonElement root, bool fromFile = false)
    {
        if (!TryReadLanguage(root, out var language))
        {
            return false;
        }

        var allProperties = new Dictionary<string, string>();
        CollectJsonProperties(root, string.Empty, allProperties);

        var excludeKeys = new[] { Consts.LanguageKey, Consts.DescriptionKey, Consts.CultureNameKey };
        foreach (var (key, value) in allProperties)
        {
            if (!excludeKeys.Any(k => key.Equals(k, StringComparison.OrdinalIgnoreCase)))
            {
                language.Languages[key] = value;
            }
        }

        LocalizationLanguage.MergeInto(Resources, language);
        if (fromFile) LocalizationLanguage.MergeInto(_fileResources, language);
        return true;
    }

    private static bool TryReadLanguage(JsonElement root, out TLanguage language)
    {
        language = new TLanguage();

        if (!root.TryGetProperty(Consts.LanguageKey, out var languageElement)
            || !root.TryGetProperty(Consts.DescriptionKey, out var descriptionElement)
            || !root.TryGetProperty(Consts.CultureNameKey, out var cultureNameElement))
        {
            return false;
        }

        var languageName = languageElement.GetString();
        var description = descriptionElement.GetString();
        var cultureName = cultureNameElement.GetString();
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

    private static void CollectJsonProperties(JsonElement element, string currentPath, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var newPath = string.IsNullOrEmpty(currentPath)
                        ? property.Name
                        : $"{currentPath}.{property.Name}";

                    CollectJsonProperties(property.Value, newPath, result);
                }

                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    CollectJsonProperties(item, $"{currentPath}[{index}]", result);
                    index++;
                }

                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                result[currentPath] = element.ToString();
                break;
        }
    }
}

/// <summary>使用默认语言元数据类型的 JSON 资源加载器。</summary>
public class JsonLangPlugin : JsonLangPlugin<LocalizationLanguage>
{
}
