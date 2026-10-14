using CodeWF.Toolkit.Lang.Json;
using System.Globalization;

namespace CodeWF.Toolkit.Lang;

internal static class LibraryText
{
    // 插件持锁加载期间，诊断文案查询不能再次进入全局管理器。
    private static readonly JsonLocalizer Resources = new(typeof(LibraryText).Assembly, allowApplicationOverrides: false);
    internal static string Get(string key) => Resources.GetResource(key, CultureInfo.CurrentUICulture.Name);
    internal static string Format(string key, params object?[] arguments) => string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
}
