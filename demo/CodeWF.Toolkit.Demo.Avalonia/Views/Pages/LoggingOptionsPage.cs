using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>
/// 日志字典：LoggerOptions / FileLogOptions 参数全览，配 Logger.Health 与 Logger.Events 现场演示。
/// 也说明与 CodeWF.Avalonia.Log 的对接点（LogView 订阅的就是 Logger.Events 的 UI 事件流）。
/// </summary>
public sealed class LoggingOptionsPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Logging" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.LoggingOptionsPage.CSharpSnippet001");

    private readonly SelectableTextBlock _output;

    public LoggingOptionsPage()
    {
        _output = Ui.Code("@Demo.Text.LoggingOptionsPage.Content002", output: true);

        Content = Ui.Page(
            Ui.Heading(I18n(DemoKeys.ItemLoggingOptions)),
            Ui.Sub(I18n(DemoKeys.DescLoggingOptions)),
            Ui.Card("@Demo.Text.LoggingOptionsPage.Content003", BuildOptionsTable()),
            Ui.Card("@Demo.Text.LoggingOptionsPage.Content004",
                Ui.Row(
                    Ui.Button("@Demo.Text.LoggingOptionsPage.Content005", ShowHealth, primary: true),
                    Ui.Button("@Demo.Text.LoggingOptionsPage.Content006", ShowFeed),
                    Ui.Button("@Demo.Text.LoggingOptionsPage.Content007", WriteSamples)),
                _output,
                Ui.Hint("@Demo.Text.LoggingOptionsPage.Content008")),
            Ui.Card("@Demo.Text.LoggingOptionsPage.Content009",
                Ui.Hint("@Demo.Text.LoggingOptionsPage.Content010"
                        + L.Instance.GetResource("Demo.Text.LoggingOptionsPage.Content011"))));
    }

    public override string TitleKey => DemoKeys.ItemLoggingOptions;

    public override string DescriptionKey => DemoKeys.DescLoggingOptions;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() =>
    [
        new PropertyItem
        {
            Id = "logging.level",
            Name = L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildProperties012"),
            Kind = PropertyItemKind.Choice,
            Options = ["Trace", "Debug", "Information", "Warning", "Error"],
            Value = "Information",
            DefaultValue = "Information",
            Description = L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildProperties013"),
        },
        new PropertyItem
        {
            Id = "logging.rotating",
            Name = L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildProperties014"),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildProperties015"),
        },
        new PropertyItem
        {
            Id = "logging.feed",
            Name = L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildProperties016"),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildProperties017"),
        },
    ];

    private static Control BuildOptionsTable()
    {
        var rows = new (string Option, string Default, string Note)[]
        {
            ("MinimumLevel", "Information", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable018")),
            ("EnableConsole", "true", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable019")),
            ("File.DirectoryPath", "BaseDirectory/logs", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable020")),
            ("File.MaxFileSizeBytes", "32 MB", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable021")),
            ("File.RetentionDays", "30", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable022")),
            ("LineTemplate", "DefaultTemplate", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable023")),
            ("LocalizedLevelFormatter", "zh / en", L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable024")),
        };

        return Ui.Table([L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable025"), L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable026"), L.Instance.GetResource("Demo.Text.LoggingOptionsPage.BuildOptionsTable027")], rows.Select(r => new[] { r.Option, r.Default, r.Note }).ToArray());
    }

    private void ShowHealth()
    {
        var snapshot = Logger.Health.GetSnapshot();
        var byLevel = snapshot.DroppedByLevel.Count == 0
            ? L.Instance.GetResource("Demo.Text.LoggingOptionsPage.ShowHealth028")
            : string.Join(Environment.NewLine, snapshot.DroppedByLevel.Select(p => $"  {p.Key}: {p.Value}"));

        _output.Text = $"Logger.Health.GetSnapshot()" + Environment.NewLine
            + $"  DroppedCount = {snapshot.DroppedCount}" + Environment.NewLine
            + "  DroppedByLevel:" + Environment.NewLine + byLevel;
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingOptionsPage.ShowHealth029"), snapshot.DroppedCount));
    }

    private void ShowFeed()
    {
        var recent = Logger.Events.GetRecentEvents();
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingOptionsPage.ShowFeed030"), recent.Count) + Environment.NewLine
            + string.Join(Environment.NewLine, recent.Take(8).Select(e => $"  [{e.Level}] {e.Message}"));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingOptionsPage.ShowFeed031"), recent.Count));
    }

    private void WriteSamples()
    {
        Logger.Info(L.Instance.GetResource("Demo.Text.LoggingOptionsPage.WriteSamples032"));
        Logger.Warn(L.Instance.GetResource("Demo.Text.LoggingOptionsPage.WriteSamples033"));
        Logger.Error(L.Instance.GetResource("Demo.Text.LoggingOptionsPage.WriteSamples034"), new InvalidOperationException(L.Instance.GetResource("Demo.Text.LoggingOptionsPage.WriteSamples035")));
        ShowFeed();
    }

    private static string I18n(string key) => CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(key);
}
