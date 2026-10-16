namespace CodeWF.Toolkit.Files.Helpers;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

public static class AppConfigHelper
{
    private const string ConfigurationElementName = "configuration";
    private const string AppSettingsElementName = "appSettings";
    private const string AddElementName = "add";
    private const string KeyAttributeName = "key";
    private const string ValueAttributeName = "value";

    public static string GetDefaultConfigPath()
    {
        if (AppContext.GetData("APP_CONFIG_FILE") is string configuredPath &&
            !string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath))
        {
            var processConfigPath = processPath + ".config";
            if (File.Exists(processConfigPath))
            {
                return processConfigPath;
            }

            var dllConfigPath = Path.Combine(
                AppContext.BaseDirectory,
                $"{Path.GetFileNameWithoutExtension(processPath)}.dll.config");
            if (File.Exists(dllConfigPath))
            {
                return dllConfigPath;
            }

            return processConfigPath;
        }

        return Path.Combine(AppContext.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.config");
    }

    [RequiresUnreferencedCode("System.Configuration.ConfigurationManager is not trim-safe. Prefer the file-path overloads on AppConfigHelper for Native AOT.")]
    public static Configuration OpenConfig(string filePath)
    {
        var fileMap = new ExeConfigurationFileMap
        {
            ExeConfigFilename = filePath
        };
        var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        return config;
    }

    [RequiresUnreferencedCode("System.Configuration.Configuration is not trim-safe. Prefer TryGet<T>(string filePath, string name, out T? value).")]
    public static bool TryGet<T>(this Configuration config, string name, out T? value)
    {
        try
        {
            var setting = config.AppSettings.Settings[name];
            if (setting is null)
            {
                value = default;
                return false;
            }

            return TryConvert(setting.Value, out value);
        }
        catch
        {
            value = default;
            return false;
        }
    }

    public static bool TryGet<T>(string name, out T? value)
    {
        return TryGet(GetDefaultConfigPath(), name, out value);
    }

