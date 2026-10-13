using System.Net;
using CodeWF.Toolkit.NetWrapper.Helpers;
using Xunit;

namespace CodeWF.Toolkit.Network.Tests.NetWrapper;

public sealed class UiContextLifecycleTests
{
    [Fact]
    public async Task ConnectAndSynchronousStopDoNotRequireTheCallerContextToPump()
    {
        var server = new TcpSocketServer();
        var client = new TcpSocketClient();
        try
        {
            await Task.Run(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new NonPumpingContext());
                var started = server.StartAsync("context-regression", "127.0.0.1", 0)
                    .WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                Assert.True(started.IsSuccess, started.ErrorMessage);
                var port = ((IPEndPoint)server.Server!.LocalEndPoint!).Port;
                var connected = client.ConnectAsync("context-regression", "127.0.0.1", port)
                    .WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                Assert.True(connected.IsSuccess, connected.ErrorMessage);
                client.Stop();
                server.StopAsync().WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            }).WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            await client.StopAsync();
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task UdpSynchronousStopDoesNotRequireTheCallerContextToPump()
    {
        await Task.Run(() =>
        {
            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new NonPumpingContext());
            try
            {
                using var probe = new System.Net.Sockets.UdpClient(0);
                var port = ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
                probe.Close();
                var client = new UdpSocketClient();
                client.ConnectAsync("udp-context-regression", "127.0.0.1", port, "127.0.0.1", 1).GetAwaiter().GetResult();
                Assert.True(client.IsRunning);
                Assert.True(client.Stop());
            }
            finally { SynchronizationContext.SetSynchronizationContext(previous); }
        }).WaitAsync(TimeSpan.FromSeconds(10));
    }

    private sealed class NonPumpingContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) { }
    }
}
