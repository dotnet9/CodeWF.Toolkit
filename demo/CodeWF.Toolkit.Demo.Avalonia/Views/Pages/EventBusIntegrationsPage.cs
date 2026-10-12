using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.EventBus.Socket;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>总线集成页：Prism / ASP.NET Core 注册形态讲解 + 真实的 Socket 分布式总线互通。</summary>
public sealed class EventBusIntegrationsPage : DemoPageBase
{
    private const string XamlSnippet = """
        <!-- 分布式总线只需引用 Socket 包 -->
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.EventBus.Socket" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        // Prism（桌面）
        containerRegistry.RegisterEventBus(assemblies);   // 或 Container.UseEventBus();
        // ASP.NET Core
        builder.Services.AddEventBus(assemblies); app.UseEventBus();
        // 通用 IOC
        services.AddEventBus(addSingleton, addScoped, assemblies); Container.UseEventBus(resolve, assemblies);

        // Socket 分布式总线：跨进程 / 跨机
        var server = new EventServer();
        server.Start("127.0.0.1", 39561);

        var client = new EventClient();
        await client.ConnectAsync("127.0.0.1", 39561);

        client.Subscribe<string>("demo.greeting", msg => Console.WriteLine(msg));
        client.Publish("demo.greeting", "hello", out var error);

        var (result, error2) = await client.QueryAsync<Ping, Pong>(new Ping { Value = 1 });
        client.Disconnect();
        server.Stop();
        """;

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
        _host = Ui.Input("127.0.0.1");
        _port = Ui.Input("39561");
        _heartbeat = Ui.Input("10");
        _serverState = Ui.Key("● 已停止");
        _clientState = Ui.Key("● 未连接");
        _feed = Ui.Code("等待启动服务端…", output: true);
        _topic = Ui.Input("demo.greeting");
        _payload = Ui.Input("hello");

        _subscriberHandler = message => Append($"[客户端订阅] {message}");

        _startButton = Ui.Button("启动服务端 + 客户端", async () => await StartAsync(), primary: true);
        _stopButton = Ui.Button("停止", async () => await StopAsync());
        _publishButton = Ui.Button("Publish", Publish, primary: true);

        var socket = Ui.Card("Socket 分布式总线 EventServer / EventClient（真实互通）",
            Ui.TwoColumns(
                Ui.Card("服务端 / 客户端",
                    Ui.Field("监听地址", _host),
                    Ui.Field("端口", _port),
                    Ui.Field("心跳(秒)", _heartbeat),
                    Ui.Row(_startButton, _stopButton),
                    Ui.Field("服务端", _serverState),
                    Ui.Field("客户端", _clientState),
                    Ui.Hint("心跳保活、断线重连、离线消息补发；底层传输由 NetWrapper 承担。")),
                Ui.Card("发布 / 订阅",
                    Ui.Field("主题", _topic),
                    Ui.Field("内容", _payload),
                    Ui.Row(_publishButton, Ui.Button("清空会话", () => _feed.Text = string.Empty)),
                    _feed)),
            Ui.Code("""
                var server = new EventServer(); server.Start("127.0.0.1", 39561);
                var client = new EventClient(); await client.ConnectAsync("127.0.0.1", 39561);
                client.Subscribe<string>("demo.greeting", msg => Console.WriteLine(msg));
                client.Publish("demo.greeting", "hello", out var error);
                var (result, error2) = await client.QueryAsync<Ping, Pong>(ping);
                """));

        var prism = Ui.Card("Prism",
            Ui.Code("""
                containerRegistry.RegisterEventBus(assemblies);
                // 或在 PrismApplication 中
                Container.UseEventBus();
                """),
            Ui.Hint("适合 Prism.DryIoc.Avalonia 桌面应用：容器内注册处理器并随模块启动。"));

        var aspnet = Ui.Card("ASP.NET Core",
            Ui.Code("""
                builder.Services.AddEventBus(assemblies);
                app.UseEventBus();
                """),
            Ui.Hint("处理器自动发现，随请求管线启动。"));

        Content = Ui.Page(
            Ui.Heading("总线集成形态"),
            Ui.Sub("同一套处理器模型，三种宿主形态；Socket 形态用于跨进程 / 跨机，下方可真实启动服务端与客户端互通。"),
            Ui.TwoColumns(prism, aspnet),
            socket);

        _host.Width = 150;
        _port.Width = 110;
        _heartbeat.Width = 110;
        _topic.Width = 200;
        _payload.Width = 200;
    }

    public override string TitleKey => DemoKeys.ItemBusIntegrations;

    public override string DescriptionKey => DemoKeys.DescBusIntegrations;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("busint.host", "宿主形态", "分布式总线的连接与保活参数。",
            DemoProperty.Choice("busint.mode", "宿主形态", "Socket", ["进程内", "Prism", "ASP.NET Core", "Socket"]),
            DemoProperty.Number("busint.port", "端口", 39561, 1024, 65535, 1, null, "EventServer 监听端口。"),
            DemoProperty.Number("busint.heartbeat", "心跳(秒)", 10, 1, 120, 1, "s", "客户端心跳间隔。"),
            DemoProperty.Bool("busint.resend", "离线补发", true, "客户端离线期间的消息在重连后补发。")));

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
            _serverState.Text = $"● 运行中（{host}:{port}）";
            Append($"EventServer.Start(\"{host}\", {port})");

            _client = new EventClient();
            var connected = await _client.ConnectAsync(host, port).WaitAsync(TimeSpan.FromSeconds(5));
            _clientState.Text = connected ? "● 已连接" : "● 连接失败";
            Append(connected ? $"EventClient.ConnectAsync → 已连接 {host}:{port}" : "EventClient.ConnectAsync → 失败");
            if (!connected)
            {
                await StopAsync();
                return;
            }

            _client.Subscribe("demo.greeting", _subscriberHandler);
            _subscribed = true;
            Append("client.Subscribe<string>(\"demo.greeting\", …) → 已订阅");
            Logger.Info($"Socket 总线已启动：{host}:{port}");
        }
        catch (Exception ex)
        {
            Append($"启动失败：{ex.Message}");
            Logger.Error("Socket 总线启动失败", ex);
            await StopAsync();
        }
        finally
        {
            _startButton.IsEnabled = true;
        }
    }

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

        _serverState.Text = "● 已停止";
        _clientState.Text = "● 未连接";
        Append("EventServer.Stop() / EventClient.Disconnect()");
        Logger.Warn("Socket 总线已停止");
        await Task.CompletedTask;
    }

    private void Publish()
    {
        if (_client is null || !_subscribed)
        {
            Append("请先启动服务端与客户端。");
            return;
        }

        var topic = string.IsNullOrWhiteSpace(_topic.Text) ? "demo.greeting" : _topic.Text!;
        var payload = _payload.Text ?? string.Empty;
        var ok = _client.Publish(topic, payload, out var error);
        Append(ok
            ? $"client.Publish(\"{topic}\", \"{payload}\") → 已投递"
            : $"client.Publish 失败：{error}");
        Logger.Info($"Socket 总线发布：{topic} = {payload}");
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith("等待启动", StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
