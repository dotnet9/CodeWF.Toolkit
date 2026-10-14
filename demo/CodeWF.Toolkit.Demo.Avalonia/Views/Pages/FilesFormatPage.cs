using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
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
    private static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.FilesFormat.Source.Xaml);

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.FilesFormat.Source.CSharp);

    private static readonly Student Sample = new();

    private readonly SelectableTextBlock _forward;
    private readonly SelectableTextBlock _backward;

    public FilesFormatPage()
    {
        _forward = Ui.Code("@Demo.Content.FilesFormat.Content.SelectAnOutputFormat", output: true);
        _backward = Ui.Code("@Demo.Content.FilesFormat.Content.AwaitingParsing", output: true);

        var forward = Ui.Card("@Demo.Content.FilesFormat.forward.ObjectTextRealCall",
            Ui.Row(
                Ui.Button("ToJson", () => ToJson(), primary: true),
                Ui.Button("ToYaml", () => ToYaml()),
                Ui.Button("@Demo.Content.FilesFormat.forward.JsonPrettifyAndSort", () => Prettify()),
                Ui.Button("Json → Yaml", () => JsonToYaml())),
            _forward);

        var backward = Ui.Card("@Demo.Content.FilesFormat.backward.TextObject",
            Ui.Row(
                Ui.Button("FromJson<Student>", () => FromJson()),
                Ui.Button("FromYaml<Student>", () => FromYaml())),
            _backward,
            Ui.Hint("@Demo.Content.FilesFormat.backward.ExtensionsUseOutErrorParsingReturnsFalseWith"));

        var model = Ui.Card("@Demo.Content.FilesFormat.model.DataModel",
            Ui.Code("@Demo.Content.FilesFormat.model.PublicSealedClassStudentAlicePublicIntAge"));

        var table = Ui.Card("@Demo.Content.FilesFormat.table.DataTableCSVExcelRealFileRoundTrip",
            Ui.Row(Ui.Button("@Demo.Content.FilesFormat.table.ExportAndImportCSV", RunCsvRoundTrip, primary: true)),
            Ui.Code("@Demo.Content.FilesFormat.table.DataTableExtensionsExportToCsvExportToXlsxDataTableExtensionsImportFromCsvImportFromExcelImportBy", output: true),
            Ui.Hint("@Demo.Content.FilesFormat.table.FilesAreWrittenUnderTEMPCodeWFToolkitDemo"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.FilesFormat.Content.FilesJSONYAMLCSVExcel"),
            Ui.Sub("@Demo.Content.FilesFormat.Content.RealRoundTripConversionUsingSystemTextJson"),
            Ui.TwoColumns(forward, backward),
            Ui.TwoColumns(model, table));
    }

    public override string TitleKey => DemoKeys.FilesFormatExampleTitle;

    public override string DescriptionKey => DemoKeys.DescFilesFormat;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("files.fmt", L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.FormatOptions), L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.DefaultFileIOOptions),
            DemoProperty.Choice("files.format", L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.DefaultFormat), "Yaml", ["Json", "Yaml", "Csv", "Excel"]),
            DemoProperty.Number("files.retry", L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.WriteRetries), 3, 0, 10, 1, L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.Retries), L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.NumberOfRetriesForSafeFileHelperIO)),
            DemoProperty.Bool("files.bom", "UTF-8 BOM", false, L.Instance.GetResource(DemoResources.Content.FilesFormat.Properties.IncludeABOMWhenWritingFiles))));

    private void ToJson()
    {
        if (!Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out var error))
        {
            _forward.Text = L.Instance.GetResource(DemoResources.Content.FilesFormat.ToJson.Failed) + error;
            return;
        }

        _forward.Text = json;
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.ToJson.FileFormatDemoToJson));
    }

    private void ToYaml()
    {
        if (!Sample.ToYaml(OverviewJsonContext.Default.Student, out var yaml, out var error))
        {
            _forward.Text = L.Instance.GetResource(DemoResources.Content.FilesFormat.ToYaml.Failed) + error;
            return;
        }

        _forward.Text = yaml;
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.ToYaml.FileFormatDemoToYaml));
    }

    private void Prettify()
    {
        Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out _);
        if (!json.JsonPrettify(2, sortKeys: true, out var pretty, out var error))
        {
            _forward.Text = L.Instance.GetResource(DemoResources.Content.FilesFormat.Prettify.Failed) + error;
            return;
        }

        _forward.Text = pretty;
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.Prettify.FileFormatDemoJsonPrettifyIndent2SortedKeys));
    }

    private void JsonToYaml()
    {
        Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out _);
        if (!json.JsonToYaml(out var yaml, out var error))
        {
            _forward.Text = L.Instance.GetResource(DemoResources.Content.FilesFormat.JsonToYaml.Failed) + error;
            return;
        }

        _forward.Text = yaml;
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.JsonToYaml.FileFormatDemoJsonToYaml));
    }

    private void FromJson()
    {
        Sample.ToJson(OverviewJsonContext.Default.Student, out var json, out _);
        if (!json.FromJson(OverviewJsonContext.Default.Student, out var student, out var error) || student is null)
        {
            _backward.Text = L.Instance.GetResource(DemoResources.Content.FilesFormat.FromJson.Failed) + error;
            return;
        }

        _backward.Text = $"FromJson<Student> → Name={student.Name}, Age={student.Age}, City={student.City}  (error = null)";
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.FromJson.FileFormatDemoFromJson));
    }

    private void FromYaml()
    {
        Sample.ToYaml(OverviewJsonContext.Default.Student, out var yaml, out _);
        if (!yaml.FromYaml(OverviewJsonContext.Default.Student, out var student, out var error) || student is null)
        {
            _backward.Text = L.Instance.GetResource(DemoResources.Content.FilesFormat.FromYaml.Failed) + error;
            return;
        }

        _backward.Text = $"FromYaml<Student> → Name={student.Name}, Age={student.Age}, City={student.City}  (error = null)";
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.FromYaml.FileFormatDemoFromYaml));
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
            lines.Add(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.FilesFormat.RunCsvRoundTrip.FileSizeBytesUTF8BOMFormat), new FileInfo(csvPath).Length, (HasBom(csvPath) ? L.Instance.GetResource(DemoResources.Content.FilesFormat.RunCsvRoundTrip.Yes) : L.Instance.GetResource(DemoResources.Content.FilesFormat.RunCsvRoundTrip.No))));
        }

        lines.Add($"ImportFromCsv → {(imported ? "true" : "false：" + importError)}");
        if (imported && back is not null)
        {
            lines.Add(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.FilesFormat.RunCsvRoundTrip.RestoredRowsColumnsFirstRowFormat), back.Rows.Count, back.Columns.Count, string.Join(", ", back.Rows[0].ItemArray)));
        }

        _backward.Text = string.Join('\n', lines);
        Log(L.Instance.GetResource(DemoResources.Content.FilesFormat.RunCsvRoundTrip.FileFormatDemoDataTableCSVRoundTrip));
    }

    private static bool HasBom(string path)
    {
        using var stream = File.OpenRead(path);
        return stream.ReadByte() == 0xEF && stream.ReadByte() == 0xBB && stream.ReadByte() == 0xBF;
    }

    private static void Log(string message) => Logger.Info(message);
}
