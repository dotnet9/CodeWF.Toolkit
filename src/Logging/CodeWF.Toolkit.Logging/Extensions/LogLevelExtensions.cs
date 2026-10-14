using LibraryResources = global::CodeWF.Toolkit.Logging.I18n.ResourceKeys;
using Microsoft.Extensions.Logging;

namespace CodeWF.Toolkit.Logging.Extensions;

public static class LogLevelExtensions
{
    public static string Description(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.Trace),
            LogLevel.Debug => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.Debug),
            LogLevel.Information => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.Information),
            LogLevel.Warning => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.Warning),
            LogLevel.Error => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.Error),
            LogLevel.Critical => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.Critical),
            LogLevel.None => global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Level.None),
            _ => level.ToString()
        };
    }
}
