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
    private static string XamlSnippet => L.Instance.GetResource("Demo.Text.CorePage.XamlSnippet001");

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.CorePage.CSharpSnippet002");

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
        _input = Ui.Input(L.Instance.GetResource("Demo.Text.CorePage.Content003"));

        _output = Ui.Code("@Demo.Text.CorePage.Content004", output: true);

        var tools = Ui.Card("@Demo.Text.CorePage.tools005",
            _input,
            Ui.Row(
                Ui.Button("@Demo.Text.CorePage.tools006", RunTruncate),
                Ui.Button("@Demo.Text.CorePage.tools007", RunSlug),
                Ui.Button("@Demo.Text.CorePage.tools008", RunBytes),
                Ui.Button("@Demo.Text.CorePage.tools009", RunMatch)),
            _output,
            Ui.Hint("@Demo.Text.CorePage.tools010"),
            Ui.Code("@Demo.Text.CorePage.tools011"));

        _updateRepo = Ui.Input("dotnet9/CodeWF.Toolkit");
        _updateCurrent = Ui.Input("1.0.0.0");
        _updateEtag = Ui.Switch(true);
        _updateOutput = Ui.Code("@Demo.Text.CorePage.Content012", output: true);
        _updateButton = Ui.Button("@Demo.Text.CorePage.Content013", async () => await CheckUpdateAsync(), primary: true);

        var update = Ui.Card("@Demo.Text.CorePage.update014",
            Ui.Field(L.Instance.GetResource("Demo.Text.CorePage.update015"), _updateRepo),
            Ui.Field(L.Instance.GetResource("Demo.Text.CorePage.update016"), _updateCurrent),
            Ui.Row(_updateButton, _updateEtag, Ui.Key("@Demo.Text.CorePage.update017")),
            _updateOutput,
            Ui.Hint("@Demo.Text.CorePage.update018"),
            Ui.Code("""
                var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
                var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
                """));

        var data = Ui.Card("@Demo.Text.CorePage.data019",
            Ui.Code("@Demo.Text.CorePage.data020"),
            Ui.Code("@Demo.Text.CorePage.data021", output: true));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.CorePage.Content022"),
            Ui.Sub("@Demo.Text.CorePage.Content023"),
            Ui.TwoColumns(tools, update),
            data);

        _updateRepo.Width = 180;
        _updateCurrent.Width = 120;
    }

    public override string TitleKey => DemoKeys.ItemCore;

    public override string DescriptionKey => DemoKeys.DescCore;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        _truncateItem = DemoProperty.Number("core.truncate", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties024"), 18, 4, 200, 1, L.Instance.GetResource("Demo.Text.CorePage.BuildProperties025"), L.Instance.GetResource("Demo.Text.CorePage.BuildProperties026"));

        return DemoProperty.Finish(
            DemoProperty.Category("core.ext", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties027"), L.Instance.GetResource("Demo.Text.CorePage.BuildProperties028"),
                DemoProperty.ReadOnly("core.namespace", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties029"), "CodeWF.Toolkit.Core.Extensions"),
                _truncateItem,
                DemoProperty.Bool("core.regex", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties030"), true, L.Instance.GetResource("Demo.Text.CorePage.BuildProperties031"))),
            DemoProperty.Category("core.update", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties032"), L.Instance.GetResource("Demo.Text.CorePage.BuildProperties033"),
                DemoProperty.Bool("core.etag", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties034"), true, L.Instance.GetResource("Demo.Text.CorePage.BuildProperties035")),
                DemoProperty.ReadOnly("core.endpoint", L.Instance.GetResource("Demo.Text.CorePage.BuildProperties036"), L.Instance.GetResource("Demo.Text.CorePage.BuildProperties037"))));
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
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.RunTruncate038"), text, _truncateLength, cut);
        Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.RunTruncate039"), _truncateLength));
    }

    private void RunSlug()
    {
        var text = _input.Text ?? string.Empty;
        var slug = System.Text.RegularExpressions.Regex.Replace(text, "[^A-Za-z0-9]+", "_").ToLowerInvariant().Trim('_');
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.RunSlug040"), slug);
        Log(L.Instance.GetResource("Demo.Text.CorePage.RunSlug041"));
    }

    private void RunBytes()
    {
        double[] values = [1024, 1_048_576, 3_221_225_472];
        var lines = values.Select(v => $"{v,13:N0} B  →  {v.FormatBytes()}");
        _output.Text = "DoubleExtensions.FormatBytes：\n" + string.Join('\n', lines);
        Log(L.Instance.GetResource("Demo.Text.CorePage.RunBytes042"));
    }

    private void RunMatch()
    {
        var text = _input.Text ?? string.Empty;
        var results = new (string Pattern, bool Matched)[]
        {
            ("CodeWF*", text.IsMatch("CodeWF*")),
            ("*Toolkit", text.IsMatch("*Toolkit")),
            (L.Instance.GetResource("Demo.Text.CorePage.RunMatch043"), text.IsMatch(L.Instance.GetResource("Demo.Text.CorePage.RunMatch044"))),
            ("Toolkit", text.IsMatch("Toolkit")),
        };
        _output.Text = L.Instance.GetResource("Demo.Text.CorePage.RunMatch045")
                       + string.Join('\n', results.Select(r => $"{r.Pattern,-12} → {r.Matched}"));
        Log(L.Instance.GetResource("Demo.Text.CorePage.RunMatch046"));
    }

    private async Task CheckUpdateAsync()
    {
        var repo = (_updateRepo.Text ?? string.Empty).Trim().Trim('/');
        var parts = repo.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            _updateOutput.Text = L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync047");
            return;
        }

        if (!Version.TryParse(_updateCurrent.Text, out var current))
        {
            current = new Version(1, 0, 0, 0);
        }

        _updateButton.IsEnabled = false;
        _updateOutput.Text = $"GET https://github.com/{parts[0]}/{parts[1]}/releases/latest\n"
                             + (_updateEtag.IsChecked == true ? L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync048") : string.Empty)
                             + L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync049");
        Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync050"), parts[0], parts[1], current));

        try
        {
            var stateFile = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "update-state.json");
            var checker = new UpdateChecker(new HttpClient(), parts[0], parts[1],
                log: message => Log(message), stateFile: stateFile);
            var result = await checker.CheckAsync(current);

            if (!result.Succeeded)
            {
                _updateOutput.Text += $"\n\nUpdateCheckResult.Succeeded = false\nError = {result.Error}";
                Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync051"), result.Error), "warn");
            }
            else if (result.Update is null)
            {
                _updateOutput.Text += string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync052"), current);
                Log(L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync053"));
            }
            else
            {
                var update = result.Update;
                _updateOutput.Text += "\n\nUpdateCheckResult.Succeeded = true\n"
                                      + $"Update = {{ Version: {update.Version}, Tag: {update.Tag} }}\n"
                                      + $"Asset: {update.AssetName ?? L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync054")}\n"
                                      + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync055"), current, update.Tag);
                Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.CorePage.CheckUpdateAsync056"), update.Tag), "warn");
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
