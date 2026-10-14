using LibraryResources = global::CodeWF.Toolkit.Logging.I18n.ResourceKeys;
namespace CodeWF.Toolkit.Logging;

using Microsoft.Extensions.Logging;

public sealed record ConsoleLogOptions
{
    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

    internal void Validate()
    {
        if (!Enum.IsDefined(MinimumLevel))
            throw new ArgumentOutOfRangeException(nameof(MinimumLevel), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.ConsoleLevel));
        if (string.IsNullOrWhiteSpace(TimestampFormat))
            throw new ArgumentException(global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.ConsoleTimestampRequired), nameof(TimestampFormat));

        try
        {
            _ = DateTimeOffset.Now.ToString(TimestampFormat);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.ConsoleTimestampInvalid), nameof(TimestampFormat), ex);
        }
    }
}
