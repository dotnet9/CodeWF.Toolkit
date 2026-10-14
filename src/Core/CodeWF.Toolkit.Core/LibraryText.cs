using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Toolkit.Core;

internal static class LibraryText
{
    private static readonly JsonLocalizer Resources = new(typeof(LibraryText).Assembly);
    private static readonly JsonLocalizer AlgorithmResources = new(typeof(LibraryText).Assembly, allowApplicationOverrides: false);
    internal static string Get(string key) => Resources.GetResource(key);
    internal static string Format(string key, params object?[] arguments) => Resources.Format(key, arguments);
    // Chinese numeral alphabets are algorithm data and must not change with the UI culture.
    internal static string Invariant(string key) => AlgorithmResources.GetResource(key, "zh-CN");
}
