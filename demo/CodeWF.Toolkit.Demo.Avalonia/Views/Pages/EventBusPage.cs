using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>事件总线页：真实的进程内总线（订阅 / 发布 / 查询 / 退订）。</summary>
public sealed class EventBusPage : DemoPageBase
{
    private const string XamlSnippet = """
        <!-- Core 页与总线页均为代码构建；总线演示只需引用 EventBus 包 -->
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.EventBus" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.EventBus;

        var bus = new EventBus();

        // 1) lambda 订阅
        Action<GreetCommand> first = cmd => Console.WriteLine($"[lambda] {cmd.Name}");
        bus.Subscribe(first);

        // 2) 扫描式订阅：[EventHandler] 标记的私有方法，按 Order 顺序执行
        bus.Subscribe(new TimeQueryHandler());
        bus.Subscribe<GreetCommand>(cmd => Console.WriteLine($"[lambda2] {cmd.Name}"));

        // 发布（同步 / 异步）
        bus.Publish(new GreetCommand("CodeWF"));
        await bus.PublishAsync(new GreetCommand("Toolkit"));

        // 查询：处理器把结果写回 Query.Result
        var query = new TimeQuery();
        bus.Query(query);
        Console.WriteLine($"服务器时间：{query.Result:HH:mm:ss.fff}");

        // 退订：句柄、类型、宿主对象三种粒度
        bus.Unsubscribe(first);
        bus.Unsubscribe<TimeQueryHandler>();
        bus.Unsubscribe(handler);

        // 程序集扫描：一次注册整程序集里的全部处理器
        bus.Subscribe([typeof(TimeQueryHandler).Assembly]);
        """;

    private readonly CodeWF.Toolkit.EventBus.EventBus _bus = new();
    private readonly SelectableTextBlock _feed;
    private readonly TextBox _message;
    private readonly Button _unsubscribeButton;
    private readonly TextBlock _subscriptionState;
    private readonly Action<GreetCommand> _firstSubscription;
    private readonly Action<GreetCommand> _secondSubscription;

    private bool _secondSubscriptionActive = true;

    public EventBusPage()
    {
        _firstSubscription = cmd => Append($"[lambda] GreetCommand → {cmd.Name}");
        _secondSubscription = cmd => Append($"[lambda2/Order=1] GreetCommand → {cmd.Name}");
        _bus.Subscribe(_firstSubscription);
        _bus.Subscribe(_secondSubscription);
        _bus.Subscribe(new TimeQueryHandler());

        _feed = Ui.Code("总线就绪：2 个 GreetCommand 订阅、1 个 TimeQuery 处理器已注册", output: true);
        _message = Ui.Input("hello");
        _unsubscribeButton = Ui.Button("取消第二条订阅", ToggleSecondSubscription);
        _subscriptionState = Ui.Key("已订阅");

        var session = Ui.Card("会话（真实总线）",
            _feed,
            Ui.Row(_message, Ui.Button("Publish(GreetCommand)", Publish, primary: true)),
            Ui.Row(
                Ui.Button("Query(TimeQuery)", Query),
                Ui.Button("PublishAsync(GreetCommand)", () => _ = PublishAsync()),
                Ui.Button("清空", ClearFeed)),
            Ui.Hint("处理器抛异常不会被静默吞掉；异步发布走 PublishAsync，订阅方按注册顺序执行。"));

        var subscriptions = Ui.Card("订阅表",
            Ui.Code("""
                GreetCommand   lambda               已注册
                GreetCommand   [EventHandler]       已注册（Order 靠后）
                TimeQuery      [EventHandler]       已注册（查询回写）
                """, output: true),
            Ui.Row(_unsubscribeButton, Ui.Key("第二条 GreetCommand 订阅："), _subscriptionState),
            Ui.Code("""
                public sealed class TimeQueryHandler
                {
                    [EventHandler]
                    private void ReceiveTimeQuery(TimeQuery query) => query.Result = DateTime.Now;
                }
                """));

        var registration = Ui.Card("注册方式",
            Ui.Code("""
                EventBus                      手动 Subscribe / Publish，零依赖
                EventBus.Prism                Prism 容器注册、扫描与启动
                EventBus.AspNetCore           DI 注册 + 请求管线启动，处理器自动发现
                EventBusRegistrationExtensions 通用 IOC：AddEventBus(...) + UseEventBus(resolve, assemblies)
                EventBus.Socket               EventServer / EventClient，跨进程跨机
                """));

        Content = Ui.Page(
            Ui.Heading("进程内事件总线"),
            Ui.Sub("发布订阅 + Command/Query。下面的总线是真实运行的：订阅、发布、查询、退订都在本地执行。"),
            Ui.TwoColumns(session, subscriptions),
            registration);

        _message.Width = 200;
    }

    public override string TitleKey => DemoKeys.ItemBus;

    public override string DescriptionKey => DemoKeys.DescBus;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("bus.core", "总线实例", "演示页持有的真实 EventBus 实例。",
            DemoProperty.ReadOnly("bus.name", "总线实例", "EventBus"),
            DemoProperty.Bool("bus.async", "异步发布", false, "开启后 Publish 走 PublishAsync。"),
            DemoProperty.Bool("bus.scan", "程序集扫描", false, "按程序集注册 [EventHandler]。"),
            DemoProperty.Number("bus.order", "处理器顺序", 1, 0, 100, 1, null, "同命令多处理器时的执行顺序。")));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "bus.async" && DemoProperty.Flag(change))
        {
            _ = PublishAsync();
        }
        else if (change.Item.Id == "bus.scan" && DemoProperty.Flag(change))
        {
            _bus.Subscribe([typeof(TimeQueryHandler).Assembly]);
            Append("程序集扫描：已注册 TimeQueryHandler");
            Logger.Info("事件总线：程序集扫描注册处理器");
        }
    }

    private void Publish()
    {
        var name = _message.Text ?? string.Empty;
        Append($"Publish(new GreetCommand(\"{name}\"))");
        _bus.Publish(new GreetCommand(name));
        Logger.Info($"事件总线发布：GreetCommand(\"{name}\")");
    }

    private async Task PublishAsync()
    {
        var name = _message.Text ?? string.Empty;
        Append($"PublishAsync(new GreetCommand(\"{name}\"))");
        await _bus.PublishAsync(new GreetCommand(name));
        Logger.Info($"事件总线异步发布：GreetCommand(\"{name}\")");
    }

    private void Query()
    {
        var query = new TimeQuery();
        _bus.Query(query);
        Append($"[查询] TimeQuery.Result = {query.Result:HH:mm:ss.fff}");
        Logger.Info("事件总线查询：TimeQuery");
    }

    private void ToggleSecondSubscription()
    {
        _secondSubscriptionActive = !_secondSubscriptionActive;
        if (_secondSubscriptionActive)
        {
            _bus.Subscribe(_secondSubscription);
            _unsubscribeButton.Content = "取消第二条订阅";
            _subscriptionState.Text = "已订阅";
        }
        else
        {
            _bus.Unsubscribe(_secondSubscription);
            _unsubscribeButton.Content = "恢复第二条订阅";
            _subscriptionState.Text = "已取消";
        }

        Logger.Info($"事件总线第二条订阅：{(_secondSubscriptionActive ? "启用" : "取消")}");
    }

    private void ClearFeed()
    {
        _feed.Text = "会话已清空";
        Logger.Info("事件总线会话已清空");
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current == "会话已清空" ? line : current + "\n" + line;
    }
}
