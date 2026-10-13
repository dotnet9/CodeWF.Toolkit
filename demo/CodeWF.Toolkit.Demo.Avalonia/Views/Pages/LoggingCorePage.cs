using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.Infrastructure;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;
using Microsoft.Extensions.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>Logger 核心页：真实写日志（控制台 + 文件轮转 + UI 事件流），含级别过滤与模板预览。</summary>
public sealed class LoggingCorePage : DemoPageBase
{
    private static string XamlSnippet => L.Instance.GetResource("Demo.Text.LoggingCorePage.XamlSnippet001");

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.LoggingCorePage.CSharpSnippet002");

    private static readonly (string Key, LogLevel Level, string Message)[] SampleLogs =
    [
        ("debug", LogLevel.Debug, L.Instance.GetResource("Demo.Text.LoggingCorePage.SampleLogs003")),
        ("info", LogLevel.Information, L.Instance.GetResource("Demo.Text.LoggingCorePage.SampleLogs004")),
        ("warn", LogLevel.Warning, L.Instance.GetResource("Demo.Text.LoggingCorePage.SampleLogs005")),
        ("error", LogLevel.Error, L.Instance.GetResource("Demo.Text.LoggingCorePage.SampleLogs006")),
        ("fatal", LogLevel.Critical, L.Instance.GetResource("Demo.Text.LoggingCorePage.SampleLogs007")),
    ];

    private readonly LogStream _stream = new();
    private readonly ListBox _streamList;
    private readonly SelectableTextBlock _templatePreview;
    private readonly ComboBox _levelFilter;
    private PropertyItem? _levelItem;
    private PropertyItem? _consoleItem;
    private PropertyItem? _rotateItem;

