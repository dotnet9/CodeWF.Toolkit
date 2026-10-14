using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.Models;

/// <summary>
///     TCP 通用响应状态枚举。
/// </summary>
public enum TcpResponseStatus
{
    /// <summary>
    ///     成功。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(TcpResponseStatus), LibraryResources.Status.Success)] Success,

    /// <summary>
    ///     失败。
    /// </summary>
    [global::CodeWF.Toolkit.Lang.LocalizedDescription(typeof(TcpResponseStatus), LibraryResources.Status.Fail)] Fail
}
