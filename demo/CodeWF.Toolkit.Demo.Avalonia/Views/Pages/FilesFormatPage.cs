using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Data;
using System.Text;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Files.Exports;
using CodeWF.Toolkit.Files.Extensions;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>文件格式页：Json / Yaml / Csv(Excel) 的真实往返，含 DataTable 导入导出。</summary>
public sealed class FilesFormatPage : DemoPageBase
{
    private static string XamlSnippet => L.Instance.GetResource("Demo.Text.FilesFormatPage.XamlSnippet001");

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.FilesFormatPage.CSharpSnippet002");

    private static readonly Student Sample = new();

    private readonly SelectableTextBlock _forward;
    private readonly SelectableTextBlock _backward;

    public FilesFormatPage()
    {
        _forward = Ui.Code("@Demo.Text.FilesFormatPage.Content003", output: true);
        _backward = Ui.Code("@Demo.Text.FilesFormatPage.Content004", output: true);

        var forward = Ui.Card("@Demo.Text.FilesFormatPage.forward005",
            Ui.Row(
                Ui.Button("ToJson", () => ToJson(), primary: true),
                Ui.Button("ToYaml", () => ToYaml()),
                Ui.Button("@Demo.Text.FilesFormatPage.forward006", () => Prettify()),
                Ui.Button("Json → Yaml", () => JsonToYaml())),
            _forward);

        var backward = Ui.Card("@Demo.Text.FilesFormatPage.backward007",
            Ui.Row(
                Ui.Button("FromJson<Student>", () => FromJson()),
                Ui.Button("FromYaml<Student>", () => FromYaml())),
            _backward,
            Ui.Hint("@Demo.Text.FilesFormatPage.backward008"));

        var model = Ui.Card("@Demo.Text.FilesFormatPage.model009",
            Ui.Code("@Demo.Text.FilesFormatPage.model010"));

        var table = Ui.Card("@Demo.Text.FilesFormatPage.table011",
            Ui.Row(Ui.Button("@Demo.Text.FilesFormatPage.table012", RunCsvRoundTrip, primary: true)),
            Ui.Code("@Demo.Text.FilesFormatPage.table013", output: true),
            Ui.Hint("@Demo.Text.FilesFormatPage.table014"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.FilesFormatPage.Content015"),
            Ui.Sub("@Demo.Text.FilesFormatPage.Content016"),
            Ui.TwoColumns(forward, backward),
            Ui.TwoColumns(model, table));
    }

    public override string TitleKey => DemoKeys.ItemFilesFormat;

    public override string DescriptionKey => DemoKeys.DescFilesFormat;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("files.fmt", L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties017"), L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties018"),
            DemoProperty.Choice("files.format", L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties019"), "Yaml", ["Json", "Yaml", "Csv", "Excel"]),
            DemoProperty.Number("files.retry", L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties020"), 3, 0, 10, 1, L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties021"), L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties022")),
            DemoProperty.Bool("files.bom", "UTF-8 BOM", false, L.Instance.GetResource("Demo.Text.FilesFormatPage.BuildProperties023"))));

    private void ToJson()
    {
        if (!Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out var error))
        {
            _forward.Text = L.Instance.GetResource("Demo.Text.FilesFormatPage.ToJson024") + error;
            return;
        }

        _forward.Text = json;
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.ToJson025"));
    }

    private void ToYaml()
    {
        if (!Sample.ToYaml(OverviewJsonContext.Default.Student, out var yaml, out var error))
        {
            _forward.Text = L.Instance.GetResource("Demo.Text.FilesFormatPage.ToYaml026") + error;
            return;
        }

        _forward.Text = yaml;
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.ToYaml027"));
    }

    private void Prettify()
    {
        Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out _);
        if (!json.JsonPrettify(2, sortKeys: true, out var pretty, out var error))
        {
            _forward.Text = L.Instance.GetResource("Demo.Text.FilesFormatPage.Prettify028") + error;
            return;
        }

        _forward.Text = pretty;
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.Prettify029"));
    }

    private void JsonToYaml()
    {
        Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out _);
        if (!json.JsonToYaml(out var yaml, out var error))
        {
            _forward.Text = L.Instance.GetResource("Demo.Text.FilesFormatPage.JsonToYaml030") + error;
            return;
        }

        _forward.Text = yaml;
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.JsonToYaml031"));
    }

    private void FromJson()
    {
        Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out _);
        if (!json.FromJson(OverviewJsonContext.Default.Student, out var student, out var error) || student is null)
        {
            _backward.Text = L.Instance.GetResource("Demo.Text.FilesFormatPage.FromJson032") + error;
            return;
        }

        _backward.Text = $"FromJson<Student> → Name={student.Name}, Age={student.Age}, City={student.City}  (error = null)";
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.FromJson033"));
    }

    private void FromYaml()
    {
        Sample.ToYaml(OverviewJsonContext.Default.Student, out var yaml, out _);
        if (!yaml.FromYaml(OverviewJsonContext.Default.Student, out var student, out var error) || student is null)
        {
            _backward.Text = L.Instance.GetResource("Demo.Text.FilesFormatPage.FromYaml034") + error;
            return;
        }

        _backward.Text = $"FromYaml<Student> → Name={student.Name}, Age={student.Age}, City={student.City}  (error = null)";
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.FromYaml035"));
    }

    private void RunCsvRoundTrip()
    {
        var folder = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "files");
        Directory.CreateDirectory(folder);
        var csvPath = Path.Combine(folder, "students.csv");

        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Age");
        table.Columns.Add("City");
        table.Rows.Add(Sample.Name, Sample.Age, Sample.City);

        var encoding = Encoding.UTF8;
        var exported = table.ExportToCsv(csvPath, encoding, out var exportError);
        var imported = csvPath.ImportFromCsv(out var importError, out var back);

        var lines = new List<string>
        {
            $"ExportToCsv(\"{csvPath}\") → {(exported ? "true" : "false：" + exportError)}",
        };
        if (File.Exists(csvPath))
        {
            lines.Add(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesFormatPage.RunCsvRoundTrip038"), new FileInfo(csvPath).Length, (HasBom(csvPath) ? L.Instance.GetResource("Demo.Text.FilesFormatPage.RunCsvRoundTrip036") : L.Instance.GetResource("Demo.Text.FilesFormatPage.RunCsvRoundTrip037"))));
        }

        lines.Add($"ImportFromCsv → {(imported ? "true" : "false：" + importError)}");
        if (imported && back is not null)
        {
            lines.Add(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesFormatPage.RunCsvRoundTrip039"), back.Rows.Count, back.Columns.Count, string.Join(", ", back.Rows[0].ItemArray)));
        }

        _backward.Text = string.Join('\n', lines);
        Log(L.Instance.GetResource("Demo.Text.FilesFormatPage.RunCsvRoundTrip040"));
    }

    private static bool HasBom(string path)
    {
        using var stream = File.OpenRead(path);
        return stream.ReadByte() == 0xEF && stream.ReadByte() == 0xBB && stream.ReadByte() == 0xBF;
    }

    private static void Log(string message) => Logger.Info(message);
}
