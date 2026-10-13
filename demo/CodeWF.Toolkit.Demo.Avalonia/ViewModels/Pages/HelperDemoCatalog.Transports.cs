using System.Net;
using System.Net.Sockets;
using CodeWF.Toolkit.Core.Helpers;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.EventBus.Socket;
using CodeWF.Toolkit.NetWrapper.FileSystem;
using CodeWF.Toolkit.NetWrapper.Response;
using CodeWF.Toolkit.NetWrapper.Helpers;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

public static partial class HelperDemoCatalog
{
    private static async Task<string> TcpDelivery(string text)
    {
        var server = new TcpSocketServer();
        var client = new TcpSocketClient();
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = server.RegisterCommandHandler((_, _, command) =>
        {
            if (!command.IsCommand<SamplePacket>()) return Task.FromResult(false);
            received.TrySetResult(command.GetCommand<SamplePacket>().Title);
            return Task.FromResult(true);
        });
        try
        {
            var started = await server.StartAsync("catalog", "127.0.0.1", 0);
            if (!started.IsSuccess) throw new InvalidOperationException(started.ErrorMessage);
            var port = ((IPEndPoint)server.Server!.LocalEndPoint!).Port;
            var connected = await client.ConnectAsync("catalog", "127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
            if (!connected.IsSuccess) throw new InvalidOperationException(connected.ErrorMessage);
            await client.SendCommandAsync(new SamplePacket { Title = text });
            var payload = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
            if (payload != text) throw new InvalidOperationException("TCP payload changed.");
            return $"127.0.0.1:{port}\nSent = {text}\nReceived = {payload}";
        }
        finally { await client.StopAsync(); await server.StopAsync(); }
    }

    private static async Task<string> EventDelivery(string text)
    {
        using var broker = new EventServer();
        using var publisher = new EventClient();
        using var subscriber = new EventClient();
        var port = IpHelper.GetAvailableTcpPort();
        var delivery = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            await broker.StartAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
            if (!await subscriber.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5))
                || !await publisher.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5)))
                throw new InvalidOperationException("Event clients did not connect.");
            subscriber.Subscribe<string>("catalog.message", message => delivery.TrySetResult(message));
            for (var attempt = 0; attempt < 20 && !delivery.Task.IsCompleted; attempt++)
            {
                if (!publisher.Publish("catalog.message", text, out var error)) throw new InvalidOperationException(error);
                await Task.Delay(50);
            }
            var result = await delivery.Task.WaitAsync(TimeSpan.FromSeconds(5));
            return $"Subject = catalog.message\nReceived = {result}";
        }
        finally { publisher.Disconnect(); subscriber.Disconnect(); broker.Stop(); }
    }

    private static async Task<string> UdpDelivery(string group)
    {
        var server = new UdpSocketServer();
        var client = new UdpSocketClient();
        int port;
        using (var probe = new UdpClient(0)) port = ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
        const string payload = "Hello UDP";
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Received += (_, command) =>
        {
            if (command.IsCommand<SamplePacket>())
                received.TrySetResult(command.GetCommand<SamplePacket>().Title);
        };
        try
        {
            var started = server.Start("catalog", 1, group.Trim(), port, "127.0.0.1");
            if (!started.IsSuccess) throw new InvalidOperationException(started.ErrorMessage);
            await client.ConnectAsync("catalog", group.Trim(), port, "127.0.0.1", 1).WaitAsync(TimeSpan.FromSeconds(5));
            if (!client.IsRunning) throw new InvalidOperationException("UDP receiver did not start.");
            for (var attempt = 0; attempt < 10 && !received.Task.IsCompleted; attempt++)
            {
                await server.SendCommandAsync(new SamplePacket { Title = payload }, DateTimeOffset.UtcNow);
                await Task.Delay(50);
            }
            var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
            if (result != payload) throw new InvalidOperationException("UDP payload changed.");
            return $"Group = {group}:{port}\nSent = {payload}\nReceived = {result}";
        }
        finally { client.Stop(); server.Stop(); }
    }

    private static async Task<string> FileSystemClient(string _)
    {
        var server = new TcpSocketServer();
        var client = new TcpSocketClient();
        var root = TemporaryDirectory();
        File.WriteAllText(Path.Combine(root, "sample.txt"), "catalog");
        using var serverFeature = new TcpSocketServerFileSystemFeature(server) { RootDirectory = root };
        using var clientFeature = new TcpSocketClientFileSystemFeature(client);
        var response = new TaskCompletionSource<BrowseFileSystemResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = client.RegisterCommandHandler(command =>
        {
            if (!command.IsCommand<BrowseFileSystemResponse>()) return Task.FromResult(false);
            response.TrySetResult(command.GetCommand<BrowseFileSystemResponse>());
            return Task.FromResult(true);
        });
        try
        {
            var started = await server.StartAsync("catalog-files", "127.0.0.1", 0);
            if (!started.IsSuccess) throw new InvalidOperationException(started.ErrorMessage);
            var port = ((IPEndPoint)server.Server!.LocalEndPoint!).Port;
            var connected = await client.ConnectAsync("catalog-files", "127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
            if (!connected.IsSuccess) throw new InvalidOperationException(connected.ErrorMessage);
            await clientFeature.BrowseFileSystemAsync(root);
            var result = await response.Task.WaitAsync(TimeSpan.FromSeconds(5));
            if (result.TotalCount != 1 || result.Entries?.Single().Name != "sample.txt")
                throw new InvalidOperationException("Remote directory response did not match its root.");
            return $"Root = {root}\nTaskId = {result.TaskId}\nTotalCount = {result.TotalCount}\nName = {result.Entries.Single().Name}";
        }
        finally { await client.StopAsync(); await server.StopAsync(); }
    }
}
