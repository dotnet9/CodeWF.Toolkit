using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.Helpers;

/// <summary>
///     网络辅助类，提供网络通信相关的辅助方法
/// </summary>
public static class NetHelper
{
    /// <summary>
    ///     任务ID计数器
    /// </summary>
    private static int _taskId;

    /// <summary>
    ///     获取任务 ID，每次调用自增
    /// </summary>
    /// <returns>任务 ID</returns>
    public static int GetTaskId() => Interlocked.Increment(ref _taskId);

    /// <summary>
    ///     获取忽略任务的 ID
    /// </summary>
    /// <returns>忽略任务的 ID</returns>
    public static int GetIgnoreTask() => int.MinValue;

    /// <summary>
    ///     安全关闭 Socket 连接
    /// </summary>
    /// <param name="socket">要关闭的 Socket 对象</param>
    public static void CloseSocket(this Socket? socket)
    {
        string? remoteEndPoint = null;
        try
        {
            remoteEndPoint = socket?.RemoteEndPoint?.ToString();
            if (socket?.Connected == true)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Socket.CloseFailed, remoteEndPoint), ex, global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Socket.CloseFailedHint, remoteEndPoint));
        }
        finally
        {
            socket?.Close();
        }
    }
}
