using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using ReactiveUI;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.EventBus.Socket;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>总线集成页：Prism / ASP.NET Core 注册形态讲解 + 真实的 Socket 分布式总线互通。</summary>
public sealed class EventBusIntegrationsPage : DemoPageBase
{
    private static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Source.Xaml);

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Source.CSharp);

    private readonly TextBox _host;
    private readonly TextBox _port;
    private readonly TextBox _heartbeat;
    private readonly TextBlock _serverState;
    private readonly TextBlock _clientState;
    private readonly SelectableTextBlock _feed;
    private readonly TextBox _topic;
    private readonly TextBox _payload;
    private readonly Button _startButton;
    private readonly Button _stopButton;
    private readonly Button _publishButton;

    private readonly Action<string> _subscriberHandler;

    private EventServer? _server;
    private EventClient? _client;
    private bool _subscribed;

    public EventBusIntegrationsPage()
    {
        var registration = new BusRegistrationViewModel();
        var prismOutput = Ui.Code(string.Empty, output: true);
        prismOutput.Bind(SelectableTextBlock.TextProperty, registration.WhenAnyValue(model => model.PrismOutput));
        var aspNetOutput = Ui.Code(string.Empty, output: true);
        aspNetOutput.Bind(SelectableTextBlock.TextProperty, registration.WhenAnyValue(model => model.AspNetOutput));
        var prismButton = new Button { Command = registration.PrismCommand };
        prismButton.Bind(ContentControl.ContentProperty, new global::CodeWF.Avalonia.Lang.MarkupExtensions.I18nExtension(DemoResources.Studio.IntegrationRun).CreateBinding());
        var aspNetButton = new Button { Command = registration.AspNetCommand };
        aspNetButton.Bind(ContentControl.ContentProperty, new global::CodeWF.Avalonia.Lang.MarkupExtensions.I18nExtension(DemoResources.Studio.IntegrationRun).CreateBinding());
        _host = Ui.Input("127.0.0.1");
        _port = Ui.Input("39561");
        _heartbeat = Ui.Input("10");
        _serverState = Ui.Key("@Demo.Content.EventBusIntegrations.Content.Stopped");
        _clientState = Ui.Key("@Demo.Content.EventBusIntegrations.Content.Disconnected");
        _feed = Ui.Code("@Demo.Content.EventBusIntegrations.Content.AwaitingServerStartup", output: true);
        _topic = Ui.Input("demo.greeting");
        _payload = Ui.Input("hello");

        _subscriberHandler = message => Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Content.ClientSubscriptionFormat), message));

        _startButton = Ui.Button("@Demo.Content.EventBusIntegrations.Content.StartServerAndClient", async () => await StartAsync(), primary: true);
        _stopButton = Ui.Button("@Demo.Content.EventBusIntegrations.Content.Stop", async () => await StopAsync());
        _publishButton = Ui.Button("Publish", Publish, primary: true);

        var socket = Ui.Card("@Demo.Content.EventBusIntegrations.socket.DistributedSocketBusEventServerEventClientRealConnection",
            Ui.TwoColumns(
                Ui.Card("@Demo.Content.EventBusIntegrations.socket.ServerClient",
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.ListenAddress), _host),
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.Port), _port),
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.HeartbeatSeconds), _heartbeat),
                    Ui.Row(_startButton, _stopButton),
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.Server), _serverState),
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.Client), _clientState),
                    Ui.Hint("@Demo.Content.EventBusIntegrations.socket.HeartbeatReconnectionAndOfflineReplayNetWrapperProvidesThe")),
                Ui.Card("@Demo.Content.EventBusIntegrations.socket.PublishSubscribe",
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.Topic), _topic),
                    Ui.Field(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.socket.Content), _payload),
                    Ui.Row(_publishButton, Ui.Button("@Demo.Content.EventBusIntegrations.socket.ClearSession", () => _feed.Text = string.Empty)),
                    _feed)),
            Ui.Code("""
                var server = new EventServer(); server.Start("127.0.0.1", 39561);
                var client = new EventClient(); await client.ConnectAsync("127.0.0.1", 39561);
                client.Subscribe<string>("demo.greeting", msg => Console.WriteLine(msg));
                client.Publish("demo.greeting", "hello", out var error);
                var (result, error2) = await client.QueryAsync<Ping, Pong>(ping);
                """));

        var prism = Ui.Card("Prism",
            Ui.Code("@Demo.Content.EventBusIntegrations.prism.UsingCodeWFToolkitEventBusPrismContainerRegistryAddEventBusAssemblies"),
            prismButton, prismOutput,
            Ui.Hint("@Demo.Content.EventBusIntegrations.prism.ForPrismDryIocAvaloniaRegisterHandlersInThe"));

        var aspnet = Ui.Card("ASP.NET Core",
            Ui.Code("""
                builder.Services.AddEventBus(assemblies);
                app.UseEventBus();
                """),
            aspNetButton, aspNetOutput,
            Ui.Hint("@Demo.Content.EventBusIntegrations.aspnet.AutomaticallyDiscoverHandlersAndInitializeThemWithThe"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.EventBusIntegrations.Content.BusIntegrations"),
            Ui.Sub("@Demo.Content.EventBusIntegrations.Content.OneHandlerModelForThreeHostIntegrationsSocket"),
            Ui.TwoColumns(prism, aspnet),
            socket);

        _host.Width = 150;
        _port.Width = 110;
        _heartbeat.Width = 110;
        _topic.Width = 200;
        _payload.Width = 200;
    }

    public override string TitleKey => DemoKeys.BusIntegrationsExampleTitle;

    public override string DescriptionKey => DemoKeys.DescBusIntegrations;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("busint.host", L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.HostIntegration), L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.ConnectionAndKeepAliveSettingsForTheDistributed),
            DemoProperty.Choice("busint.mode", L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.HostIntegration), "Socket", [L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.InProcess), "Prism", "ASP.NET Core", "Socket"]),
            DemoProperty.Number("busint.port", L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.Port), 39561, 1024, 65535, 1, null, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.EventServerListenPort)),
            DemoProperty.Number("busint.heartbeat", L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.HeartbeatSeconds), 10, 1, 120, 1, "s", L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.ClientHeartbeatInterval)),
            DemoProperty.Bool("busint.resend", L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.ReplayOfflineMessages), true, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Properties.OfflineClientMessagesAreReplayedAfterReconnection))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "busint.port")
        {
            _port.Text = ((int)DemoProperty.Num(change, 39561)).ToString();
        }
        else if (change.Item.Id == "busint.heartbeat")
        {
            _heartbeat.Text = ((int)DemoProperty.Num(change, 10)).ToString();
        }
    }

    private async Task StartAsync()
    {
        if (_server is not null)
        {
            return;
        }

        var port = int.TryParse(_port.Text, out var parsed) ? parsed : 39561;
        var host = string.IsNullOrWhiteSpace(_host.Text) ? "127.0.0.1" : _host.Text!;

        _startButton.IsEnabled = false;
        try
        {
            _server = new EventServer();
            _server.Start(host, port);
            _serverState.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.RunningFormat), host, port);
            Append($"EventServer.Start(\"{host}\", {port})");

            _client = new EventClient();
            var connected = await _client.ConnectAsync(host, port).WaitAsync(TimeSpan.FromSeconds(5));
            _clientState.Text = connected ? L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.Connected) : L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.ConnectionFailed);
            Append(connected ? string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.EventClientConnectAsyncConnectedToFormat), host, port) : L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.EventClientConnectAsyncFailed));
            if (!connected)
            {
                await StopAsync();
                return;
            }

            _client.Subscribe("demo.greeting", _subscriberHandler);
            _subscribed = true;
            Append(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.ClientSubscribeStringDemoGreetingSubscribed));
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.SocketBusStartedFormat), host, port));
        }
        catch (Exception ex)
        {
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.StartupFailedFormat), ex.Message));
            Logger.Error(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StartAsync.SocketBusStartupFailed), ex);
            await StopAsync();
        }
        finally
        {
            _startButton.IsEnabled = true;
        }
    }

    public override Task OnDocumentClosedAsync() => StopAsync();

    private async Task StopAsync()
    {
        if (_client is not null)
        {
            if (_subscribed)
            {
                _client.Unsubscribe("demo.greeting", _subscriberHandler);
                _subscribed = false;
            }

            _client.Disconnect();
            _client.Dispose();
            _client = null;
        }

        if (_server is not null)
        {
            _server.Stop();
            _server.Dispose();
            _server = null;
        }

        _serverState.Text = L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StopAsync.Stopped);
        _clientState.Text = L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StopAsync.Disconnected);
        Append("EventServer.Stop() / EventClient.Disconnect()");
        Logger.Warn(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.StopAsync.SocketBusStopped));
        await Task.CompletedTask;
    }

    private void Publish()
    {
        if (_client is null || !_subscribed)
        {
            Append(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Publish.StartTheServerAndClientFirst));
            return;
        }

        var topic = string.IsNullOrWhiteSpace(_topic.Text) ? "demo.greeting" : _topic.Text!;
        var payload = _payload.Text ?? string.Empty;
        var ok = _client.Publish(topic, payload, out var error);
        Append(ok
            ? string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Publish.ClientPublishDeliveredFormat), topic, payload)
            : string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Publish.ClientPublishFailedFormat), error));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Publish.SocketBusPublishFormat), topic, payload));
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource(DemoResources.Content.EventBusIntegrations.Append.AwaitingStartup), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
