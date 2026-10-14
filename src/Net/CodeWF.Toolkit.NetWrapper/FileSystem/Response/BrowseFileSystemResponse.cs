using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.Response;

/// <summary>
///     浏览服务端文件系统的响应对象。
/// </summary>
[NetHead(FileSystemSocketConstants.BrowseFileSystemResponseObjectId, 1)]
public class BrowseFileSystemResponse : INetObject
{
    /// <summary>
    ///     请求任务 ID，用于把响应与原请求对应起来。
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    ///     当前目录下的总条目数量。
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    ///     单页返回的条目数量。
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     总页数。
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    ///     当前页索引，从 0 开始。
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    ///     当前页的文件系统条目集合。
    /// </summary>
    public List<FileSystemEntry>? Entries { get; set; }

    public override string ToString() =>
        global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Packet.BrowseResponse, TaskId, PageIndex + 1, PageCount, Entries?.Count ?? 0);
}
