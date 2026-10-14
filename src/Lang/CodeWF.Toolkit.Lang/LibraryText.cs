using CodeWF.Toolkit.Lang.Json;
using System.Globalization;

namespace CodeWF.Toolkit.Lang;

internal static class LibraryText
{
    // Loader diagnostics must not re-enter the global manager while a plugin is loading under its lock.
    private static readonly JsonLocalizer Resources = new(typeof(LibraryText).Assembly, allowApplicationOverrides: false);
    internal static string Get(string key) => Resources.GetResource(key, CultureInfo.CurrentUICulture.Name);
    internal static string Format(string key, params object?[] arguments) => string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
}
