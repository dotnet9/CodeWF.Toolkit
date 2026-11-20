using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.Helpers;
using Xunit;

namespace CodeWF.Toolkit.Tests.Files;

public class IpHelperTest
{
    [Fact]
    public async Task Test_GetLocalIpAsync_Success()
    {
        string? ip = null;
        try
        {
            ip = await IpHelper.GetLocalIpAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // 外网探测服务（ipv4.gdt.qq.com）在部分网络/CI 出口不可达，属环境依赖而非代码缺陷。
            Assert.Skip("External IP probe service is unreachable from this network.");
        }

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
