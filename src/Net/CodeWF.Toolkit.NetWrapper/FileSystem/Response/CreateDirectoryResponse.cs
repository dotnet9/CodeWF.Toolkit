using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.Response;

/// <summary>
///     创建服务端目录的响应对象。
/// </summary>
[NetHead(FileSystemSocketConstants.CreateDirectoryResponseObjectId, 1)]
public class CreateDirectoryResponse : INetObject
{
    /// <summary>
    ///     请求任务 ID，用于把响应与原请求对应起来。
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    ///     标记目录创建是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     服务端目录路径。
    /// </summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    ///     响应消息。
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public override string ToString() => global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Packet.CreateResponse, TaskId, Success, DirectoryPath, Message);
}
