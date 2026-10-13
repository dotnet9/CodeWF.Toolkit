using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>事件总线页：真实的进程内总线（订阅 / 发布 / 查询 / 退订）。</summary>
public sealed class EventBusPage : DemoPageBase
{
    private static string XamlSnippet => L.Instance.GetResource("Demo.Text.EventBusPage.XamlSnippet001");

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.EventBusPage.CSharpSnippet002");

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

        _feed = Ui.Code("@Demo.Text.EventBusPage.Content003", output: true);
        _message = Ui.Input("hello");
        _unsubscribeButton = Ui.Button("@Demo.Text.EventBusPage.Content004", ToggleSecondSubscription);
        _subscriptionState = Ui.Key("@Demo.Text.EventBusPage.Content005");

        var session = Ui.Card("@Demo.Text.EventBusPage.session006",
            _feed,
            Ui.Row(_message, Ui.Button("Publish(GreetCommand)", Publish, primary: true)),
            Ui.Row(
                Ui.Button("Query(TimeQuery)", Query),
                Ui.Button("PublishAsync(GreetCommand)", () => _ = PublishAsync()),
                Ui.Button("@Demo.Text.EventBusPage.session007", ClearFeed)),
            Ui.Hint("@Demo.Text.EventBusPage.session008"));

        var subscriptions = Ui.Card("@Demo.Text.EventBusPage.subscriptions009",
            Ui.Code("@Demo.Text.EventBusPage.subscriptions010", output: true),
            Ui.Row(_unsubscribeButton, Ui.Key("@Demo.Text.EventBusPage.subscriptions011"), _subscriptionState),
            Ui.Code("""
                public sealed class TimeQueryHandler
                {
                    [EventHandler]
                    private void ReceiveTimeQuery(TimeQuery query) => query.Result = DateTime.Now;
                }
                """));

        var registration = Ui.Card("@Demo.Text.EventBusPage.registration012",
            Ui.Code("@Demo.Text.EventBusPage.registration013"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.EventBusPage.Content014"),
            Ui.Sub("@Demo.Text.EventBusPage.Content015"),
            Ui.TwoColumns(session, subscriptions),
            registration);

        _message.Width = 200;
    }

    public override string TitleKey => DemoKeys.ItemBus;

    public override string DescriptionKey => DemoKeys.DescBus;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("bus.core", L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties016"), L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties017"),
            DemoProperty.ReadOnly("bus.name", L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties018"), "EventBus"),
            DemoProperty.Bool("bus.async", L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties019"), false, L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties020")),
            DemoProperty.Bool("bus.scan", L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties021"), false, L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties022")),
            DemoProperty.Number("bus.order", L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties023"), 1, 0, 100, 1, null, L.Instance.GetResource("Demo.Text.EventBusPage.BuildProperties024"))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "bus.async" && DemoProperty.Flag(change))
        {
            _ = PublishAsync();
        }
        else if (change.Item.Id == "bus.scan" && DemoProperty.Flag(change))
        {
            _bus.Subscribe([typeof(TimeQueryHandler).Assembly]);
            Append(L.Instance.GetResource("Demo.Text.EventBusPage.OnPropertyCommitted025"));
            Logger.Info(L.Instance.GetResource("Demo.Text.EventBusPage.OnPropertyCommitted026"));
        }
    }

    private void Publish()
    {
        var name = _message.Text ?? string.Empty;
        Append($"Publish(new GreetCommand(\"{name}\"))");
        _bus.Publish(new GreetCommand(name));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.EventBusPage.Publish027"), name));
    }

    private async Task PublishAsync()
    {
        var name = _message.Text ?? string.Empty;
        Append($"PublishAsync(new GreetCommand(\"{name}\"))");
        await _bus.PublishAsync(new GreetCommand(name));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.EventBusPage.PublishAsync028"), name));
    }

    private void Query()
    {
        var query = new TimeQuery();
        _bus.Query(query);
        Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.EventBusPage.Query029"), query.Result));
        Logger.Info(L.Instance.GetResource("Demo.Text.EventBusPage.Query030"));
    }

    private void ToggleSecondSubscription()
    {
        _secondSubscriptionActive = !_secondSubscriptionActive;
        if (_secondSubscriptionActive)
        {
            _bus.Subscribe(_secondSubscription);
            _unsubscribeButton.Content = L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription031");
            _subscriptionState.Text = L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription032");
        }
        else
        {
            _bus.Unsubscribe(_secondSubscription);
            _unsubscribeButton.Content = L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription033");
            _subscriptionState.Text = L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription034");
        }

        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription037"), (_secondSubscriptionActive ? L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription035") : L.Instance.GetResource("Demo.Text.EventBusPage.ToggleSecondSubscription036"))));
    }

    private void ClearFeed()
    {
        _feed.Text = L.Instance.GetResource("Demo.Text.EventBusPage.ClearFeed038");
        Logger.Info(L.Instance.GetResource("Demo.Text.EventBusPage.ClearFeed039"));
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current == L.Instance.GetResource("Demo.Text.EventBusPage.Append040") ? line : current + "\n" + line;
    }
}
