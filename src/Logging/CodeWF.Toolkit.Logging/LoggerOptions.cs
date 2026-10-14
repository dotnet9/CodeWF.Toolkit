using LibraryResources = global::CodeWF.Toolkit.Logging.I18n.ResourceKeys;
using Microsoft.Extensions.Logging;

namespace CodeWF.Toolkit.Logging;

public enum LogQueueFullMode
{
    Wait,
    DropNewest,
    DropTraceAndDebug
}

/// <summary>Core pipeline configuration.</summary>
public sealed record LoggerOptions
{
    public LogLevel MinimumLevel { get; init; } = LogLevel.Information;
    /// <summary>在日志进入队列时生成级别文本，避免语言切换重绘历史事件。</summary>
    public Func<LogLevel, string?>? LocalizedLevelFormatter { get; init; }
    public FileLogOptions? File { get; init; }
    public bool EnableConsole { get; init; } = true;
    public ConsoleLogOptions? Console { get; init; }
    public string LineTemplate { get; init; } = LineTemplateController.DefaultTemplate;
    public bool EnableEventFeed { get; init; } = true;
    public int QueueCapacity { get; init; } = 10_000;
    public LogQueueFullMode QueueFullMode { get; init; } = LogQueueFullMode.DropTraceAndDebug;
    public TimeSpan EnqueueTimeout { get; init; } = TimeSpan.FromMilliseconds(100);
    public int RecentEventCapacity { get; init; } = 2_000;

    internal void Validate()
    {
        if (!Enum.IsDefined(MinimumLevel))
            throw new ArgumentOutOfRangeException(nameof(MinimumLevel), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.MinimumLevel));
        if (!Enum.IsDefined(QueueFullMode))
            throw new ArgumentOutOfRangeException(nameof(QueueFullMode), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.QueuePolicy));
        if (QueueCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(QueueCapacity), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.QueueCapacity));
        if (RecentEventCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(RecentEventCapacity), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.RecentCapacity));
        if (EnqueueTimeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(EnqueueTimeout), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.EnqueueTimeout));
        if (!LogTemplateFormatter.TryValidate(LineTemplate, out var error))
            throw new ArgumentException(error, nameof(LineTemplate));

        File?.Validate();
        Console?.Validate();
    }

    internal LoggerOptions Normalize()
    {
        Validate();
        return File is null
            ? this
            : this with { File = File with { DirectoryPath = Path.GetFullPath(File.DirectoryPath) } };
    }
}
