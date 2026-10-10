namespace CodeWF.Toolkit.Network.Tests.NetWeaver.Models;

public enum ProcessStatus
{
    [Description("New")] New,
    [Description("Ready")] Ready,
    [Description("Running")] Running,
    [Description("Blocked")] Blocked,
    [Description("Terminated")] Terminated
}
