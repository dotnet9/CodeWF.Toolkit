namespace CodeWF.Toolkit.EventBus.Socket.Models.Requests;

[NetHead(3, 1)]
internal class RequestUnsubscribe : INetObject
{
    public string TaskId { get; set; } = null!;

    public string Subject { get; set; } = null!;
}