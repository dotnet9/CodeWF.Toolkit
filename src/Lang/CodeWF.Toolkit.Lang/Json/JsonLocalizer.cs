using System.Globalization;
using System.Reflection;

namespace CodeWF.Toolkit.Lang.Json;

/// <summary>
/// 读取库程序集内嵌的 JSON 翻译，无需宿主注册，也不依赖界面框架。
/// 应用资源可按同键覆盖库文案，缺失语言自动回退到库的默认语言。
/// </summary>
public sealed class JsonLocalizer
{
    private readonly JsonLangPlugin _resources;
    private readonly bool _allowApplicationOverrides;

    /// <summary>为指定程序集的内嵌 JSON 资源创建本地化查询器。</summary>
    public JsonLocalizer(Assembly resourceAssembly, string fallbackCulture = "en-US", bool allowApplicationOverrides = true)
    {
        ArgumentNullException.ThrowIfNull(resourceAssembly);
        _allowApplicationOverrides = allowApplicationOverrides;
        _resources = new JsonLangPlugin { ScanResourceFolder = false };
        _resources.AddResource(resourceAssembly);
        _resources.Load(CultureInfo.GetCultureInfo(fallbackCulture));
    }

    /// <summary>按指定语言或当前界面语言查询资源。</summary>
    public string GetResource(string key, string? cultureName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var culture = cultureName ?? I18nManager.Instance.Culture?.Name ?? CultureInfo.CurrentUICulture.Name;
        var applicationText = _allowApplicationOverrides ? I18nManager.Instance.GetResource(key, culture) : key;
        return applicationText != key ? applicationText : _resources.GetResource(key, culture);
    }

    /// <summary>按当前区域格式设置填充翻译中的占位参数。</summary>
    public string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, GetResource(key), arguments);
}
