using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>事件总线页：真实的进程内总线（订阅 / 发布 / 查询 / 退订）。</summary>
public sealed class EventBusPage : DemoPageBase
{
    private static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.EventBus.Source.Xaml);

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.EventBus.Source.CSharp);

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

        _feed = Ui.Code("@Demo.Content.EventBus.Content.BusReadyTwoGreetCommandSubscribersAndOneTimeQuery", output: true);
        _message = Ui.Input("hello");
        _unsubscribeButton = Ui.Button("@Demo.Content.EventBus.Content.RemoveSecondSubscription", ToggleSecondSubscription);
        _subscriptionState = Ui.Key("@Demo.Content.EventBus.Content.Subscribed");

        var session = Ui.Card("@Demo.Content.EventBus.session.SessionLiveBus",
            _feed,
            Ui.Row(_message, Ui.Button("Publish(GreetCommand)", Publish, primary: true)),
            Ui.Row(
                Ui.Button("Query(TimeQuery)", Query),
                Ui.Button("PublishAsync(GreetCommand)", () => _ = PublishAsync()),
                Ui.Button("@Demo.Content.EventBus.session.Clear", ClearFeed)),
            Ui.Hint("@Demo.Content.EventBus.session.HandlerExceptionsPropagatePublishAsyncExecutesSubscribersInRegistration"));

        var subscriptions = Ui.Card("@Demo.Content.EventBus.subscriptions.Subscriptions",
            Ui.Code("@Demo.Content.EventBus.subscriptions.GreetCommandLambdaRegisteredGreetCommandEventHandlerRegisteredLaterOrder", output: true),
            Ui.Row(_unsubscribeButton, Ui.Key("@Demo.Content.EventBus.subscriptions.SecondGreetCommandSubscription"), _subscriptionState),
            Ui.Code("""
                public sealed class TimeQueryHandler
                {
                    [EventHandler]
                    private void ReceiveTimeQuery(TimeQuery query) => query.Result = DateTime.Now;
                }
                """));

        var registration = Ui.Card("@Demo.Content.EventBus.registration.Registration",
            Ui.Code("@Demo.Content.EventBus.registration.EventBusSubscribePublishNoDependenciesEventBusPrismDesktop"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.EventBus.Content.InProcessEventBus"),
            Ui.Sub("@Demo.Content.EventBus.Content.PublishSubscribeCommandQueryAndUnsubscribeRunThrough"),
            Ui.TwoColumns(session, subscriptions),
            registration);

        _message.Width = 200;
    }

    public override string TitleKey => DemoPages.Bus;

    public override string DescriptionKey => DemoKeys.DescBus;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("bus.core", L.Instance.GetResource(DemoResources.Content.EventBus.Properties.BusInstance), L.Instance.GetResource(DemoResources.Content.EventBus.Properties.TheActualEventBusInstanceOwnedByThisDemo),
            DemoProperty.ReadOnly("bus.name", L.Instance.GetResource(DemoResources.Content.EventBus.Properties.BusInstance), "EventBus"),
            DemoProperty.Bool("bus.async", L.Instance.GetResource(DemoResources.Content.EventBus.Properties.PublishAsynchronously), false, L.Instance.GetResource(DemoResources.Content.EventBus.Properties.UsePublishAsyncWhenEnabled)),
            DemoProperty.Bool("bus.scan", L.Instance.GetResource(DemoResources.Content.EventBus.Properties.AssemblyScanning), false, L.Instance.GetResource(DemoResources.Content.EventBus.Properties.RegisterEventHandlerMethodsFromAnAssembly)),
            DemoProperty.Number("bus.order", L.Instance.GetResource(DemoResources.Content.EventBus.Properties.HandlerOrder), 1, 0, 100, 1, null, L.Instance.GetResource(DemoResources.Content.EventBus.Properties.ExecutionOrderWhenACommandHasMultipleHandlers))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "bus.async" && DemoProperty.Flag(change))
        {
            _ = PublishAsync();
        }
        else if (change.Item.Id == "bus.scan" && DemoProperty.Flag(change))
        {
            _bus.Subscribe([typeof(TimeQueryHandler).Assembly]);
            Append(L.Instance.GetResource(DemoResources.Content.EventBus.OnPropertyCommitted.AssemblyScanningTimeQueryHandlerRegistered));
            Logger.Info(L.Instance.GetResource(DemoResources.Content.EventBus.OnPropertyCommitted.EventBusRegisteringHandlersFromTheAssembly));
        }
    }

    private void Publish()
    {
        var name = _message.Text ?? string.Empty;
        Append($"Publish(new GreetCommand(\"{name}\"))");
        _bus.Publish(new GreetCommand(name));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBus.Publish.EventBusPublishGreetCommandFormat), name));
    }

    private async Task PublishAsync()
    {
        var name = _message.Text ?? string.Empty;
        Append($"PublishAsync(new GreetCommand(\"{name}\"))");
        await _bus.PublishAsync(new GreetCommand(name));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBus.PublishAsync.EventBusAsyncPublishGreetCommandFormat), name));
    }

    private void Query()
    {
        var query = new TimeQuery();
        _bus.Query(query);
        Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBus.Query.QueryTimeQueryResultFormat), query.Result));
        Logger.Info(L.Instance.GetResource(DemoResources.Content.EventBus.Query.EventBusQueryTimeQuery));
    }

    private void ToggleSecondSubscription()
    {
        _secondSubscriptionActive = !_secondSubscriptionActive;
        if (_secondSubscriptionActive)
        {
            _bus.Subscribe(_secondSubscription);
            _unsubscribeButton.Content = L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.RemoveSecondSubscription);
            _subscriptionState.Text = L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.Subscribed);
        }
        else
        {
            _bus.Unsubscribe(_secondSubscription);
            _unsubscribeButton.Content = L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.RestoreSecondSubscription);
            _subscriptionState.Text = L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.Unsubscribed);
        }

        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.SecondEventBusSubscriptionFormat), (_secondSubscriptionActive ? L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.Enabled) : L.Instance.GetResource(DemoResources.Content.EventBus.ToggleSecondSubscription.Cancel))));
    }

    private void ClearFeed()
    {
        _feed.Text = L.Instance.GetResource(DemoResources.Content.EventBus.ClearFeed.SessionCleared);
        Logger.Info(L.Instance.GetResource(DemoResources.Content.EventBus.ClearFeed.EventBusSessionCleared));
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current == L.Instance.GetResource(DemoResources.Content.EventBus.Append.SessionCleared) ? line : current + "\n" + line;
    }
}
