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
    private const string XamlSnippet = """
        <!-- 演示台的日志面板就是本页的另一个实例：底部 Dock 工具面板绑同一个 LogEventFeed -->
        <codewf:LogView Source="{Binding LogSource}" MaxDisplayCount="500" />
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Logging;

        Logger.TryInitialize(new LoggerOptions
        {
            MinimumLevel = LogLevel.Information,
            EnableConsole = true,
            File = new FileLogOptions
            {
                DirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs"),
                MaxFileSizeBytes = 32 * 1024 * 1024,   // 单文件上限，超出自动轮转
                RetentionDays = 30,                    // 保留天数
                BatchSize = 200,                       // 批量落盘条数
                FlushInterval = TimeSpan.FromMilliseconds(500),
                OutputTemplate = null,                 // 为空时用 LineTemplate
            },
            LineTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} 【{Level:zh}】 {UserMessage}{NewLine}",
            QueueCapacity = 10_000,
            QueueFullMode = LogQueueFullMode.DropTraceAndDebug,
        });

        Logger.Debug("调试：加载配置");
        Logger.Info("服务已启动");
        Logger.Warn("内存水位偏高");
        Logger.Error("连接已超时", new TimeoutException("connect timeout"));

        // UI 事件流（LogView 的数据源）；回调在后台线程，订阅方自行调度到 UI 线程
        using var subscription = Logger.Events.Subscribe(logEvent => Console.WriteLine(logEvent.Message));

        // 热更新输出模板（含校验失败原因）
        Logger.LineTemplate.TryUpdate("{Timestamp:HH:mm:ss.fff} {Level:zh} {Message}{NewLine}", out var error);

        await Logger.ShutdownAsync();   // 退出前刷盘
        """;

    private static readonly (string Key, LogLevel Level, string Message)[] SampleLogs =
    [
        ("debug", LogLevel.Debug, "调试：加载配置完成"),
        ("info", LogLevel.Information, "服务已启动，监听 127.0.0.1:39561"),
        ("warn", LogLevel.Warning, "内存水位偏高：82%"),
        ("error", LogLevel.Error, "连接已超时（示例异常）"),
        ("fatal", LogLevel.Critical, "致命：磁盘写入不可恢复"),
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

        var stream = Ui.Card("日志流（真实 Logger）",
            Ui.Row(
                Ui.Button("Debug", () => Write("debug")),
                Ui.Button("Info", () => Write("info")),
                Ui.Button("Warn", () => Write("warn")),
                Ui.Button("Error", () => Write("error")),
                Ui.Button("Fatal", () => Write("fatal"))),
            _streamList,
            Ui.Row(Ui.Key("最低显示级别"), _levelFilter, Ui.Button("清空", ClearStream)),
            Ui.Hint("写入会同时进控制台、文件（批量异步 + 轮转）与 UI 事件流（本页列表与底部日志面板）。"));

        _templatePreview = Ui.Code($"DefaultTemplate:\n{LineTemplateController.DefaultTemplate}", output: true);
        var templateButton = Ui.Button("应用自定义模板", ApplyTemplate);

        var options = Ui.Card("初始化与选项",
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
            Ui.Hint("文件写入是批量异步的：崩溃前请 ShutdownAsync / FlushAsync；UI 事件流回调在后台线程。"),
            Ui.Field("当前日志目录", Ui.Key(DemoBootstrapper.LogFolder)));
        var members = Ui.Card("成员一览",
            Ui.Code("""
                Logger.TryInitialize / Initialize     初始化（重复调用返回 false / 抛异常）
                Logger.Trace / Debug / Info / Warn    静态写入，支持 userMessage 与 requestNotification
                Logger.Error / Fatal                  带异常的写入
                Logger.*ToFile                        仅落文件、不进 UI 事件流
                Logger.FlushAsync / ShutdownAsync     刷盘与关闭
                LogEventFeed.Subscribe / GetRecentEvents  UI 事件流订阅与最近事件快照
                LineTemplateController                运行期热更新输出模板（带校验）
                CodeWFLogHealth                       日志系统自身健康与自诊断
                """));

        Content = Ui.Page(
            Ui.Heading("Logger 核心"),
            Ui.Sub("静态 Logger：控制台 + 文件轮转 + UI 事件流。下方按钮写入真实的日志，事件流与底部日志面板同步刷新。"),
            Ui.TwoColumns(stream, options),
            members);
    }

    public override string TitleKey => DemoKeys.ItemLogCore;

    public override string DescriptionKey => DemoKeys.DescLogCore;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        _levelItem = DemoProperty.Choice("lg.level", "最低级别", "Information",
            ["Trace", "Debug", "Information", "Warning", "Error"], "控制台与 UI 事件流的过滤参考级别。");
        _consoleItem = DemoProperty.Bool("lg.console", "控制台输出", true, "LoggerOptions.EnableConsole。");
        _rotateItem = DemoProperty.Bool("lg.rotate", "文件轮转", true, "超出单文件上限时按序号轮转。");

        return DemoProperty.Finish(
            DemoProperty.Category("lg.core", "Logger 选项", "与 LoggerOptions 对应的选项。",
                _levelItem,
                _consoleItem,
                _rotateItem,
                DemoProperty.Number("lg.maxmb", "单文件上限(MB)", 32, 1, 1024, 1, "MB", "FileLogOptions.MaxFileSizeBytes。"),
                DemoProperty.Number("lg.days", "保留天数", 30, 1, 365, 1, "天", "FileLogOptions.RetentionDays。")),
            DemoProperty.Category("lg.state", "运行状态", "当前演示台的日志运行状态。",
                DemoProperty.ReadOnly("lg.folder", "日志目录", DemoBootstrapper.LogFolder),
                DemoProperty.ReadOnly("lg.feed", "事件流容量", $"{_stream.Capacity} 条（RecentEventCapacity）")));
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
                Logger.Info($"日志选项（演示）：{change.Item.Name} = {DemoProperty.Text(change)}");
                break;
        }
    }

    public override void OnDocumentOpened()
    {
        Logger.Info("打开演示页：Logger 核心");
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
        Logger.Info("日志流已清空（仅清 UI 事件流，文件日志保留）");
    }

    private void ApplyTemplate()
    {
        var template = "{Timestamp:HH:mm:ss.fff} {Level:zh} {Message} [类别 {Category}]{NewLine}";
        if (Logger.Events.LineTemplate.TryUpdate(template, out var error))
        {
            _templatePreview.Text = $"LineTemplate 已更新：\n{Logger.Events.LineTemplate.Current}";
            Logger.Info("输出模板已更新");
        }
        else
        {
            _templatePreview.Text = $"模板校验失败：\n{LineTemplateController.DefaultTemplate}\n\n{error}";
            Logger.Warn("输出模板校验失败：" + error);
        }
    }
}