    public static bool TryGet<T>(string filePath, string name, out T? value)
    {
        value = default;
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            var appSettings = GetAppSettings(XDocument.Load(filePath));
            var setting = FindSetting(appSettings, name);
            return setting is not null && TryConvert(setting.Attribute(ValueAttributeName)?.Value, out value);
        }
        catch
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// 一次读取全部配置项。文件不存在或无法读取时返回空快照，后续读取快照不会访问磁盘。
    /// </summary>
    public static IReadOnlyDictionary<string, string> ReadAll(string filePath)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            if (File.Exists(filePath))
            {
                var seenKeys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var setting in GetAppSettings(XDocument.Load(filePath)).Elements(AddElementName))
                {
                    var key = setting.Attribute(KeyAttributeName)?.Value;
                    var value = setting.Attribute(ValueAttributeName)?.Value;
                    if (key is not null && seenKeys.Add(key) && value is not null)
                    {
                        values.Add(key, value);
                    }
                }
            }
        }
        catch
        {
            // 与单项读取保持一致：损坏、不可读的配置交由调用方使用默认值。
            values.Clear();
        }

        return values;
    }

    /// <summary>一次读取默认配置文件的全部配置项。</summary>
    public static IReadOnlyDictionary<string, string> ReadAll() => ReadAll(GetDefaultConfigPath());

    /// <summary>从已读取的快照获取类型化配置值，不重复解析配置文件。</summary>
    public static bool TryGet<T>(this IReadOnlyDictionary<string, string> settings, string name, out T? value)
    {
        value = default;
        return settings.TryGetValue(name, out var rawValue) && TryConvert(rawValue, out value);
    }

    [RequiresUnreferencedCode("System.Configuration.Configuration is not trim-safe. Prefer TryGetOrSet<T>(string filePath, string name, out T? value, T? defaultValue).")]
    public static bool TryGetOrSet<T>(this Configuration config, string name, out T? value, T? defaultValue = default)
    {
        try
        {
            if (config.TryGet(name, out value))
            {
                return true;
            }

            config.SetOrAdd(name, defaultValue);
            value = defaultValue;
            return false;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    public static bool TryGetOrSet<T>(string name, out T? value, T? defaultValue = default)
    {
        return TryGetOrSet(GetDefaultConfigPath(), name, out value, defaultValue);
    }

    public static bool TryGetOrSet<T>(string filePath, string name, out T? value, T? defaultValue = default)
    {
        if (TryGet(filePath, name, out value))
        {
            return true;
        }

        Set(filePath, name, defaultValue);
        value = defaultValue;
        return false;
    }

    [RequiresUnreferencedCode("System.Configuration.Configuration is not trim-safe. Prefer Set<T>(string filePath, string name, T value).")]
    public static void Set<T>(this Configuration config, string name, T value)
    {
        var setting = config.AppSettings.Settings[name];
        if (setting is null)
        {
            config.AppSettings.Settings.Add(name, ConvertToString(value));
        }
        else
        {
            setting.Value = ConvertToString(value);
        }

        config.Save(ConfigurationSaveMode.Modified);
    }

    public static void Set<T>(string name, T value)
    {
        Set(GetDefaultConfigPath(), name, value);
    }

    public static void Set<T>(string filePath, string name, T value)
    {
        var document = LoadOrCreateConfig(filePath);
        var appSettings = GetAppSettings(document);
        SetSetting(appSettings, name, value);
        SaveDocument(document, filePath);
    }

    /// <summary>
    /// 批量更新配置项，只加载和保存文件各一次；保留未更新的键及其他配置节。
    /// </summary>
    public static void SetMany(string filePath, IReadOnlyDictionary<string, object?> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        var document = LoadOrCreateConfig(filePath);
        var appSettings = GetAppSettings(document);
        foreach (var entry in values)
        {
            SetSetting(appSettings, entry.Key, entry.Value);
        }

        SaveDocument(document, filePath);
    }

    /// <summary>在默认配置文件中批量更新配置项。</summary>
    public static void SetMany(IReadOnlyDictionary<string, object?> values) => SetMany(GetDefaultConfigPath(), values);

    private static void SetSetting<T>(XElement appSettings, string name, T value)
    {
        var setting = FindSetting(appSettings, name);
        if (setting is null)
        {
            appSettings.Add(new XElement(
                AddElementName,
                new XAttribute(KeyAttributeName, name),
                new XAttribute(ValueAttributeName, ConvertToString(value))));
        }
        else
        {
            setting.SetAttributeValue(ValueAttributeName, ConvertToString(value));
        }
    }

    private static void SaveDocument(XDocument document, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        document.Save(filePath);
    }

    [RequiresUnreferencedCode("System.Configuration.Configuration is not trim-safe. Prefer SetOrAdd<T>(string filePath, string name, T value).")]
    public static void SetOrAdd<T>(this Configuration config, string name, T value)
    {
        config.Set(name, value);
    }

    public static void SetOrAdd<T>(string name, T value)
    {
        Set(name, value);
    }

    public static void SetOrAdd<T>(string filePath, string name, T value)
    {
        Set(filePath, name, value);
    }

    [RequiresUnreferencedCode("System.Configuration.Configuration is not trim-safe. Prefer GetOrAdd<T>(string filePath, string name, T? defaultValue).")]
    public static T GetOrAdd<T>(this Configuration config, string name, T? defaultValue)
    {
        config.TryGetOrSet(name, out var value, defaultValue);
        return value!;
    }

    public static T GetOrAdd<T>(string name, T? defaultValue)
    {
        return GetOrAdd(GetDefaultConfigPath(), name, defaultValue);
    }

    public static T GetOrAdd<T>(string filePath, string name, T? defaultValue)
    {
        TryGetOrSet(filePath, name, out var value, defaultValue);
        return value!;
    }

    private static XDocument LoadOrCreateConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            return XDocument.Load(filePath);
        }

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ConfigurationElementName, new XElement(AppSettingsElementName)));
    }

    private static XElement GetAppSettings(XDocument document)
    {
        if (document.Root is null || document.Root.Name != ConfigurationElementName)
        {
            document.RemoveNodes();
            document.Add(new XElement(ConfigurationElementName));
        }

        var root = document.Root!;
        var appSettings = root.Element(AppSettingsElementName);
        if (appSettings is not null)
        {
            return appSettings;
        }

        appSettings = new XElement(AppSettingsElementName);
        root.Add(appSettings);
        return appSettings;
    }

    private static XElement? FindSetting(XElement appSettings, string name)
    {
        foreach (var setting in appSettings.Elements(AddElementName))
        {
            if (string.Equals(setting.Attribute(KeyAttributeName)?.Value, name, StringComparison.Ordinal))
            {
                return setting;
            }
        }

        return null;
    }

    private static bool TryConvert<T>(string? rawValue, out T? value)
    {
        value = default;
        if (rawValue is null)
        {
            return false;
        }

        try
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (targetType == typeof(string))
            {
                value = (T)(object)rawValue;
                return true;
            }

            if (targetType.IsEnum)
            {
                value = (T)Enum.Parse(targetType, rawValue, ignoreCase: true);
                return true;
            }

            value = (T)Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    private static string ConvertToString<T>(T value)
    {
        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
