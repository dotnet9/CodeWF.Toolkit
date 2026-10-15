using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
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

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.FilesIndex.Source.CSharp);

    public FilesIndexPage()
    {
        Content = Ui.Page(
            Ui.Heading(I18n(DemoKeys.FilesIndexExampleTitle)),
            Ui.Sub(I18n(DemoKeys.DescFilesIndex)),
            Ui.Card("@Demo.Content.FilesIndex.Content.CapabilityMatrix",
                BuildMatrix()),
            Ui.Card("@Demo.Content.FilesIndex.Content.RelatedPages",
                Ui.Hint("@Demo.Content.FilesIndex.Content.TryJSONYAMLCSVRoundTripsOnFile"
                        + L.Instance.GetResource(DemoResources.Content.FilesIndex.Content.ThisPageIndexesTheAPIsUseTheLinked))),
            Ui.Card("@Demo.Content.FilesIndex.Content.RoundTripDemosSameAPIsAsFileFormats",
                Ui.Hint("@Demo.Content.FilesIndex.Content.OpenFileFormatsOrArchiveConfigurationFromThe")));
    }

    public override string TitleKey => DemoKeys.FilesIndexExampleTitle;

    public override string DescriptionKey => DemoKeys.DescFilesIndex;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() =>
    [
        new PropertyItem
        {
            Id = "files.format",
            Name = L.Instance.GetResource(DemoResources.Content.FilesIndex.Properties.DefaultFormat),
            Kind = PropertyItemKind.Choice,
            Options = ["Json", "Yaml", "Csv", "Excel"],
            Value = "Yaml",
            DefaultValue = "Yaml",
            Description = L.Instance.GetResource(DemoResources.Content.FilesIndex.Properties.DefaultSerializationFormatInTheDemo),
        },
        new PropertyItem
        {
            Id = "files.aot",
            Name = L.Instance.GetResource(DemoResources.Content.FilesIndex.Properties.AOTFriendlyMode),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.FilesIndex.Properties.RecommendJsonTypeInfoTAndStaticGeneratedSerializersWhen),
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
            ("Json", "JsonExtensions.ToJson / FromJson", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.BuiltInSystemTextJson), L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.ReflectionOverloadsRequireUnreferencedCodeUseJsonTypeInfoT)),
            ("Yaml", "YamlExtensions.ToYaml / FromYaml", "YamlDotNet", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.ReflectionBasedSerializerBuilderUseStaticSourceGenerationFor)),
            ("Csv / Excel", "DataTableExtensions", "CsvHelper / MiniExcel", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.DataTableRoundTripsForBatchExports)),
            (L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.ArchiveCompression), "SevenZipCompressor.Zip / Decompress", "SharpCompress", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.CheckSharpCompressSupportBeforeUsingSplitArchivesOr)),
            ("App.config", "AppConfigHelper", "System.Configuration.ConfigurationManager", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.NotTrimSafeUseFilePathOverloadsFor)),
            (L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.SafeFileIO), "FileExtension", "—", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.StringByteFileIOWithRetries)),
        };

        for (var column = 0; column < 4; column++)
        {
            var header = new TextBlock
            {
                Text = new[] { L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.Capability), "API", L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.Dependencies), L.Instance.GetResource(DemoResources.Content.FilesIndex.BuildMatrix.Notes) }[column],
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

    private static string I18n(string key) => CodeWF.Toolkit.Lang.I18nManager.Instance.GetResource(key);
}
