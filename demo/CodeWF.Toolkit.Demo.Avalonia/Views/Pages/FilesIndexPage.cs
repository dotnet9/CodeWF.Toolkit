using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>
/// 数据字典：Files 包能力矩阵（依赖、AOT 注意点与边界）。
/// 读写在「格式读写」实测、压缩与配置在「压缩配置」实测，本页负责把边界与依赖讲清楚。
/// </summary>
public sealed class FilesIndexPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Files" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Files.Extensions;

        // Json：框架内置 System.Text.Json；AOT 请用 JsonTypeInfo<T> 重载
        var json = student.ToJson(out var error);

        // Yaml：YamlDotNet；反射式 SerializerBuilder 非 AOT 安全
        var yaml = student.ToYaml(out error);

        // Csv / Excel：DataTableExtensions（CsvHelper / MiniExcel）
        dataTable.ToExcel("students.xlsx");

        // 安全读写：带重试的字符串 / 字节读写
        await FileExtension.SafeWriteAllTextAsync("app.log", "hello");
        """;

    public FilesIndexPage()
    {
        Content = Ui.Page(
            Ui.Heading(I18n(DemoKeys.ItemFilesIndex)),
            Ui.Sub(I18n(DemoKeys.DescFilesIndex)),
            Ui.Card("能力矩阵",
                BuildMatrix()),
            Ui.Card("相关页面",
                Ui.Hint("「格式读写」页实测 Json / Yaml / Csv 往返；「压缩配置」页实测 7z 与 App.config。"
                        + "本页只做索引，避免同一批 API 在两处各写一遍。")),
            Ui.Card("往返演示（与「格式读写」页同一批 API）",
                Ui.Hint("点击左侧页签切到「格式读写」或「压缩配置」即可上手操作。")));
    }

    public override string TitleKey => DemoKeys.ItemFilesIndex;

    public override string DescriptionKey => DemoKeys.DescFilesIndex;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() =>
    [
        new PropertyItem
        {
            Id = "files.format",
            Name = "默认格式",
            Kind = PropertyItemKind.Choice,
            Options = ["Json", "Yaml", "Csv", "Excel"],
            Value = "Yaml",
            DefaultValue = "Yaml",
            Description = "演示页默认使用的序列化格式。",
        },
        new PropertyItem
        {
            Id = "files.aot",
            Name = "AOT 友好模式",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "开启后提示改用 JsonTypeInfo<T> / 静态源生成序列化。",
        },
    ];

    private static Control BuildMatrix()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("120,260,*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto"),
        };

        var rows = new (string Capability, string Api, string Dependency, string Note)[]
        {
            ("Json", "JsonExtensions.ToJson / FromJson", "框架内置 System.Text.Json", "反射重载标记 RequiresUnreferencedCode，AOT 请用 JsonTypeInfo<T>"),
            ("Yaml", "YamlExtensions.ToYaml / FromYaml", "YamlDotNet", "基于反射的 SerializerBuilder，AOT 请改用静态源生成"),
            ("Csv / Excel", "DataTableExtensions", "CsvHelper / MiniExcel", "面向 DataTable 往返，适合批处理导出"),
            ("7z 压缩", "SevenZipCompressor.Zip / Decompress", "SharpCompress", "分卷与加密需评估 SharpCompress 能力边界"),
            ("App.config", "AppConfigHelper", "System.Configuration.ConfigurationManager", "非 trim-safe，AOT 请用文件路径重载"),
            ("安全读写", "FileExtension", "—", "带重试的字符串 / 字节读写"),
        };

        for (var column = 0; column < 4; column++)
        {
            var header = new TextBlock
            {
                Text = new[] { "能力", "API", "依赖", "注意" }[column],
                FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
                FontSize = 11.5,
                Margin = new global::Avalonia.Thickness(0, 0, 12, 6),
            };
            Grid.SetRow(header, 0);
            Grid.SetColumn(header, column);
            grid.Children.Add(header);
        }

        for (var i = 0; i < rows.Length; i++)
        {
            var values = new[] { rows[i].Capability, rows[i].Api, rows[i].Dependency, rows[i].Note };
            for (var column = 0; column < values.Length; column++)
            {
                var cell = new TextBlock
                {
                    Text = values[column],
                    FontSize = 11.5,
                    TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                    Opacity = column == 0 ? 0.95 : 0.75,
                    Margin = new global::Avalonia.Thickness(0, 0, 12, 4),
                };
                Grid.SetRow(cell, i + 1);
                Grid.SetColumn(cell, column);
                grid.Children.Add(cell);
            }
        }

        return grid;
    }

    private static string I18n(string key) => CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(key);
}
