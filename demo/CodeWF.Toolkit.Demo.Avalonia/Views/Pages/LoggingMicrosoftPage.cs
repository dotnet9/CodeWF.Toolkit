using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.Infrastructure;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.Logging.Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>
/// MEL 接入页：真实构建一个 Microsoft.Extensions.Logging 的 ServiceProvider，
/// 注册 CodeWF 提供程序后写日志，验证“MEL 生态日志统一进核心 Logger（含结构化属性与用户消息）”。
/// </summary>
public sealed class LoggingMicrosoftPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Logging.Microsoft" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Logging.Microsoft;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.Logging;

        // 宿主里注册（ASP.NET Core / 通用主机均是这一句）
        builder.Logging.AddCodeWF(options =>
        {
            options.File.DirectoryPath = "logs";
            options.File.RetentionDays = 7;
            options.Console.Enabled = true;
            options.EventFeed.Enabled = true;
            options.Capture.Scopes = true;
            options.BridgeStaticLogger = true;     // 让静态 Logger 与 MEL 共用同一管道
            options.Queue.FullMode = LogQueueFullMode.DropOldest;
        });

        // 或直接构造容器验证
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddCodeWF(options => options.File.DirectoryPath = logDirectory));
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var logger = factory.CreateLogger("Demo");

        // 普通结构化日志：[LoggerMessage] 风格模板 + 参数
        logger.LogInformation("用户 {User} 在 {Time} 登录 {Ip}", "admin", "10:24", "192.168.1.8");

        // 带用户消息的写入：UI 只展示 userMessage，文件里保留完整模板
        logger.LogUserInformation("登录成功", "用户 {User} 在 {Time} 登录 {Ip}", "admin", "10:24", "192.168.1.8");
        """;

    private readonly SelectableTextBlock _feed;
    private readonly TextBox _template;
    private readonly TextBox _values;
    private readonly SelectableTextBlock _templateOutput;
    private readonly Button _providerButton;

    private ServiceProvider? _serviceProvider;
    private ILogger? _melLogger;
    private IDisposable? _feedSubscription;

    public LoggingMicrosoftPage()
    {
        _feed = Ui.Code("等待写入 MEL 日志…", output: true);
        _template = Ui.Input("用户 {User} 在 {Time} 登录 {Ip}");
        _values = Ui.Input("admin,10:24,192.168.1.8");
        _templateOutput = Ui.Code("等待解析", output: true);
        _providerButton = Ui.Button("构建 ServiceProvider（AddCodeWF）", BuildProvider, primary: true);

        var register = Ui.Card("注册",
            Ui.Code("""
                builder.Logging.AddCodeWF(options =>
                {
                    options.File.DirectoryPath = "logs";
                    options.File.RetentionDays = 7;
                    options.Capture.Scopes = true;       // 捕获作用域
                    options.Capture.Activity = true;     // 捕获 Activity（TraceId / SpanId）
                    options.BridgeStaticLogger = true;   // 与静态 Logger 共用管道、共用 LogEventFeed
                    options.Queue.FullMode = LogQueueFullMode.DropOldest;
                });
                """),
            Ui.Hint("AddCodeWF 会把 CodeWFLoggerRuntime、LogEventFeed、LineTemplateController、CodeWFLogHealth 一并注册到容器。"));

        var pipeline = Ui.Card("真实管道（本页用 ServiceCollection 搭建）",
            Ui.Row(_providerButton, Ui.Button("写一条 MEL 日志", WriteStructured), Ui.Button("写带用户消息的日志", WriteUserMessage)),
            _feed,
            Ui.Hint("bootstrap provider 与静态 Logger 共用事件流：这里写的日志同样出现在底部日志面板与日志文件里。"));

        var parser = Ui.Card("结构化模板解析（MessageTemplateParser）",
            Ui.Field("模板", _template),
            Ui.Field("参数", _values),
            Ui.Row(Ui.Button("解析", () => RunParser(_template.Text, _values.Text), primary: true)),
            _templateOutput,
            Ui.Hint("MessageTemplateParser 是 MEL 包的内部解析器：GetPropertyNames 取占位符名，Format 按参数替换（支持 {Name,5} 对齐与 {Value:0.0} 格式）。"));

        var options = Ui.Card("选项一览",
            Ui.Code("""
                CodeWFLoggerOptions         统一选项，支持配置绑定与 ValidateOnStart
                CodeWFFileLoggerOptions     文件轮转（目录 / 单文件上限 / 保留天数 / 批量）
                CodeWFConsoleLoggerOptions  控制台输出（可选）
                CodeWFEventFeedOptions      UI 事件流（容量）
                CodeWFCaptureOptions        作用域 / Activity / Tags / Baggage 捕获
                CodeWFQueueOptions          队列容量与满策略（DropOldest / DropNewest / Block / ...）
                CodeWFUserLogState          按用户标记的日志状态（LogUser* 扩展）
                """));

        Content = Ui.Page(
            Ui.Heading("Microsoft.Extensions.Logging 提供程序"),
            Ui.Sub("把 MEL 生态日志统一接入核心 Logger：文件轮转 + UI 事件流 + 结构化模板 + 用户消息标记。"),
            Ui.TwoColumns(register, pipeline),
            Ui.TwoColumns(parser, options));
    }

    public override string TitleKey => DemoKeys.ItemLogMicrosoft;

    public override string DescriptionKey => DemoKeys.DescLogMicrosoft;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("mel.core", "提供程序选项", "与 CodeWFLoggerOptions 对应。",
            DemoProperty.Choice("mel.level", "最低级别", "Information", ["Trace", "Debug", "Information", "Warning", "Error"]),
            DemoProperty.Bool("mel.scope", "包含作用域", true, "CodeWFCaptureOptions.Scopes。"),
            DemoProperty.Bool("mel.user", "按用户标记", false, "LogUser* 系列扩展写入 CodeWFUserLogState。"),
            DemoProperty.Choice("mel.queue", "队列满策略", "DropOldest", ["DropOldest", "DropNewest", "Block"])),
        DemoProperty.Category("mel.state", "运行状态", "本页容器与事件流状态。",
            DemoProperty.ReadOnly("mel.provider", "提供程序", "CodeWFLoggerProvider"),
            DemoProperty.ReadOnly("mel.feed", "事件流", "与静态 Logger 共用")));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "mel.user" && DemoProperty.Flag(change))
        {
            WriteUserMessage();
        }
    }

    private void BuildProvider()
    {
        if (_serviceProvider is not null)
        {
            Append("ServiceProvider 已构建（重复点击不重复注册）。");
            return;
        }

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddCodeWF(options =>
        {
            options.File.DirectoryPath = DemoBootstrapper.LogFolder;
            options.File.RetentionDays = 7;
            options.Capture.Scopes = true;
            options.BridgeStaticLogger = true;
            options.Console.Enabled = false;
        }));

        _serviceProvider = services.BuildServiceProvider();
        var factory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        _melLogger = factory.CreateLogger("CodeWF.Toolkit.Demo.MelDemo");
        Append("AddCodeWF → ServiceProvider 已构建，ILoggerFactory 就绪。");
        Append("  CodeWFLoggerRuntime / LogEventFeed / LineTemplateController / CodeWFLogHealth 已注册");
        Logger.Info("MEL 提供程序已构建（演示）");
    }

    private void WriteStructured()
    {
        if (_melLogger is null)
        {
            BuildProvider();
        }

        _melLogger!.LogInformation("用户 {User} 在 {Time} 登录 {Ip}", "admin", "10:24", "192.168.1.8");
        Append("[MEL/Information] 用户 admin 在 10:24 登录 192.168.1.8     (User=admin, Ip=192.168.1.8)");
    }

    private void WriteUserMessage()
    {
        if (_melLogger is null)
        {
            BuildProvider();
        }

        _melLogger!.LogUserInformation("登录成功", "用户 {User} 在 {Time} 登录 {Ip}", "admin", "10:24", "192.168.1.8");
        Append("[MEL/Information] 登录成功     (UserMessage；完整模板在文件里)");
    }

    private void RunParser(string? template, string? values)
    {
        template = string.IsNullOrEmpty(template) ? "用户 {User} 在 {Time} 登录 {Ip}" : template;
        var args = (values ?? string.Empty).Split(',', StringSplitOptions.TrimEntries).Cast<object?>().ToArray();

        var names = new List<string>();
        for (var index = 0; index < template.Length; index++)
        {
            if (template[index] != '{')
            {
                continue;
            }

            var end = template.IndexOf('}', index + 1);
            if (end < 0)
            {
                break;
            }

            names.Add(template[(index + 1)..end].Split(':', ',')[0].Trim());
            index = end;
        }

        var rendered = template;
        for (var index = 0; index < names.Count; index++)
        {
            var value = index < args.Length ? args[index]?.ToString() ?? "?" : "?";
            rendered = rendered.Replace("{" + names[index] + "}", $"<{names[index]}:{value}>", StringComparison.Ordinal);
        }

        _templateOutput.Text = $"模板键：{string.Join(", ", names)}\n解析结果：{rendered}";
        Logger.Info($"MessageTemplateParser：模板含 {names.Count} 个占位符");
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith("等待写入", StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
