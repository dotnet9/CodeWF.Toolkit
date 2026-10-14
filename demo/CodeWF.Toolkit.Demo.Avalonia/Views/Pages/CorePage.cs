using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.FileExtensions;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>Core 页：扩展方法（可实时试用）+ GitHub Release 更新检查。</summary>
public sealed class CorePage : DemoPageBase
{
    private static string XamlSnippet => L.Instance.GetResource("Demo.Content.Core.Source.Xaml");

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Content.Core.Source.CSharp");

    private readonly TextBox _input;
    private readonly SelectableTextBlock _output;
    private readonly TextBox _updateRepo;
    private readonly TextBox _updateCurrent;
    private readonly ToggleSwitch _updateEtag;
    private readonly SelectableTextBlock _updateOutput;
    private readonly Button _updateButton;

    private PropertyItem? _truncateItem;
    private int _truncateLength = 18;

    public CorePage()
    {
        _input = Ui.Input(L.Instance.GetResource("Demo.Content.Core.Content.CodeWFToolkitIsACollectionOfNETExtensions"));

        _output = Ui.Code("@Demo.Content.Core.Content.ClickAButtonAboveToViewItsResult", output: true);

        var tools = Ui.Card("@Demo.Content.Core.tools.StringNumericExtensionsRealCalls",
            _input,
            Ui.Row(
                Ui.Button("@Demo.Content.Core.tools.TruncateTo18", RunTruncate),
                Ui.Button("@Demo.Content.Core.tools.ConvertToIdentifier", RunSlug),
                Ui.Button("@Demo.Content.Core.tools.ByteUnits", RunBytes),
                Ui.Button("@Demo.Content.Core.tools.MatchPattern", RunMatch)),
            _output,
            Ui.Hint("@Demo.Content.Core.tools.SetTruncationLengthInThePropertyPanelByte"),
            Ui.Code("@Demo.Content.Core.tools.StringExtensionsMatchingAndJoiningDoubleExtensionsReadableByteSizes"));

        _updateRepo = Ui.Input("dotnet9/CodeWF.Toolkit");
        _updateCurrent = Ui.Input("1.0.0.0");
        _updateEtag = Ui.Switch(true);
        _updateOutput = Ui.Code("@Demo.Content.Core.Content.NotCheckedYet", output: true);
        _updateButton = Ui.Button("@Demo.Content.Core.Content.CheckUpdates", async () => await CheckUpdateAsync(), primary: true);

        var update = Ui.Card("@Demo.Content.Core.update.UpdateChecker",
            Ui.Field(L.Instance.GetResource("Demo.Content.Core.update.Repository"), _updateRepo),
            Ui.Field(L.Instance.GetResource("Demo.Content.Core.update.CurrentVersion"), _updateCurrent),
            Ui.Row(_updateButton, _updateEtag, Ui.Key("@Demo.Content.Core.update.EnableETagCache")),
            _updateOutput,
            Ui.Hint("@Demo.Content.Core.update.ChecksTheReleasesLatestRedirectFirstReusesCached"),
            Ui.Code("""
                var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
                var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
                """));

        var data = Ui.Card("@Demo.Content.Core.data.ApplicationDataDirectoriesAndEncodingDetection",
            Ui.Code("@Demo.Content.Core.data.VarDataDirAppDataMigratorResolveAndMigrateCodeWFToolkitConfigJson"),
            Ui.Code("@Demo.Content.Core.data.FileHelperGetFileEncodeTypeEncodingDetectionFileHelperSafeReadAllTextSafeText", output: true));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.Core.Content.CoreExtensionsAndInfrastructure"),
            Ui.Sub("@Demo.Content.Core.Content.CoreHasNoThirdPartyDependenciesTryExtensions"),
            Ui.TwoColumns(tools, update),
            data);

