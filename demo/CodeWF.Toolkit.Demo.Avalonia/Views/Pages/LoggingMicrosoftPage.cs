using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
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

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.CSharpSnippet001");

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
        _feed = Ui.Code("@Demo.Text.LoggingMicrosoftPage.Content002", output: true);
        _template = Ui.Input(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.Content003"));
        _values = Ui.Input("admin,10:24,192.168.1.8");
        _templateOutput = Ui.Code("@Demo.Text.LoggingMicrosoftPage.Content004", output: true);
        _providerButton = Ui.Button("@Demo.Text.LoggingMicrosoftPage.Content005", BuildProvider, primary: true);

        var register = Ui.Card("@Demo.Text.LoggingMicrosoftPage.register006",
            Ui.Code("@Demo.Text.LoggingMicrosoftPage.register007"),
            Ui.Hint("@Demo.Text.LoggingMicrosoftPage.register008"));

        var pipeline = Ui.Card("@Demo.Text.LoggingMicrosoftPage.pipeline009",
            Ui.Row(_providerButton, Ui.Button("@Demo.Text.LoggingMicrosoftPage.pipeline010", WriteStructured), Ui.Button("@Demo.Text.LoggingMicrosoftPage.pipeline011", WriteUserMessage)),
            _feed,
            Ui.Hint("@Demo.Text.LoggingMicrosoftPage.pipeline012"));

        var parser = Ui.Card("@Demo.Text.LoggingMicrosoftPage.parser013",
            Ui.Field(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.parser014"), _template),
            Ui.Field(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.parser015"), _values),
            Ui.Row(Ui.Button("@Demo.Text.LoggingMicrosoftPage.parser016", () => RunParser(_template.Text, _values.Text), primary: true)),
            _templateOutput,
            Ui.Hint("@Demo.Text.LoggingMicrosoftPage.parser017"));

        var options = Ui.Card("@Demo.Text.LoggingMicrosoftPage.options018",
            Ui.Code("@Demo.Text.LoggingMicrosoftPage.options019"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.LoggingMicrosoftPage.Content020"),
            Ui.Sub("@Demo.Text.LoggingMicrosoftPage.Content021"),
            Ui.TwoColumns(register, pipeline),
            Ui.TwoColumns(parser, options));
    }

    public override string TitleKey => DemoKeys.ItemLogMicrosoft;

    public override string DescriptionKey => DemoKeys.DescLogMicrosoft;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("mel.core", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties022"), L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties023"),
            DemoProperty.Choice("mel.level", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties024"), "Information", ["Trace", "Debug", "Information", "Warning", "Error"]),
            DemoProperty.Bool("mel.scope", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties025"), true, "CodeWFCaptureOptions.Scopes。"),
            DemoProperty.Bool("mel.user", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties026"), false, L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties027")),
            DemoProperty.Choice("mel.queue", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties028"), "DropOldest", ["DropOldest", "DropNewest", "Block"])),
        DemoProperty.Category("mel.state", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties029"), L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties030"),
            DemoProperty.ReadOnly("mel.provider", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties031"), "CodeWFLoggerProvider"),
            DemoProperty.ReadOnly("mel.feed", L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties032"), L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProperties033"))));

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
            Append(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProvider034"));
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
        Append(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProvider035"));
        Append(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProvider036"));
        Logger.Info(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.BuildProvider037"));
    }

    private void WriteStructured()
    {
        if (_melLogger is null)
        {
            BuildProvider();
        }

        _melLogger!.LogInformation(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.WriteStructured038"), "admin", "10:24", "192.168.1.8");
        Append(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.WriteStructured039"));
    }

    private void WriteUserMessage()
    {
        if (_melLogger is null)
        {
            BuildProvider();
        }

        _melLogger!.LogUserInformation(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.WriteUserMessage040"), L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.WriteUserMessage041"), "admin", "10:24", "192.168.1.8");
        Append(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.WriteUserMessage042"));
    }

    private void RunParser(string? template, string? values)
    {
        template = string.IsNullOrEmpty(template) ? L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.RunParser043") : template;
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

        _templateOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.RunParser044"), string.Join(", ", names), rendered);
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.RunParser045"), names.Count));
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource("Demo.Text.LoggingMicrosoftPage.Append046"), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
