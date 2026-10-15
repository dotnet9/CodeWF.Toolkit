using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
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

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Source.CSharp);

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
        _feed = Ui.Code("@Demo.Content.LoggingMicrosoft.Content.AwaitingMELLogs", output: true);
        _template = Ui.Input(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Content.UserLoggedInAtFrom));
        _values = Ui.Input("admin,10:24,192.168.1.8");
        _templateOutput = Ui.Code("@Demo.Content.LoggingMicrosoft.Content.AwaitingParsing", output: true);
        _providerButton = Ui.Button("@Demo.Content.LoggingMicrosoft.Content.BuildServiceProviderAddCodeWF", BuildProvider, primary: true);

        var register = Ui.Card("@Demo.Content.LoggingMicrosoft.register.Registration",
            Ui.Code("@Demo.Content.LoggingMicrosoft.register.BuilderLoggingAddCodeWFOptions"),
            Ui.Hint("@Demo.Content.LoggingMicrosoft.register.AddCodeWFRegistersCodeWFLoggerRuntimeLogEventFeedLineTemplateControllerAndCodeWFLogHealth"));

        var pipeline = Ui.Card("@Demo.Content.LoggingMicrosoft.pipeline.RealPipelineBuiltWithServiceCollection",
            Ui.Row(_providerButton, Ui.Button("@Demo.Content.LoggingMicrosoft.pipeline.WriteAMELLog", WriteStructured), Ui.Button("@Demo.Content.LoggingMicrosoft.pipeline.WriteAUserMessageLog", WriteUserMessage)),
            _feed,
            Ui.Hint("@Demo.Content.LoggingMicrosoft.pipeline.TheBootstrapProviderSharesTheEventStreamWith"));

        var parser = Ui.Card("@Demo.Content.LoggingMicrosoft.parser.StructuredTemplatesMessageTemplateParser",
            Ui.Field(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.parser.Template), _template),
            Ui.Field(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.parser.Parameter), _values),
            Ui.Row(Ui.Button("@Demo.Content.LoggingMicrosoft.parser.Parse", () => RunParser(_template.Text, _values.Text), primary: true)),
            _templateOutput,
            Ui.Hint("@Demo.Content.LoggingMicrosoft.parser.MessageTemplateParserExtractsPlaceholderNamesAndFormatsArgumentsIncluding"));

        var options = Ui.Card("@Demo.Content.LoggingMicrosoft.options.OptionsOverview",
            Ui.Code("@Demo.Content.LoggingMicrosoft.options.CodeWFLoggerOptionsConfigurationBindingAndValidationCodeWFFileLogOptionsFileRotation"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.LoggingMicrosoft.Content.MicrosoftExtensionsLoggingProvider"),
            Ui.Sub("@Demo.Content.LoggingMicrosoft.Content.ConnectMELLogsToTheCoreLoggerWith"),
            Ui.TwoColumns(register, pipeline),
            Ui.TwoColumns(parser, options));
    }

    public override string TitleKey => DemoPages.LogMicrosoft;

    public override string DescriptionKey => DemoKeys.DescLogMicrosoft;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("mel.core", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.ProviderOptions), L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.MapsToCodeWFLoggerOptions),
            DemoProperty.Choice("mel.level", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.MinimumLevel), "Information", ["Trace", "Debug", "Information", "Warning", "Error"]),
            DemoProperty.Bool("mel.scope", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.IncludeScopes), true, "CodeWFCaptureOptions.Scopes。"),
            DemoProperty.Bool("mel.user", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.UserMessageMarker), false, L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.LogUserExtensionsWriteCodeWFUserLogState)),
            DemoProperty.Choice("mel.queue", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.QueueOverflowPolicy), "DropOldest", ["DropOldest", "DropNewest", "Block"])),
        DemoProperty.Category("mel.state", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.RuntimeState), L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.ContainerAndEventStreamState),
            DemoProperty.ReadOnly("mel.provider", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.Provider), "CodeWFLoggerProvider"),
            DemoProperty.ReadOnly("mel.feed", L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.EventFlow), L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Properties.SharedWithStaticLogger))));

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
            Append(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.BuildProvider.ServiceProviderBuiltRepeatedClicksDoNotRegisterIt));
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
        Append(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.BuildProvider.AddCodeWFServiceProviderBuiltILoggerFactoryReady));
        Append(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.BuildProvider.CodeWFLoggerRuntimeLogEventFeedLineTemplateControllerCodeWFLogHealthRegistered));
        Logger.Info(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.BuildProvider.MELProviderBuilt));
    }

    private void WriteStructured()
    {
        if (_melLogger is null)
        {
            BuildProvider();
        }

        _melLogger!.LogInformation(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.WriteStructured.UserLoggedInAtFrom), "admin", "10:24", "192.168.1.8");
        Append(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.WriteStructured.MELInformationUserAdminLoggedInAt10));
    }

    private void WriteUserMessage()
    {
        if (_melLogger is null)
        {
            BuildProvider();
        }

        _melLogger!.LogUserInformation(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.WriteUserMessage.LoginSucceeded), L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.WriteUserMessage.UserLoggedInAtFrom), "admin", "10:24", "192.168.1.8");
        Append(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.WriteUserMessage.MELInformationLoginSucceededUserMessageFullTemplateIn));
    }

    private void RunParser(string? template, string? values)
    {
        template = string.IsNullOrEmpty(template) ? L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.RunParser.UserLoggedInAtFrom) : template;
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

        _templateOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.RunParser.TemplateKeysParsedResultFormat), string.Join(", ", names), rendered);
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.RunParser.MessageTemplateParserPlaceholdersFormat), names.Count));
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource(DemoResources.Content.LoggingMicrosoft.Append.AwaitingLogWrite), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
