using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.EventBus.Socket;
using CodeWF.Toolkit.NetWrapper.Helpers;
using ReactiveUI;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

/// <summary>使用真实服务端和客户端 API 的本地协议会话演示。</summary>
public sealed class TransportDemoViewModel : ReactiveObject
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private TcpSocketServer? _tcpServer;
    private TcpSocketClient? _tcpClient;
    private UdpSocketServer? _udpServer;
    private UdpSocketClient? _udpClient;
    private EventServer? _eventServer;
    private EventClient? _publisher, _subscriber;
    private IDisposable? _registration;
    private readonly Action<string> _handler;
    private TaskCompletionSource<string>? _received;
    private string _input = "Hello CodeWF", _output = "";
    private bool _running, _busy, _subscribed;
    private int _receivedCount;
    private const string Subject = "demo.message";

    public TransportDemoViewModel(string api)
    {
        Api = api;
        _handler = message => _received?.TrySetResult(message);
        StartCommand = ReactiveCommand.CreateFromTask(StartAsync);
        StopCommand = ReactiveCommand.CreateFromTask(StopAsync);
        SendCommand = ReactiveCommand.CreateFromTask(SendAsync);
        SubscriptionCommand = ReactiveCommand.Create(ToggleSubscription);
    }
    public string Api { get; }
    public bool IsEvent => Api is "EventServer" or "EventClient";
    public bool IsUdp => Api.StartsWith("Udp", StringComparison.Ordinal);
    public bool IsRunning { get => _running; private set { this.RaiseAndSetIfChanged(ref _running, value); Changed(); } }
    public bool IsBusy { get => _busy; private set { this.RaiseAndSetIfChanged(ref _busy, value); Changed(); } }
    public bool IsSubscribed { get => _subscribed; private set => this.RaiseAndSetIfChanged(ref _subscribed, value); }
    public bool CanStart => !IsRunning && !IsBusy;
    public bool CanOperate => IsRunning && !IsBusy;
    public bool Succeeded { get; private set; }
    private PropertyItem? _inputProperty;
    public string Input
    {
        get => _input;
        set { this.RaiseAndSetIfChanged(ref _input, value); if (_inputProperty is not null) _inputProperty.Value = value; }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        _inputProperty = new PropertyItem { Id = nameof(Input), Name = I18nManager.Instance.GetResource(DemoResources.Catalog.Input),
            Kind = PropertyItemKind.MultilineText, Value = Input, DefaultValue = Input };
        return [_inputProperty];
    }

    public void Commit(PropertyValueChange change)
    {
        if (change.Item.Id == nameof(Input)) Input = change.NewValue?.ToString() ?? "";
    }

    public string Output { get => _output; private set => this.RaiseAndSetIfChanged(ref _output, value); }
    public int ReceivedCount { get => _receivedCount; private set => this.RaiseAndSetIfChanged(ref _receivedCount, value); }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand SubscriptionCommand { get; }
    private void Changed() { this.RaisePropertyChanged(nameof(CanStart)); this.RaisePropertyChanged(nameof(CanOperate)); }

    public Task StartAsync() => ExecuteAsync(async () =>
    {
        if (IsRunning) return;
        try
        {
            if (IsEvent)
            {
                _eventServer = new(); _publisher = new(); _subscriber = new();
                using var reservation = new TcpListener(IPAddress.Loopback, 0);
                reservation.Start(); var port = ((IPEndPoint)reservation.LocalEndpoint).Port; reservation.Stop();
                await _eventServer.StartAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
                if (!await _publisher.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5))
                    || !await _subscriber.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5)))
                    throw new IOException("Event peer did not connect.");
                _subscriber.Subscribe(Subject, _handler); IsSubscribed = true;
                Output = $"127.0.0.1:{port}\nSubject = {Subject}";
            }
            else if (IsUdp)
            {
                _udpServer = new(); _udpClient = new();
                using var reservation = new UdpClient(0);
                var port = ((IPEndPoint)reservation.Client.LocalEndPoint!).Port;
                reservation.Close();
                _udpClient.Received += (_, command) => { if (command.IsCommand<SamplePacket>()) _handler(command.GetCommand<SamplePacket>().Title); };
                var started = _udpServer.Start("udp-demo", 1, "239.1.1.1", port, "127.0.0.1");
                if (!started.IsSuccess) throw new IOException(started.ErrorMessage);
                await _udpClient.ConnectAsync("udp-demo", "239.1.1.1", port, "127.0.0.1", 1).WaitAsync(TimeSpan.FromSeconds(5));
                if (!_udpClient.IsRunning) throw new IOException("UDP receiver did not start.");
                Output = $"239.1.1.1:{port}\n127.0.0.1";
            }
            else
            {
                _tcpServer = new(); _tcpClient = new();
                _registration = _tcpServer.RegisterCommandHandler((_, _, command) =>
                {
                    if (!command.IsCommand<SamplePacket>()) return Task.FromResult(false);
                    _handler(command.GetCommand<SamplePacket>().Title); return Task.FromResult(true);
                });
                var started = await _tcpServer.StartAsync("tcp-demo", "127.0.0.1", 0);
                if (!started.IsSuccess) throw new IOException(started.ErrorMessage);
                var port = ((IPEndPoint)_tcpServer.Server!.LocalEndPoint!).Port;
                var connected = await _tcpClient.ConnectAsync("tcp-demo", "127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
                if (!connected.IsSuccess) throw new IOException(connected.ErrorMessage);
                Output = $"127.0.0.1:{port}";
            }
            IsRunning = true;
            Output += "\n" + I18nManager.Instance.GetResource(DemoResources.Catalog.TransportStarted);
        }
        catch { await StopCoreAsync(); throw; }
    });

    public Task SendAsync() => ExecuteAsync(async () =>
    {
        if (!IsRunning) throw new InvalidOperationException(I18nManager.Instance.GetResource(DemoResources.Catalog.FileStartFirst));
        _received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var payload = Input;
        if (IsEvent && !IsSubscribed)
        {
            if (!_publisher!.Publish(Subject, payload, out var error)) throw new IOException(error);
            Output += "\n" + I18nManager.Instance.GetResource(DemoResources.Catalog.TransportNoSubscription);
            return;
        }
        for (var attempt = 0; attempt < (IsEvent || IsUdp ? 20 : 1) && !_received.Task.IsCompleted; attempt++)
        {
            if (IsEvent) { if (!_publisher!.Publish(Subject, payload, out var error)) throw new IOException(error); }
            else if (IsUdp) await _udpServer!.SendCommandAsync(new SamplePacket { Title = payload }, DateTimeOffset.UtcNow);
            else await _tcpClient!.SendCommandAsync(new SamplePacket { Title = payload });
            if (IsEvent || IsUdp) await Task.Delay(50);
        }
        var received = await _received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (received != payload) throw new InvalidDataException("Transport payload changed.");
        ReceivedCount++;
        Output += $"\nSent = {payload}\nReceived = {received}";
    });
    public void ToggleSubscription()
    {
        if (!CanOperate || !IsEvent) return;
        if (IsSubscribed) _subscriber!.Unsubscribe(Subject, _handler);
        else _subscriber!.Subscribe(Subject, _handler);
        IsSubscribed = !IsSubscribed;
        Output += $"\nSubscribe = {IsSubscribed}";
    }
    public Task StopAsync() => ExecuteAsync(StopCoreAsync);
    private async Task StopCoreAsync()
    {
        _publisher?.Disconnect(); _subscriber?.Disconnect(); _eventServer?.Stop();
        _publisher?.Dispose(); _subscriber?.Dispose(); _eventServer?.Dispose();
        _publisher = null; _subscriber = null; _eventServer = null;
        _udpClient?.Stop(); _udpServer?.Stop(); _udpClient = null; _udpServer = null;
        if (_tcpClient is not null) await _tcpClient.StopAsync();
        if (_tcpServer is not null) await _tcpServer.StopAsync();
        _registration?.Dispose(); _registration = null; _tcpClient = null; _tcpServer = null;
        IsRunning = false; IsSubscribed = false;
        Output += "\n" + I18nManager.Instance.GetResource(DemoResources.Catalog.FileStopped);
    }
    private async Task ExecuteAsync(Func<Task> action)
    {
        await _gate.WaitAsync(); IsBusy = true; Succeeded = false;
        try { await action(); Succeeded = true; }
        catch (Exception ex) { Output += "\n" + string.Format(I18nManager.Instance.GetResource(DemoResources.Catalog.Failed), ex.Message); }
        finally { IsBusy = false; _gate.Release(); }
    }
}
