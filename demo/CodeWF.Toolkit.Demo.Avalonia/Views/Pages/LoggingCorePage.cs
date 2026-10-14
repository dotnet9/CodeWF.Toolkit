using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
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
    private static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.LoggingCore.Source.Xaml);

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.LoggingCore.Source.CSharp);

    private static readonly (string Key, LogLevel Level, string Message)[] SampleLogs =
    [
        ("debug", LogLevel.Debug, L.Instance.GetResource(DemoResources.Content.LoggingCore.SampleLogs.DebugConfigurationLoaded)),
        ("info", LogLevel.Information, L.Instance.GetResource(DemoResources.Content.LoggingCore.SampleLogs.ServiceStartedListeningOn127001)),
        ("warn", LogLevel.Warning, L.Instance.GetResource(DemoResources.Content.LoggingCore.SampleLogs.MemoryUtilization82)),
        ("error", LogLevel.Error, L.Instance.GetResource(DemoResources.Content.LoggingCore.SampleLogs.ConnectionTimedOutSampleException)),
        ("fatal", LogLevel.Critical, L.Instance.GetResource(DemoResources.Content.LoggingCore.SampleLogs.CriticalUnrecoverableDiskWriteFailure)),
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

        var stream = Ui.Card("@Demo.Content.LoggingCore.stream.LogStreamRealLogger",
            Ui.Row(
                Ui.Button("Debug", () => Write("debug")),
                Ui.Button("Info", () => Write("info")),
                Ui.Button("Warn", () => Write("warn")),
                Ui.Button("Error", () => Write("error")),
                Ui.Button("Fatal", () => Write("fatal"))),
            _streamList,
            Ui.Row(Ui.Key("@Demo.Content.LoggingCore.stream.MinimumDisplayLevel"), _levelFilter, Ui.Button("@Demo.Content.LoggingCore.stream.Clear", ClearStream)),
            Ui.Hint("@Demo.Content.LoggingCore.stream.LogsReachTheConsoleAsynchronousRotatingFilesAnd"));

        _templatePreview = Ui.Code($"DefaultTemplate:\n{LineTemplateController.DefaultTemplate}", output: true);
        var templateButton = Ui.Button("@Demo.Content.LoggingCore.templateButton.ApplyCustomTemplate", ApplyTemplate);

        var options = Ui.Card("@Demo.Content.LoggingCore.options.InitializationAndOptions",
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
            Ui.Hint("@Demo.Content.LoggingCore.options.FileWritesAreAsynchronousFlushOrShutDown"),
            Ui.Field(L.Instance.GetResource(DemoResources.Content.LoggingCore.options.CurrentLogDirectory), Ui.Key(DemoBootstrapper.LogFolder)));
        var members = Ui.Card("@Demo.Content.LoggingCore.members.Members",
            Ui.Code("@Demo.Content.LoggingCore.members.LoggerTryInitializeInitializeInitializeTheRuntimeLoggerTrace"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.LoggingCore.Content.LoggerCore"),
            Ui.Sub("@Demo.Content.LoggingCore.Content.StaticLoggerProvidesConsoleRotatingFilesAndUI"),
            Ui.TwoColumns(stream, options),
            members);
    }

    public override string TitleKey => DemoPages.LogCore;

    public override string DescriptionKey => DemoKeys.DescLogCore;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        _levelItem = DemoProperty.Choice("lg.level", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.MinimumLevel), "Information",
            ["Trace", "Debug", "Information", "Warning", "Error"], L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.FilterLevelForConsoleAndUIEventOutput));
        _consoleItem = DemoProperty.Bool("lg.console", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.ConsoleOutput), true, "LoggerOptions.EnableConsole。");
        _rotateItem = DemoProperty.Bool("lg.rotate", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.FileRotation), true, L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.RotateBySequenceNumberAboveThePerFile));

        return DemoProperty.Finish(
            DemoProperty.Category("lg.core", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.LoggerOptions), L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.MapsToLoggerOptions),
                _levelItem,
                _consoleItem,
                _rotateItem,
                DemoProperty.Number("lg.maxmb", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.PerFileLimitMB), 32, 1, 1024, 1, "MB", "FileLogOptions.MaxFileSizeBytes。"),
                DemoProperty.Number("lg.days", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.RetentionDays), 30, 1, 365, 1, L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.Days), "FileLogOptions.RetentionDays。")),
            DemoProperty.Category("lg.state", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.RuntimeState), L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.LoggingRuntimeState),
                DemoProperty.ReadOnly("lg.folder", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.LogDirectory), DemoBootstrapper.LogFolder),
                DemoProperty.ReadOnly("lg.feed", L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.EventCapacity), string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingCore.Properties.EntriesRecentEventCapacityFormat), _stream.Capacity))));
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
                Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingCore.OnPropertyCommitted.LoggingOptionDemoFormat), change.Item.Name, DemoProperty.Text(change)));
                break;
        }
    }

    public override void OnDocumentOpened()
    {
        Logger.Info(L.Instance.GetResource(DemoResources.Content.LoggingCore.OnDocumentOpened.OpenedDemoLoggerCore));
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
        Logger.Info(L.Instance.GetResource(DemoResources.Content.LoggingCore.ClearStream.UILogStreamClearedLogFilesRemainAvailable));
    }

    private void ApplyTemplate()
    {
        var template = L.Instance.GetResource(DemoResources.Content.LoggingCore.ApplyTemplate.Category);
        if (Logger.Events.LineTemplate.TryUpdate(template, out var error))
        {
            _templatePreview.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingCore.ApplyTemplate.LineTemplateUpdatedFormat), Logger.Events.LineTemplate.Current);
            Logger.Info(L.Instance.GetResource(DemoResources.Content.LoggingCore.ApplyTemplate.OutputTemplateUpdated));
        }
        else
        {
            _templatePreview.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingCore.ApplyTemplate.TemplateValidationFailedFormat), LineTemplateController.DefaultTemplate, error);
            Logger.Warn(L.Instance.GetResource(DemoResources.Content.LoggingCore.ApplyTemplate.OutputTemplateValidationFailed) + error);
        }
    }
}
