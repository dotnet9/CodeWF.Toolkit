using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Image;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>图标生成页：MergeGenerateIcon 多尺寸 ico 合成 / SeparateGenerateIcon 拆分（Magick.NET 真实生成）。</summary>
public sealed class ImageIconPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Image" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.ImageIcon.Source.CSharp);

    private static readonly uint[] DefaultSizes = [16, 24, 32, 48, 64, 128, 256];

    private readonly string _sourceImage;
    private readonly string _outputDirectory;
    private readonly TextBox _destIcon;
    private readonly TextBox _separateSizes;
    private readonly SelectableTextBlock _output;
    private readonly WrapPanel _preview;

    private uint[] _sizes = DefaultSizes;

    public ImageIconPage()
    {
        _outputDirectory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "image");
        _sourceImage = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");

        _destIcon = Ui.Input("logo.ico");
        _separateSizes = Ui.Input("16,32");
        _output = Ui.Code("@Demo.Content.ImageIcon.Content.ReadyOutputTEMPCodeWFToolkitDemoImage", output: true);
        _preview = new WrapPanel();

        var merge = Ui.Card("@Demo.Content.ImageIcon.merge.MergeMergeGenerateIcon",
            Ui.Field(L.Instance.GetResource(DemoResources.Content.ImageIcon.merge.SourceImage), Ui.Key(_sourceImage)),
            Ui.Field(L.Instance.GetResource(DemoResources.Content.ImageIcon.merge.OutputFile), _destIcon),
            Ui.Row(Ui.Button("MergeGenerateIcon", Merge, primary: true),
                Ui.Button("@Demo.Content.ImageIcon.merge.ResetToRepositoryLogo", () => _destIcon.Text = "logo.ico")),
            Ui.Card("@Demo.Content.ImageIcon.merge.SizePreviewsPopulatedAfterGeneration", _preview),
            Ui.Hint("@Demo.Content.ImageIcon.merge.SizesAreSetByIncludedSizesInThe"));

        var separate = Ui.Card("@Demo.Content.ImageIcon.separate.SeparateSeparateGenerateIcon",
            Ui.Field(L.Instance.GetResource(DemoResources.Content.ImageIcon.separate.SourceIcon), Ui.Key(_sourceImage)),
            Ui.Field(L.Instance.GetResource(DemoResources.Content.ImageIcon.separate.OutputDirectory), Ui.Key(Path.Combine(_outputDirectory, "icons"))),
            Ui.Field(L.Instance.GetResource(DemoResources.Content.ImageIcon.separate.SizeList), _separateSizes),
            Ui.Row(Ui.Button("SeparateGenerateIcon", Separate)),
            _output);

        var others = Ui.Card("@Demo.Content.ImageIcon.others.OtherImageCapabilitiesMagickNET",
            Ui.Code("@Demo.Content.ImageIcon.others.FormatsPNGJPGWebPBMPWithQualityOptions"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.ImageIcon.Content.IconsMergeAndSeparateMultipleSizes"),
            Ui.Sub("@Demo.Content.ImageIcon.Content.ImageHelperMergeGenerateIconSeparateGenerateIconUseMagickNETGenerateActual"),
            Ui.TwoColumns(merge, separate),
            others);

        _destIcon.Width = 200;
        _separateSizes.Width = 200;
    }

    public override string TitleKey => DemoPages.ImageIcon;

    public override string DescriptionKey => DemoKeys.DescImageIcon;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("ic.opt", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.IconOptions), L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.ICOGenerationParameters),
            DemoProperty.ReadOnly("ic.sizes", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.IncludedSizes), string.Join(",", DefaultSizes)),
            DemoProperty.Bool("ic.compress", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.PNGCompression), true, L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.MagickNETOutputCompressionSwitch)),
            DemoProperty.Number("ic.quality", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.Quality), 92, 1, 100, 1, "%", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.OutputImageQuality))),
        DemoProperty.Category("ic.io", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.InputOutput), L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.DemoDirectoryAndSourceImage),
            DemoProperty.ReadOnly("ic.source", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.SourceImage), "Assets/logo.png"),
            DemoProperty.ReadOnly("ic.dest", L.Instance.GetResource(DemoResources.Content.ImageIcon.Properties.OutputDirectory), Path.Combine("%TEMP%", "CodeWFToolkitDemo", "image"))));

    private async void Merge()
    {
        try
        {
            Directory.CreateDirectory(_outputDirectory);
            if (!File.Exists(_sourceImage))
            {
                _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ImageIcon.Merge.SourceImageNotFoundFormat), _sourceImage);
                return;
            }

            var dest = Path.Combine(_outputDirectory, Path.GetFileName(_destIcon.Text ?? "logo.ico"));
            await ImageHelper.MergeGenerateIcon(_sourceImage, dest, _sizes);
            var size = new FileInfo(dest).Length;
            _output.Text = $"MergeGenerateIcon(\"logo.png\", \"{Path.GetFileName(dest)}\", [{string.Join(",", _sizes)}])\n"
                           + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ImageIcon.Merge.CompleteBytesSizesFormat), dest, size, _sizes.Length);
            FillPreview();
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ImageIcon.Merge.IconGeneratedSizesBytesFormat), _sizes.Length, size));
        }
        catch (Exception ex)
        {
            _output.Text = L.Instance.GetResource(DemoResources.Content.ImageIcon.Merge.MergeFailed) + ex.Message;
            Logger.Error(L.Instance.GetResource(DemoResources.Content.ImageIcon.Merge.IconGenerationFailed), ex);
        }
    }

    private async void Separate()
    {
        try
        {
            if (!File.Exists(_sourceImage))
            {
                _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ImageIcon.Separate.SourceImageNotFoundFormat), _sourceImage);
                return;
            }

            var sizes = ParseSizes(_separateSizes.Text);
            var folder = Path.Combine(_outputDirectory, "icons");
            Directory.CreateDirectory(folder);
            await ImageHelper.SeparateGenerateIcon(_sourceImage, folder, sizes);

            var files = Directory.GetFiles(folder, "*.ico").Select(Path.GetFileName).OrderBy(name => name).ToList();
            _output.Text = $"SeparateGenerateIcon(\"logo.png\", \"icons\", [{string.Join(",", sizes)}])\n"
                           + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ImageIcon.Separate.CompleteOutputFilesFormat), string.Join("\n  ", files));
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ImageIcon.Separate.IconSeparatedFilesFormat), files.Count));
        }
        catch (Exception ex)
        {
            _output.Text = L.Instance.GetResource(DemoResources.Content.ImageIcon.Separate.SeparationFailed) + ex.Message;
            Logger.Error(L.Instance.GetResource(DemoResources.Content.ImageIcon.Separate.IconSeparationFailed), ex);
        }
    }

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "ic.sizes" && DemoProperty.Text(change) is { } text)
        {
            _sizes = ParseSizes(text);
        }
    }

    private void FillPreview()
    {
        _preview.Children.Clear();
        foreach (var size in _sizes)
        {
            var px = Math.Clamp(size, 14u, 64u);
            var preview = new Border
            {
                Width = px,
                Height = px,
                Margin = new Thickness(0, 0, 8, 8),
                CornerRadius = new CornerRadius(6),
                Child = new TextBlock
                {
                    Text = size.ToString(),
                    FontSize = Math.Max(9, px / 3.0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            preview.Bind(Border.BackgroundProperty, preview.GetResourceObservable("StudioPrimary"));
            ((TextBlock)preview.Child!).Bind(TextBlock.ForegroundProperty, preview.GetResourceObservable("StudioOnPrimary"));
            _preview.Children.Add(preview);
        }
    }

    private static uint[] ParseSizes(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return DefaultSizes;
        }

        var parsed = text.Split([',', '，', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => uint.TryParse(part.Trim(), out var size) ? size : 0u)
            .Where(size => size > 0)
            .Distinct()
            .OrderBy(size => size)
            .ToArray();
        return parsed.Length == 0 ? DefaultSizes : parsed;
    }
}
