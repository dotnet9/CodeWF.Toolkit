using System.Globalization;
using System.Reflection;

namespace CodeWF.Toolkit.Lang.Json;

/// <summary>
/// Reads a library's embedded JSON translations without requiring application registration or UI dependencies.
/// Application translations can override library keys; missing cultures fall back to the library's default.
/// </summary>
public sealed class JsonLocalizer
{
    private readonly JsonLangPlugin _resources;

    /// <summary>Creates a localizer for JSON resources embedded in an assembly.</summary>
    public JsonLocalizer(Assembly resourceAssembly, string fallbackCulture = "en-US")
    {
        ArgumentNullException.ThrowIfNull(resourceAssembly);
        _resources = new JsonLangPlugin { ScanResourceFolder = false };
        _resources.AddResource(resourceAssembly);
        _resources.Load(CultureInfo.GetCultureInfo(fallbackCulture));
    }

    /// <summary>Resolves a key using the requested or current UI culture.</summary>
    public string GetResource(string key, string? cultureName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var culture = cultureName ?? I18nManager.Instance.Culture?.Name ?? CultureInfo.CurrentUICulture.Name;
        var applicationText = I18nManager.Instance.GetResource(key, culture);
        return applicationText != key ? applicationText : _resources.GetResource(key, culture);
    }

    /// <summary>Formats a translation using the current formatting culture.</summary>
    public string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, GetResource(key), arguments);
}
