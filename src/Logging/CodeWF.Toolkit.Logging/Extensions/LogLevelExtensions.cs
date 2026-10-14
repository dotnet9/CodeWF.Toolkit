using Microsoft.Extensions.Logging;

namespace CodeWF.Toolkit.Logging.Extensions;

public static class LogLevelExtensions
{
    public static string Description(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.Trace"),
            LogLevel.Debug => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.Debug"),
            LogLevel.Information => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.Information"),
            LogLevel.Warning => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.Warning"),
            LogLevel.Error => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.Error"),
            LogLevel.Critical => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.Critical"),
            LogLevel.None => global::CodeWF.Toolkit.Logging.LibraryText.Get("Toolkit.Logging.Level.None"),
            _ => level.ToString()
        };
    }
}
