using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.Helpers;
using Xunit;

namespace CodeWF.Toolkit.Tests.Files;

public class IpHelperTest
{
    [Fact]
    public async Task Test_GetLocalIpAsync_Success()
    {
        var ip = await IpHelper.GetLocalIpAsync();
        Assert.False(ip.IsNullOrEmpty());
    }

    [Fact]
    public async Task Test_GetAllIpv4Async_Success()
    {
        var ip = await IpHelper.GetAllIpV4Async();
        Assert.False(ip.IsNullOrEmpty());
    }

    [Fact]
    public void Test_GetAvailableTcpPort_Success()
    {
        var port = IpHelper.GetAvailableTcpPort();
        Assert.True(port > 0);
    }

    [Fact]
    public void Test_GetMulticastIpAndPort_Success()
    {
        IpHelper.GetMulticastIpAndPort(out var ip, out var port, needConnectCheck: true);
        Assert.False(ip.IsNullOrEmpty());
        Assert.False(port == default);
    }
}
