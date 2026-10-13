namespace CodeWF.Toolkit.EventBus.Socket.Helpers;

internal static class SocketHelper
{
    // Commands enter NetWeaver through INetObject; preserve the internal wire contract.
    private const System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes WireMembers =
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All;

    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(RequestIsEventServer))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(RequestSubscribe))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(RequestUnsubscribe))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(RequestPublish))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(RequestQuery))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(ResponseCommon))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(UpdateEvent))]
    [System.Diagnostics.CodeAnalysis.DynamicDependency(WireMembers, typeof(Heartbeat))]
    internal static string GetNewTaskId()
    {
        return Guid.NewGuid().ToString();
    }
}
