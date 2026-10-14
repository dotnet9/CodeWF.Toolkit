namespace CodeWF.Toolkit.NetWrapper.Response;

/// <summary>
///     文件系统条目类型。
/// </summary>
public enum FileType
{
    /// <summary>
    ///     未知类型。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileType), "Toolkit.NetWrapper.Entry.Unknown")] Unknown = 0,

    /// <summary>
    ///     普通文件。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileType), "Toolkit.NetWrapper.Entry.File")] File = 1,

    /// <summary>
    ///     目录。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileType), "Toolkit.NetWrapper.Entry.Directory")] Directory = 2,

    /// <summary>
    ///     快捷方式。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileType), "Toolkit.NetWrapper.Entry.Shortcut")] Shortcut = 3,

    /// <summary>
    ///     符号链接。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileType), "Toolkit.NetWrapper.Entry.SymbolicLink")] SymbolicLink = 4,

    /// <summary>
    ///     重解析点。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileType), "Toolkit.NetWrapper.Entry.ReparsePoint")] ReparsePoint = 5
}

/// <summary>
///     单个文件系统条目的信息对象。
/// </summary>
public class FileSystemEntry
{
    /// <summary>
    ///     条目名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     条目大小，单位为字节。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///     最后修改时间，使用 <see cref="System.DateTime.Ticks" /> 表示。
    /// </summary>
    public long LastModifiedTime { get; set; }

    /// <summary>
    ///     条目类型。
    /// </summary>
    public FileType EntryType { get; set; }
}