    public LoggingCorePage()
    {
        _streamList = new ListBox
        {
            ItemsSource = _stream.Lines,
            Height = 200,
            FontFamily = new global::Avalonia.Media.FontFamily("Consolas,Cascadia Mono,Menlo,monospace"),
            FontSize = 11.5,
        };

        _levelFilter = Ui.Select(["Trace", "Debug", "Information", "Warning", "Error"], 0, 130);
        _levelFilter.SelectionChanged += (_, _) =>
        {
            _stream.MinimumLevel = _levelFilter.SelectedIndex switch
            {
                1 => LogLevel.Debug,
                2 => LogLevel.Information,
                3 => LogLevel.Warning,
                4 => LogLevel.Error,
                _ => LogLevel.Trace,
            };
        };

        var stream = Ui.Card("@Demo.Text.LoggingCorePage.stream008",
            Ui.Row(
                Ui.Button("Debug", () => Write("debug")),
                Ui.Button("Info", () => Write("info")),
                Ui.Button("Warn", () => Write("warn")),
                Ui.Button("Error", () => Write("error")),
                Ui.Button("Fatal", () => Write("fatal"))),
            _streamList,
            Ui.Row(Ui.Key("@Demo.Text.LoggingCorePage.stream009"), _levelFilter, Ui.Button("@Demo.Text.LoggingCorePage.stream010", ClearStream)),
            Ui.Hint("@Demo.Text.LoggingCorePage.stream011"));

        _templatePreview = Ui.Code($"DefaultTemplate:\n{LineTemplateController.DefaultTemplate}", output: true);
        var templateButton = Ui.Button("@Demo.Text.LoggingCorePage.templateButton012", ApplyTemplate);

        var options = Ui.Card("@Demo.Text.LoggingCorePage.options013",
            Ui.Code("""
                Logger.TryInitialize(new LoggerOptions
                {
                    MinimumLevel = LogLevel.Information,
                    EnableConsole = true,
                    File = new FileLogOptions
                    {
                        DirectoryPath = "logs",
                        MaxFileSizeBytes = 32 * 1024 * 1024,
                        RetentionDays = 30,
                        BatchSize = 200,
                        FlushInterval = TimeSpan.FromMilliseconds(500),
                    },
                    LineTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} 【{Level:zh}】 {UserMessage}{NewLine}",
                    QueueCapacity = 10_000,
                    QueueFullMode = LogQueueFullMode.DropTraceAndDebug,
                });
                """),
            Ui.Row(templateButton),
            _templatePreview,
            Ui.Hint("@Demo.Text.LoggingCorePage.options014"),
            Ui.Field(L.Instance.GetResource("Demo.Text.LoggingCorePage.options015"), Ui.Key(DemoBootstrapper.LogFolder)));
        var members = Ui.Card("@Demo.Text.LoggingCorePage.members016",
            Ui.Code("@Demo.Text.LoggingCorePage.members017"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.LoggingCorePage.Content018"),
            Ui.Sub("@Demo.Text.LoggingCorePage.Content019"),
            Ui.TwoColumns(stream, options),
            members);
    }

    public override string TitleKey => DemoKeys.ItemLogCore;

    public override string DescriptionKey => DemoKeys.DescLogCore;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        _levelItem = DemoProperty.Choice("lg.level", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties020"), "Information",
            ["Trace", "Debug", "Information", "Warning", "Error"], L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties021"));
        _consoleItem = DemoProperty.Bool("lg.console", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties022"), true, "LoggerOptions.EnableConsole。");
        _rotateItem = DemoProperty.Bool("lg.rotate", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties023"), true, L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties024"));

        return DemoProperty.Finish(
            DemoProperty.Category("lg.core", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties025"), L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties026"),
                _levelItem,
                _consoleItem,
                _rotateItem,
                DemoProperty.Number("lg.maxmb", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties027"), 32, 1, 1024, 1, "MB", "FileLogOptions.MaxFileSizeBytes。"),
                DemoProperty.Number("lg.days", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties028"), 30, 1, 365, 1, L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties029"), "FileLogOptions.RetentionDays。")),
            DemoProperty.Category("lg.state", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties030"), L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties031"),
                DemoProperty.ReadOnly("lg.folder", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties032"), DemoBootstrapper.LogFolder),
                DemoProperty.ReadOnly("lg.feed", L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties033"), string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingCorePage.BuildProperties034"), _stream.Capacity))));
    }

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case "lg.level" when DemoProperty.Text(change) is { } level:
                var index = Array.FindIndex(["Trace", "Debug", "Information", "Warning", "Error"],
                    value => value.Equals(level, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    _levelFilter.SelectedIndex = index;
                }

                break;
            case "lg.maxmb" or "lg.days":
                Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingCorePage.OnPropertyCommitted035"), change.Item.Name, DemoProperty.Text(change)));
                break;
        }
    }

    public override void OnDocumentOpened()
    {
        Logger.Info(L.Instance.GetResource("Demo.Text.LoggingCorePage.OnDocumentOpened036"));
    }

    private void Write(string key)
    {
        var (_, level, message) = SampleLogs.First(sample => sample.Key == key);
        switch (level)
        {
            case LogLevel.Debug:
                Logger.Debug(message);
                break;
            case LogLevel.Information:
                Logger.Info(message);
                break;
            case LogLevel.Warning:
                Logger.Warn(message);
                break;
            case LogLevel.Error:
                Logger.Error(message, new TimeoutException("connect timeout"));
                break;
            default:
                Logger.Fatal(message, new IOException("disk write failed"));
                break;
        }
    }

    private void ClearStream()
    {
        _stream.Clear();
        Logger.Info(L.Instance.GetResource("Demo.Text.LoggingCorePage.ClearStream037"));
    }

    private void ApplyTemplate()
    {
        var template = L.Instance.GetResource("Demo.Text.LoggingCorePage.ApplyTemplate038");
        if (Logger.Events.LineTemplate.TryUpdate(template, out var error))
        {
            _templatePreview.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingCorePage.ApplyTemplate039"), Logger.Events.LineTemplate.Current);
            Logger.Info(L.Instance.GetResource("Demo.Text.LoggingCorePage.ApplyTemplate040"));
        }
        else
        {
            _templatePreview.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingCorePage.ApplyTemplate041"), LineTemplateController.DefaultTemplate, error);
            Logger.Warn(L.Instance.GetResource("Demo.Text.LoggingCorePage.ApplyTemplate042") + error);
        }
    }
}
