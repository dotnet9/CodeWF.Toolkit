using LibraryResources = global::CodeWF.Toolkit.Logging.I18n.ResourceKeys;
namespace CodeWF.Toolkit.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// 文件日志配置。
/// </summary>
public sealed record FileLogOptions
{
    /// <summary>
    /// 最终日志目录。组件不会再自动追加子目录；文件名包含当前进程 ID。
    /// </summary>
    public required string DirectoryPath { get; set; }

    /// <summary>
    /// 单个日志文件最大字节数。
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 1_000L * 1024 * 1024;

    public int RetentionDays { get; set; } = 30;

    public int? RetainedFileCountLimit { get; set; }

    public long? MaxDirectorySizeBytes { get; set; }

    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// 累积到此数量后立即刷新文件缓冲区。
    /// </summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>
    /// 文件缓冲区最长刷新间隔。
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// 日志时间格式。
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// 文本输出模板；为空时使用 CodeWF 默认详细格式。
    /// </summary>
    public string? OutputTemplate { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new ArgumentException(global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.DirectoryRequired), nameof(DirectoryPath));
        if (MaxFileSizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxFileSizeBytes), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.FileSizePositive));
        if (RetentionDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(RetentionDays), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.RetentionDaysPositive));
        if (RetainedFileCountLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(RetainedFileCountLimit), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.FileCountPositive));
        if (MaxDirectorySizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxDirectorySizeBytes), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.DirectorySizePositive));
        if (!Enum.IsDefined(MinimumLevel))
            throw new ArgumentOutOfRangeException(nameof(MinimumLevel), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.FileLevel));
        if (BatchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(BatchSize), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.BatchSizePositive));
        if (FlushInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(FlushInterval), global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.FlushIntervalPositive));
        if (string.IsNullOrWhiteSpace(TimestampFormat))
            throw new ArgumentException(global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.TimestampRequired), nameof(TimestampFormat));
        if (OutputTemplate is not null && !LogTemplateFormatter.TryValidate(OutputTemplate, out var templateError))
            throw new ArgumentException(templateError, nameof(OutputTemplate));

        try
        {
            _ = DateTimeOffset.Now.ToString(TimestampFormat);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(global::CodeWF.Toolkit.Logging.LibraryText.Get(LibraryResources.Validation.TimestampInvalid), nameof(TimestampFormat), ex);
        }
    }
}
