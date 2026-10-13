using System.Net;
using System.Net.Sockets;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.EventBus.Socket;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWrapper.Helpers;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

public partial class MainViewModel
{
    private static async Task SmokeLocalTransportsAsync(CancellationToken cancellation)
    {
        var server = new TcpSocketServer();
        var client = new TcpSocketClient();
        var received = new TaskCompletionSource<SamplePacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = server.RegisterCommandHandler((_, _, command) =>
        {
            if (!command.IsCommand<SamplePacket>()) return Task.FromResult(false);
            received.TrySetResult(command.GetCommand<SamplePacket>());
            return Task.FromResult(true);
        });
        try
        {
            var started = await server.StartAsync("studio-smoke", "127.0.0.1", 0);
            if (!started.IsSuccess) throw new InvalidOperationException(started.ErrorMessage);
            var port = ((IPEndPoint)server.Server!.LocalEndPoint!).Port;
            var connected = await client.ConnectAsync("studio-smoke", "127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5), cancellation);
            if (!connected.IsSuccess) throw new InvalidOperationException(connected.ErrorMessage);
            await client.SendCommandAsync(new SamplePacket { Id = 42, Score = 1.5f, Title = "studio-smoke" }, cancellation);
            var packet = await received.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellation);
            if (packet.Id != 42 || packet.Score != 1.5f || packet.Title != "studio-smoke")
                throw new InvalidOperationException("TCP packet payload changed.");
        }
        finally
        {
            await client.StopAsync();
            await server.StopAsync();
        }

        using var portReservation = new TcpListener(IPAddress.Loopback, 0);
        portReservation.Start();
        var eventPort = ((IPEndPoint)portReservation.LocalEndpoint).Port;
        portReservation.Stop();
        using var broker = new EventServer();
        using var publisher = new EventClient();
        using var subscriber = new EventClient();
        var delivery = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            await broker.StartAsync("127.0.0.1", eventPort, cancellation).WaitAsync(TimeSpan.FromSeconds(5), cancellation);
            if (!await subscriber.ConnectAsync("127.0.0.1", eventPort).WaitAsync(TimeSpan.FromSeconds(5), cancellation)
                || !await publisher.ConnectAsync("127.0.0.1", eventPort).WaitAsync(TimeSpan.FromSeconds(5), cancellation))
                throw new InvalidOperationException("Socket event bus did not connect.");
            subscriber.Subscribe<string>("studio.smoke", message => delivery.TrySetResult(message));
            for (var attempt = 0; attempt < 20 && !delivery.Task.IsCompleted; attempt++)
            {
                if (!publisher.Publish("studio.smoke", "studio-delivery", out var error))
                    throw new InvalidOperationException(error);
                await Task.Delay(50, cancellation);
            }
            if (await delivery.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellation) != "studio-delivery")
                throw new InvalidOperationException("Socket event bus payload changed.");
        }
        finally
        {
            publisher.Disconnect();
            subscriber.Disconnect();
            broker.Stop();
        }
        Logger.Info("Transport smoke: NetWrapper TCP packet and Socket event bus delivery passed on loopback");
    }
}