        _updateRepo.Width = 180;
        _updateCurrent.Width = 120;
    }

    public override string TitleKey => DemoKeys.CoreExampleTitle;

    public override string DescriptionKey => DemoKeys.DescCore;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        _truncateItem = DemoProperty.Number("core.truncate", L.Instance.GetResource("Demo.Content.Core.Properties.TruncationLength"), 18, 4, 200, 1, L.Instance.GetResource("Demo.Content.Core.Properties.Characters"), L.Instance.GetResource("Demo.Content.Core.Properties.LengthUsedByTheStringTruncationDemo"));

        return DemoProperty.Finish(
            DemoProperty.Category("core.ext", L.Instance.GetResource("Demo.Content.Core.Properties.ExtensionMethods"), L.Instance.GetResource("Demo.Content.Core.Properties.ParametersForTheCoreExtensionDemos"),
                DemoProperty.ReadOnly("core.namespace", L.Instance.GetResource("Demo.Content.Core.Properties.Namespace"), "CodeWF.Toolkit.Core.Extensions"),
                _truncateItem,
                DemoProperty.Bool("core.regex", L.Instance.GetResource("Demo.Content.Core.Properties.EnableRegexExtensions"), true, L.Instance.GetResource("Demo.Content.Core.Properties.WildcardExpressionUsedByIsMatch"))),
            DemoProperty.Category("core.update", L.Instance.GetResource("Demo.Content.Core.Properties.UpdateChecking"), L.Instance.GetResource("Demo.Content.Core.Properties.OptionsForGitHubReleaseUpdateChecking"),
                DemoProperty.Bool("core.etag", L.Instance.GetResource("Demo.Content.Core.Properties.ETagConditionalCheck"), true, L.Instance.GetResource("Demo.Content.Core.Properties.304ResponsesDoNotConsumeTheAPIQuota")),
                DemoProperty.ReadOnly("core.endpoint", L.Instance.GetResource("Demo.Content.Core.Properties.Endpoint"), L.Instance.GetResource("Demo.Content.Core.Properties.WebEndpointFirstAPIFallback"))));
    }

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "core.truncate")
        {
            _truncateLength = (int)DemoProperty.Num(change, _truncateLength);
        }
        else if (change.Item.Id == "core.etag")
        {
            _updateEtag.IsChecked = DemoProperty.Flag(change);
        }
    }

    private void RunTruncate()
    {
        var text = _input.Text ?? string.Empty;
        var cut = text.Length > _truncateLength ? text[.._truncateLength] + "…" : text;
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.RunTruncate.OriginalTruncatedToFormat"), text, _truncateLength, cut);
        Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.RunTruncate.CoreExtensionDemoTruncateToCharactersFormat"), _truncateLength));
    }

    private void RunSlug()
    {
        var text = _input.Text ?? string.Empty;
        var slug = System.Text.RegularExpressions.Regex.Replace(text, "[^A-Za-z0-9]+", "_").ToLowerInvariant().Trim('_');
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.RunSlug.ReplaceNonAlphanumericCharactersAndLowercaseFormat"), slug);
        Log(L.Instance.GetResource("Demo.Content.Core.RunSlug.CoreExtensionDemoIdentifierConversion"));
    }

    private void RunBytes()
    {
        double[] values = [1024, 1_048_576, 3_221_225_472];
        var lines = values.Select(v => $"{v,13:N0} B  →  {v.FormatBytes()}");
        _output.Text = "DoubleExtensions.FormatBytes：\n" + string.Join('\n', lines);
        Log(L.Instance.GetResource("Demo.Content.Core.RunBytes.CoreExtensionDemoByteFormatting"));
    }

    private void RunMatch()
    {
        var text = _input.Text ?? string.Empty;
        var results = new (string Pattern, bool Matched)[]
        {
            ("CodeWF*", text.IsMatch("CodeWF*")),
            ("*Toolkit", text.IsMatch("*Toolkit")),
            (L.Instance.GetResource("Demo.Content.Core.RunMatch.Extensions"), text.IsMatch(L.Instance.GetResource("Demo.Content.Core.RunMatch.Extensions"))),
            ("Toolkit", text.IsMatch("Toolkit")),
        };
        _output.Text = L.Instance.GetResource("Demo.Content.Core.RunMatch.RegexExtensionsIsMatchWildcardCharacterDigitLetter")
                       + string.Join('\n', results.Select(r => $"{r.Pattern,-12} → {r.Matched}"));
        Log(L.Instance.GetResource("Demo.Content.Core.RunMatch.CoreExtensionDemoPatternMatching"));
    }

    private async Task CheckUpdateAsync()
    {
        var repo = (_updateRepo.Text ?? string.Empty).Trim().Trim('/');
        var parts = repo.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            _updateOutput.Text = L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.RepositoryMustUseOwnerRepoFormat");
            return;
        }

        if (!Version.TryParse(_updateCurrent.Text, out var current))
        {
            current = new Version(1, 0, 0, 0);
        }

        _updateButton.IsEnabled = false;
        _updateOutput.Text = $"GET https://github.com/{parts[0]}/{parts[1]}/releases/latest\n"
                             + (_updateEtag.IsChecked == true ? L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.ETagCacheHitsAvoidConsumingTheAPIQuota") : string.Empty)
                             + L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.Checking");
        Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.CheckingUpdatesCurrentFormat"), parts[0], parts[1], current));

        try
        {
            var stateFile = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "update-state.json");
            var checker = new UpdateChecker(new HttpClient(), parts[0], parts[1],
                log: message => Log(message), stateFile: stateFile);
            var result = await checker.CheckAsync(current);

            if (!result.Succeeded)
            {
                _updateOutput.Text += $"\n\nUpdateCheckResult.Succeeded = false\nError = {result.Error}";
                Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.UpdateCheckFailedFormat"), result.Error), "warn");
            }
            else if (result.Update is null)
            {
                _updateOutput.Text += string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.UpdateCheckResultSucceededTrueUpdateNullCurrentVersionIsFormat"), current);
                Log(L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.UpdateCheckAlreadyUpToDate"));
            }
            else
            {
                var update = result.Update;
                _updateOutput.Text += "\n\nUpdateCheckResult.Succeeded = true\n"
                                      + $"Update = {{ Version: {update.Version}, Tag: {update.Tag} }}\n"
                                      + $"Asset: {update.AssetName ?? L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.None")}\n"
                                      + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.CurrentNewVersionFormat"), current, update.Tag);
                Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.Core.CheckUpdateAsync.UpdateCheckNewVersionFormat"), update.Tag), "warn");
            }
        }
        finally
        {
            _updateButton.IsEnabled = true;
        }
    }

    private static void Log(string message, string level = "info")
    {
        if (level == "warn")
        {
            CodeWF.Toolkit.Logging.Logger.Warn(message);
        }
        else
        {
            CodeWF.Toolkit.Logging.Logger.Info(message);
        }
    }
}
