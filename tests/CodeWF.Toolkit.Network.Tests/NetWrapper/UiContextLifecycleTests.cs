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

    private sealed class NonPumpingContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) { }
    }
}
