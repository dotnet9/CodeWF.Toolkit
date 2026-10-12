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

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Image;
        using ImageMagick;

        // 生成二维码：标题 + 内容 + 副标题，白底蓝边 500×600/630
        QrCodeGenerator.GenerateQrCode(
            title: "CodeWF.Toolkit",
            content: "https://github.com/dotnet9/CodeWF.Toolkit",
            imagePath: "qrcode.png",
            subTitle: "扫码访问仓库");

        // 生成结果是普通图片文件，可继续交给 Magick.NET 处理
        using var image = new MagickImage("qrcode.png");
        image.Resize(300, 0);
        image.Write("qrcode-preview.png");
        """;

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
        _subTitle = Ui.Input("扫码访问仓库", 200);
        _preview = new AvaloniaImage
        {
            Width = 220,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _output = Ui.Code("等待生成（文件写入 %TEMP%\\CodeWFToolkitDemo\\image）", output: true);

        var generate = Ui.Card("生成二维码（QrCodeGenerator）",
            Ui.Field("标题", _title),
            Ui.Field("内容", _content),
            Ui.Field("副标题", _subTitle),
            Ui.Row(Ui.Button("生成二维码", Generate, primary: true)),
            _output,
            Ui.Hint("ZXing.Net.Bindings.Magick 生成位图后用 Magick.NET 绘制标题与副标题：500×600（有副标题时 630），纠错级别 H、UTF-8。"));

        var preview = Ui.Card("预览（生成结果缩放显示）",
            _preview,
            Ui.Code("""
                var qr = QrCodeGenerator.GenerateQrCode(title, content, imagePath, subTitle);
                using var image = new MagickImage(imagePath);
                image.Resize(300, 0);
                """, output: true));

        var pipeline = Ui.Card("图片处理管线（ImageHelper / Magick.NET）",
            Ui.Code("""
                格式转换   new MagickImage("photo.jpg").Write("photo.webp");
                等比缩放   image.Resize(800, 0);
                指定宽高   image.Resize(800, 600);
                质量压制   image.Quality = 92;
                水印       文字 / 图片水印，九宫格定位与透明度
                ico        多尺寸合成与拆分（见“图标生成”页）
                """));

        Content = Ui.Page(
            Ui.Heading("二维码与图片处理"),
            Ui.Sub("QrCodeGenerator 输出普通图片文件，可与 ImageHelper / Magick.NET 无缝衔接；下方生成的是真实二维码。"),
            Ui.TwoColumns(generate, preview),
            pipeline);

        _content.Width = 260;
    }

    public override string TitleKey => DemoKeys.ItemImageQr;

    public override string DescriptionKey => DemoKeys.DescImageQr;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("qr.opt", "二维码选项", "QrCodeGenerator 的固定参数（当前实现内置）。",
            DemoProperty.ReadOnly("qr.size", "二维码尺寸", "400 × 400（最终画布 500 宽）"),
            DemoProperty.ReadOnly("qr.ecc", "纠错级别", "H（ErrorCorrectionLevel.H）"),
            DemoProperty.ReadOnly("qr.charset", "字符集", "UTF-8（DisableECI = true）")),
        DemoProperty.Category("img.opt", "图片处理", "后续处理参数示例。",
            DemoProperty.Number("img.quality", "输出质量", 92, 1, 100, 1, "%", "MagickImage.Quality。"),
            DemoProperty.Bool("img.preview", "生成预览", true, "生成后缩放显示预览。")));

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
                           + $"完成：{path}\n{size} 字节 · 100% 质量";
            _preview.Source = new Bitmap(path);
            Logger.Info($"二维码生成完成：{fileName}，{size} 字节");
        }
        catch (Exception ex)
        {
            _output.Text = "生成失败：" + ex.Message;
            Logger.Error("二维码生成失败", ex);
        }
    }
}
