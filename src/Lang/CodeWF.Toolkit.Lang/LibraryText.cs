using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Toolkit.Lang;

internal static class LibraryText
{
    private static readonly JsonLocalizer Resources = new(typeof(LibraryText).Assembly);
    internal static string Get(string key) => Resources.GetResource(key);
    internal static string Format(string key, params object?[] arguments) => Resources.Format(key, arguments);
}
