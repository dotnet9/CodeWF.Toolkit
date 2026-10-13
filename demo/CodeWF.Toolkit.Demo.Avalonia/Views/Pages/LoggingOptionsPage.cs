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

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Logging;

        // 全量选项：控制台 / 文件轮转 / 保留策略 / 行模板 / 本地化
        Logger.TryInitialize(new LoggerOptions
        {
            MinimumLevel = LogLevel.Debug,
            EnableConsole = false,
            File = new FileLogOptions
            {
                DirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs"),
                MaxFileSizeBytes = 32 * 1024 * 1024,
                RetentionDays = 30,
            },
        });

        // 健康诊断：丢弃计数（按级别细分）
        var snapshot = Logger.Health.GetSnapshot();
        var dropped = snapshot.DroppedCount;

        // UI 事件流：CodeWF.Avalonia.Log 的 LogView 订阅的就是它
        var recent = Logger.Events.GetRecentEvents();

        // 退出前刷盘，避免批量异步写入丢失
        await Logger.ShutdownAsync();
        """;

    private readonly SelectableTextBlock _output;

    public LoggingOptionsPage()
    {
        _output = Ui.Code("等待操作：点击上方按钮查看诊断 / 事件流快照", output: true);

        Content = Ui.Page(
            Ui.Heading(I18n(DemoKeys.ItemLoggingOptions)),
            Ui.Sub(I18n(DemoKeys.DescLoggingOptions)),
            Ui.Card("参数全览", BuildOptionsTable()),
            Ui.Card("健康与事件流",
                Ui.Row(
                    Ui.Button("Logger.Health 诊断", ShowHealth, primary: true),
                    Ui.Button("Logger.Events 快照", ShowFeed),
                    Ui.Button("写入三条示例日志", WriteSamples)),
                _output,
                Ui.Hint("UI 事件流的回调在后台线程触发，界面订阅方需自行调度到 UI 线程。")),
            Ui.Card("与 CodeWF.Avalonia 的对接",
                Ui.Hint("CodeWF.Avalonia.Log 的 LogView 直接订阅这份 UI 事件流；"
                        + "桌面演示台的底部日志面板就是它的一个实例。")));
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
            Name = "最低级别",
            Kind = PropertyItemKind.Choice,
            Options = ["Trace", "Debug", "Information", "Warning", "Error"],
            Value = "Information",
            DefaultValue = "Information",
            Description = "低于该级别的日志会被丢弃。",
        },
        new PropertyItem
        {
            Id = "logging.rotating",
            Name = "文件轮转",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "按单文件上限轮转，并按保留天数清理。",
        },
        new PropertyItem
        {
            Id = "logging.feed",
            Name = "UI 事件流",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "开启 UI 事件流，供桌面端日志控件订阅。",
        },
    ];

    private static Control BuildOptionsTable()
    {
        var rows = new (string Option, string Default, string Note)[]
        {
            ("MinimumLevel", "Information", "低于该级别的日志直接丢弃"),
            ("EnableConsole", "true", "控制台输出（桌面应用可关闭）"),
            ("File.DirectoryPath", "BaseDirectory/logs", "日志目录"),
            ("File.MaxFileSizeBytes", "32 MB", "单文件上限，超限轮转"),
            ("File.RetentionDays", "30", "保留天数"),
            ("LineTemplate", "DefaultTemplate", "LineTemplateController 可自定义输出模板"),
            ("LocalizedLevelFormatter", "zh / en", "级别文案本地化"),
        };

        return Ui.Table(["参数", "默认", "说明"], rows.Select(r => new[] { r.Option, r.Default, r.Note }).ToArray());
    }

    private void ShowHealth()
    {
        var snapshot = Logger.Health.GetSnapshot();
        var byLevel = snapshot.DroppedByLevel.Count == 0
            ? "  （无丢弃）"
            : string.Join(Environment.NewLine, snapshot.DroppedByLevel.Select(p => $"  {p.Key}: {p.Value}"));

        _output.Text = $"Logger.Health.GetSnapshot()" + Environment.NewLine
            + $"  DroppedCount = {snapshot.DroppedCount}" + Environment.NewLine
            + "  DroppedByLevel:" + Environment.NewLine + byLevel;
        Logger.Info($"日志健康检查：丢弃 {snapshot.DroppedCount} 条");
    }

    private void ShowFeed()
    {
        var recent = Logger.Events.GetRecentEvents();
        _output.Text = $"Logger.Events.GetRecentEvents() → {recent.Count} 条" + Environment.NewLine
            + string.Join(Environment.NewLine, recent.Take(8).Select(e => $"  [{e.Level}] {e.Message}"));
        Logger.Info($"UI 事件流快照：{recent.Count} 条");
    }

    private void WriteSamples()
    {
        Logger.Info("演示：一条信息日志");
        Logger.Warn("演示：一条警告日志");
        Logger.Error("演示：一条错误日志", new InvalidOperationException("示例异常"));
        ShowFeed();
    }

    private static string I18n(string key) => CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(key);
}
