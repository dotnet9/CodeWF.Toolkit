using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.Response;

/// <summary>
///     返回服务端磁盘列表的响应对象。
/// </summary>
[NetHead(FileSystemSocketConstants.DriveListResponseObjectId, 1)]
public class DriveListResponse : INetObject
{
    /// <summary>
    ///     请求任务 ID，用于把响应与原请求对应起来。
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    ///     当前可用磁盘信息集合。
    /// </summary>
    public List<DiskInfo>? Disks { get; set; }

    public override string ToString() => global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Packet.DrivesResponse, TaskId, Disks?.Count ?? 0);
}
