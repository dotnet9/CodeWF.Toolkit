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
    private const string XamlSnippet = """
        <!-- 文件包基于框架内置 System.Text.Json 与 YamlDotNet / CsvHelper / MiniExcel -->
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Files" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Files.Extensions;
        using CodeWF.Toolkit.Files.Exports;

        var student = new Student { Name = "张三", Age = 20, City = "成都" };

        // 对象 → 文本：扩展方法统一带 out error，失败不外抛
        student.ToJson(out var json, out var error1);
        student.ToYaml(out var yaml, out var error2);

        // 文本 → 对象
        json.FromJson<Student>(out var fromJson, out var error3);
        yaml.FromYaml<Student>(out var fromYaml, out var error4);

        // 格式化与互转
        json.JsonPrettify(2, sortKeys: true, out var pretty, out _);
        json.JsonToYaml(out var yaml2, out _);

        // DataTable ↔ Csv / Excel
        dataTable.ExportToCsv("students.csv", Encoding.UTF8, out var error5);
        "students.csv".ImportFromCsv(out var error6, out var table);
        dataTable.ExportToXlsx("students.xlsx", out var error7);
        """;

    private static readonly Student Sample = new();

    private readonly SelectableTextBlock _forward;
    private readonly SelectableTextBlock _backward;

    public FilesFormatPage()
    {
        _forward = Ui.Code("选择一种格式输出", output: true);
        _backward = Ui.Code("等待反向解析", output: true);

        var forward = Ui.Card("对象 → 文本（真实调用）",
            Ui.Row(
                Ui.Button("ToJson", () => ToJson(), primary: true),
                Ui.Button("ToYaml", () => ToYaml()),
                Ui.Button("JsonPrettify + 排序", () => Prettify()),
                Ui.Button("Json → Yaml", () => JsonToYaml())),
            _forward);

        var backward = Ui.Card("文本 → 对象",
            Ui.Row(
                Ui.Button("FromJson<Student>", () => FromJson()),
                Ui.Button("FromYaml<Student>", () => FromYaml())),
            _backward,
            Ui.Hint("全部扩展方法都带 out error 参数：解析失败返回 false 并给出错误文本，不抛异常。"));

        var model = Ui.Card("数据模型",
            Ui.Code("""
                public sealed class Student
                {
                    public string Name { get; set; } = "张三";
                    public int Age { get; set; } = 20;
                    public string City { get; set; } = "成都";
                }
                """));

        var table = Ui.Card("DataTable ↔ Csv / Excel（真实写盘往返）",
            Ui.Row(Ui.Button("导出 Csv 再导入", RunCsvRoundTrip, primary: true)),
            Ui.Code("""
                DataTableExtensions.ExportToCsv  / ExportToXlsx
                DataTableExtensions.ImportFromCsv / ImportFromExcel / Import（按扩展名分派）
                """, output: true),
            Ui.Hint("写盘位置为临时目录 %TEMP%\\CodeWFToolkitDemo。"));

        Content = Ui.Page(
            Ui.Heading("文件读写：Json / Yaml / Csv / Excel"),
            Ui.Sub("基于框架内置 System.Text.Json 与 YamlDotNet / CsvHelper / MiniExcel，下面是真实往返转换。"),
            Ui.TwoColumns(forward, backward),
            Ui.TwoColumns(model, table));
    }

    public override string TitleKey => DemoKeys.ItemFilesFormat;

    public override string DescriptionKey => DemoKeys.DescFilesFormat;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("files.fmt", "格式选项", "文件读写演示的默认选项（与原型一致）。",
            DemoProperty.Choice("files.format", "默认格式", "Yaml", ["Json", "Yaml", "Csv", "Excel"]),
            DemoProperty.Number("files.retry", "写入重试", 3, 0, 10, 1, "次", "FileHelper 安全读写的重试次数。"),
            DemoProperty.Bool("files.bom", "UTF-8 BOM", false, "写文件时是否输出 BOM。")));

    private void ToJson()
    {
        if (!Sample.ToJson(out var json, out var error))
        {
            _forward.Text = "失败：" + error;
            return;
        }

        _forward.Text = json;
        Log("文件格式演示：ToJson");
    }

    private void ToYaml()
    {
        if (!Sample.ToYaml(out var yaml, out var error))
        {
            _forward.Text = "失败：" + error;
            return;
        }

        _forward.Text = yaml;
        Log("文件格式演示：ToYaml");
    }

    private void Prettify()
    {
        Sample.ToJson(out var json, out _);
        if (!json.JsonPrettify(2, sortKeys: true, out var pretty, out var error))
        {
            _forward.Text = "失败：" + error;
            return;
        }

        _forward.Text = pretty;
        Log("文件格式演示：JsonPrettify（缩进 2、键排序）");
    }

    private void JsonToYaml()
    {
        Sample.ToJson(out var json, out _);
        if (!json.JsonToYaml(out var yaml, out var error))
        {
            _forward.Text = "失败：" + error;
            return;
        }

        _forward.Text = yaml;
        Log("文件格式演示：JsonToYaml");
    }

    private void FromJson()
    {
        Sample.ToJson(out var json, out _);
        if (!json.FromJson<Student>(out var student, out var error) || student is null)
        {
            _backward.Text = "失败：" + error;
            return;
        }

        _backward.Text = $"FromJson<Student> → Name={student.Name}, Age={student.Age}, City={student.City}  (error = null)";
        Log("文件格式演示：FromJson");
    }

    private void FromYaml()
    {
        Sample.ToYaml(out var yaml, out _);
        if (!yaml.FromYaml<Student>(out var student, out var error) || student is null)
        {
            _backward.Text = "失败：" + error;
            return;
        }

        _backward.Text = $"FromYaml<Student> → Name={student.Name}, Age={student.Age}, City={student.City}  (error = null)";
        Log("文件格式演示：FromYaml");
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
            lines.Add($"文件大小：{new FileInfo(csvPath).Length} 字节 · UTF-8 BOM：{(HasBom(csvPath) ? "有" : "无")}");
        }

        lines.Add($"ImportFromCsv → {(imported ? "true" : "false：" + importError)}");
        if (imported && back is not null)
        {
            lines.Add($"还原：{back.Rows.Count} 行 {back.Columns.Count} 列 → 首行 {string.Join(", ", back.Rows[0].ItemArray)}");
        }

        _backward.Text = string.Join('\n', lines);
        Log("文件格式演示：DataTable Csv 往返");
    }

    private static bool HasBom(string path)
    {
        using var stream = File.OpenRead(path);
        return stream.ReadByte() == 0xEF && stream.ReadByte() == 0xBB && stream.ReadByte() == 0xBF;
    }

    private static void Log(string message) => Logger.Info(message);
}
