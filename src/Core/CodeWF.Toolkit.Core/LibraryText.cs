using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Toolkit.Core;

internal static class LibraryText
{
    private static readonly JsonLocalizer Resources = new(typeof(LibraryText).Assembly);
    private static readonly JsonLocalizer AlgorithmResources = new(typeof(LibraryText).Assembly, allowApplicationOverrides: false);
    internal static string Get(string key) => Resources.GetResource(key);
    internal static string Format(string key, params object?[] arguments) => Resources.Format(key, arguments);
    // 中文数字字符表属于算法数据，不应随界面语言变化。
    internal static string Invariant(string key) => AlgorithmResources.GetResource(key, "zh-CN");
}
