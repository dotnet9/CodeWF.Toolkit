using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.Models;

/// <summary>
///     文件传输相关错误码。
/// </summary>
public enum FileTransferErrorCode
{
    /// <summary>
    ///     成功。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.Success)] Success = 0,

    /// <summary>
    ///     文件不存在。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.FileNotFound)] FileNotFound = -21,

    /// <summary>
    ///     删除失败。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.DeleteFailed)] DeleteFailed = -22,

    /// <summary>
    ///     服务端文件大于客户端已有文件。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Transfer.ServerFileLargerThanLocal)] UploadServerFileLarger = -31,

    /// <summary>
    ///     文件已存在，无需重复上传。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Transfer.UploadAlreadyExists)] UploadFileAlreadyExists = -32,

    /// <summary>
    ///     文件大小相同但哈希值不同。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.HashMismatch)] UploadFileHashMismatch = -33,

    /// <summary>
    ///     服务端文件不存在。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Transfer.ServerFileMissing)] DownloadServerFileNotFound = -41,

    /// <summary>
    ///     服务端文件小于客户端已有文件。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Transfer.ServerFileSmallerThanLocal)] DownloadServerFileSmaller = -42,

    /// <summary>
    ///     文件相同，不需要下载。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Transfer.DownloadNotNeeded)] DownloadFileIdentical = -43,

    /// <summary>
    ///     文件大小相同但哈希值不同。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.HashMismatch)] DownloadFileHashMismatch = -44,

    /// <summary>
    ///     目录不存在。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.DirectoryNotFound)] DirectoryNotFound = -1,

    /// <summary>
    ///     目录访问被拒绝。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.DirectoryAccessDenied)] DirectoryAccessDenied = -2,

    /// <summary>
    ///     未知错误。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.UnknownError)] UnknownError = -99,

    /// <summary>
    ///     请求或文件分块参数无效。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(FileTransferErrorCode), LibraryResources.Status.InvalidTransfer)] InvalidTransferRequest = -50
}
