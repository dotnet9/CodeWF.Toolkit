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

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Image;

        // 多尺寸合成：一张 PNG → 多帧 ico
        await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [16, 24, 32, 48, 64, 128, 256]);

        // 拆分：从源图分别按尺寸生成独立 ico 文件
        await ImageHelper.SeparateGenerateIcon("logo.png", "out/icons", [16, 32]);

        // 底层是 Magick.NET：也可以直接用它做转换 / 缩放 / 水印
        using var image = new MagickImage("photo.jpg");
        image.Resize(800, 0);
        image.Write("photo.webp");
        """;

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
        _output = Ui.Code("等待操作（所有文件写入 %TEMP%\\CodeWFToolkitDemo\\image）", output: true);
        _preview = new WrapPanel();

        var merge = Ui.Card("合成（MergeGenerateIcon）",
            Ui.Field("源图片", Ui.Key(_sourceImage)),
            Ui.Field("输出文件", _destIcon),
            Ui.Row(Ui.Button("MergeGenerateIcon", Merge, primary: true),
                Ui.Button("用仓库 logo 重置", () => _destIcon.Text = "logo.ico")),
            Ui.Card("尺寸预览（生成后填充）", _preview),
            Ui.Hint("尺寸取自右侧属性面板的“包含尺寸”，默认 16,24,32,48,64,128,256。"));

        var separate = Ui.Card("拆分（SeparateGenerateIcon）",
            Ui.Field("源图标", Ui.Key(_sourceImage)),
            Ui.Field("输出目录", Ui.Key(Path.Combine(_outputDirectory, "icons"))),
            Ui.Field("尺寸列表", _separateSizes),
            Ui.Row(Ui.Button("SeparateGenerateIcon", Separate)),
            _output);

        var others = Ui.Card("其它图像能力（同一套 Magick.NET 基础）",
            Ui.Code("""
                格式转换    png / jpg / webp / bmp 互转，可指定质量
                缩放        等比缩放、指定宽高
                水印        文字 / 图片水印，位置与透明度
                ico         多尺寸合成、拆分（本页）
                二维码      QrCodeGenerator（见“二维码与处理”页）
                """));

        Content = Ui.Page(
            Ui.Heading("图标生成：多尺寸 ico 合成与拆分"),
            Ui.Sub("ImageHelper.MergeGenerateIcon / SeparateGenerateIcon，底层 Magick.NET-Q16；点按钮即在临时目录真实生成。"),
            Ui.TwoColumns(merge, separate),
            others);

        _destIcon.Width = 200;
        _separateSizes.Width = 200;
    }

    public override string TitleKey => DemoKeys.ItemImageIcon;

    public override string DescriptionKey => DemoKeys.DescImageIcon;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("ic.opt", "图标选项", "ico 合成参数。",
            DemoProperty.ReadOnly("ic.sizes", "包含尺寸", string.Join(",", DefaultSizes)),
            DemoProperty.Bool("ic.compress", "PNG 压缩", true, "Magick.NET 写出时的压缩开关。"),
            DemoProperty.Number("ic.quality", "质量", 92, 1, 100, 1, "%", "写出图片质量。")),
        DemoProperty.Category("ic.io", "输入输出", "演示目录与源图。",
            DemoProperty.ReadOnly("ic.source", "源图", "Assets/logo.png"),
            DemoProperty.ReadOnly("ic.dest", "输出目录", Path.Combine("%TEMP%", "CodeWFToolkitDemo", "image"))));

    private async void Merge()
    {
        try
        {
            Directory.CreateDirectory(_outputDirectory);
            if (!File.Exists(_sourceImage))
            {
                _output.Text = $"源图片不存在：{_sourceImage}";
                return;
            }

            var dest = Path.Combine(_outputDirectory, Path.GetFileName(_destIcon.Text ?? "logo.ico"));
            await ImageHelper.MergeGenerateIcon(_sourceImage, dest, _sizes);
            var size = new FileInfo(dest).Length;
            _output.Text = $"MergeGenerateIcon(\"logo.png\", \"{Path.GetFileName(dest)}\", [{string.Join(",", _sizes)}])\n"
                           + $"完成：{dest}\n{size} 字节 · {_sizes.Length} 个尺寸";
            FillPreview();
            Logger.Info($"图标合成完成：{_sizes.Length} 个尺寸，{size} 字节");
        }
        catch (Exception ex)
        {
            _output.Text = "合成失败：" + ex.Message;
            Logger.Error("图标合成失败", ex);
        }
    }

    private async void Separate()
    {
        try
        {
            if (!File.Exists(_sourceImage))
            {
                _output.Text = $"源图片不存在：{_sourceImage}";
                return;
            }

            var sizes = ParseSizes(_separateSizes.Text);
            var folder = Path.Combine(_outputDirectory, "icons");
            Directory.CreateDirectory(folder);
            await ImageHelper.SeparateGenerateIcon(_sourceImage, folder, sizes);

            var files = Directory.GetFiles(folder, "*.ico").Select(Path.GetFileName).OrderBy(name => name).ToList();
            _output.Text = $"SeparateGenerateIcon(\"logo.png\", \"icons\", [{string.Join(",", sizes)}])\n"
                           + $"完成，输出文件：\n  {string.Join("\n  ", files)}";
            Logger.Info($"图标拆分完成：{files.Count} 个文件");
        }
        catch (Exception ex)
        {
            _output.Text = "拆分失败：" + ex.Message;
            Logger.Error("图标拆分失败", ex);
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
            _preview.Children.Add(new Border
            {
                Width = px,
                Height = px,
                Margin = new Thickness(0, 0, 8, 8),
                CornerRadius = new CornerRadius(6),
                Background = Brushes.SteelBlue,
                Child = new TextBlock
                {
                    Text = size.ToString(),
                    Foreground = Brushes.White,
                    FontSize = Math.Max(9, px / 3.0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            });
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
