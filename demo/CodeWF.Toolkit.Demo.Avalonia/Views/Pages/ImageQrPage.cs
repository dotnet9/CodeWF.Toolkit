using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;

// 别名：本文件所在命名空间下 Avalonia 会被解析成 CodeWF.Toolkit.Demo.Avalonia，必须用别名限定
using AvaloniaImage = Avalonia.Controls.Image;
using QrCodeGenerator = CodeWF.Toolkit.Image.QrCodeGenerator;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>二维码与图片处理页：QrCodeGenerator 真实生成二维码卡片并预览，附图片处理管线说明。</summary>
public sealed class ImageQrPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Image" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Content.ImageQr.Source.CSharp");

    private readonly TextBox _title;
    private readonly TextBox _content;
    private readonly TextBox _subTitle;
    private readonly AvaloniaImage _preview;
    private readonly SelectableTextBlock _output;
    private readonly string _outputDirectory;

    public ImageQrPage()
    {
        _outputDirectory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "image");
        _title = Ui.Input("CodeWF.Toolkit", 200);
        _content = Ui.Input("https://github.com/dotnet9/CodeWF.Toolkit");
        _subTitle = Ui.Input(L.Instance.GetResource("Demo.Content.ImageQr.Content.ScanToVisitTheRepository"), 200);
        _preview = new AvaloniaImage
        {
            Width = 220,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _output = Ui.Code("@Demo.Content.ImageQr.Content.ReadyToGenerateOutputTEMPCodeWFToolkitDemoImage", output: true);

        var generate = Ui.Card("@Demo.Content.ImageQr.generate.GenerateQRCodeQrCodeGenerator",
            Ui.Field(L.Instance.GetResource("Demo.Content.ImageQr.generate.Title"), _title),
            Ui.Field(L.Instance.GetResource("Demo.Content.ImageQr.generate.Content"), _content),
            Ui.Field(L.Instance.GetResource("Demo.Content.ImageQr.generate.Subtitle"), _subTitle),
            Ui.Row(Ui.Button("@Demo.Content.ImageQr.generate.GenerateQRCode", Generate, primary: true)),
            _output,
            Ui.Hint("@Demo.Content.ImageQr.generate.ZXingCreatesTheQRBitmapMagickNETAdds"));

        var preview = Ui.Card("@Demo.Content.ImageQr.preview.PreviewScaledGeneratedImage",
            _preview,
            Ui.Code("""
                var qr = QrCodeGenerator.GenerateQrCode(title, content, imagePath, subTitle);
                using var image = new MagickImage(imagePath);
                image.Resize(300, 0);
                """, output: true));

        var pipeline = Ui.Card("@Demo.Content.ImageQr.pipeline.ImagePipelineImageHelperMagickNET",
            Ui.Code("@Demo.Content.ImageQr.pipeline.ConvertNewMagickImagePhotoJpgWritePhotoWebp"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.ImageQr.Content.QRCodesAndImageProcessing"),
            Ui.Sub("@Demo.Content.ImageQr.Content.QrCodeGeneratorProducesImageFilesThatCanBeProcessed"),
            Ui.TwoColumns(generate, preview),
            pipeline);

        _content.Width = 260;
    }

    public override string TitleKey => DemoPages.ImageQr;

    public override string DescriptionKey => DemoKeys.DescImageQr;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("qr.opt", L.Instance.GetResource("Demo.Content.ImageQr.Properties.QRCodeOptions"), L.Instance.GetResource("Demo.Content.ImageQr.Properties.FixedQrCodeGeneratorParametersInTheCurrentImplementation"),
            DemoProperty.ReadOnly("qr.size", L.Instance.GetResource("Demo.Content.ImageQr.Properties.QRCodeSize"), L.Instance.GetResource("Demo.Content.ImageQr.Properties.400400FinalCanvasWidth500")),
            DemoProperty.ReadOnly("qr.ecc", L.Instance.GetResource("Demo.Content.ImageQr.Properties.ErrorCorrection"), "H（ErrorCorrectionLevel.H）"),
            DemoProperty.ReadOnly("qr.charset", L.Instance.GetResource("Demo.Content.ImageQr.Properties.CharacterSet"), "UTF-8（DisableECI = true）")),
        DemoProperty.Category("img.opt", L.Instance.GetResource("Demo.Content.ImageQr.Properties.ImageProcessing"), L.Instance.GetResource("Demo.Content.ImageQr.Properties.SampleProcessingOptions"),
            DemoProperty.Number("img.quality", L.Instance.GetResource("Demo.Content.ImageQr.Properties.OutputQuality"), 92, 1, 100, 1, "%", "MagickImage.Quality。"),
            DemoProperty.Bool("img.preview", L.Instance.GetResource("Demo.Content.ImageQr.Properties.GeneratePreview"), true, L.Instance.GetResource("Demo.Content.ImageQr.Properties.ScaleTheGeneratedImageForPreview"))));

    private void Generate()
    {
        try
        {
            Directory.CreateDirectory(_outputDirectory);
            var fileName = $"qrcode-{DateTime.Now:HHmmss}.png";
            var path = Path.Combine(_outputDirectory, fileName);

            var title = string.IsNullOrWhiteSpace(_title.Text) ? "CodeWF.Toolkit" : _title.Text!;
            var content = string.IsNullOrWhiteSpace(_content.Text) ? "https://github.com/dotnet9/CodeWF.Toolkit" : _content.Text!;
            var subTitle = _subTitle.Text ?? string.Empty;

            QrCodeGenerator.GenerateQrCode(title, content, path, subTitle);

            var size = new FileInfo(path).Length;
            _output.Text = $"GenerateQrCode(\"{title}\", \"{content}\", \"{fileName}\", \"{subTitle}\")\n"
                           + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.ImageQr.Generate.CompleteBytes100QualityFormat"), path, size);
            _preview.Source = new Bitmap(path);
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.ImageQr.Generate.QRCodeGeneratedBytesFormat"), fileName, size));
        }
        catch (Exception ex)
        {
            _output.Text = L.Instance.GetResource("Demo.Content.ImageQr.Generate.GenerationFailed") + ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Content.ImageQr.Generate.QRCodeGenerationFailed"), ex);
        }
    }
}
