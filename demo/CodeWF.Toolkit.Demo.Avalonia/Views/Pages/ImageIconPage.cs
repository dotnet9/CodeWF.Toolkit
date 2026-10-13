using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
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

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.ImageIconPage.CSharpSnippet001");

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
        _output = Ui.Code("@Demo.Text.ImageIconPage.Content002", output: true);
        _preview = new WrapPanel();

        var merge = Ui.Card("@Demo.Text.ImageIconPage.merge003",
            Ui.Field(L.Instance.GetResource("Demo.Text.ImageIconPage.merge004"), Ui.Key(_sourceImage)),
            Ui.Field(L.Instance.GetResource("Demo.Text.ImageIconPage.merge005"), _destIcon),
            Ui.Row(Ui.Button("MergeGenerateIcon", Merge, primary: true),
                Ui.Button("@Demo.Text.ImageIconPage.merge006", () => _destIcon.Text = "logo.ico")),
            Ui.Card("@Demo.Text.ImageIconPage.merge007", _preview),
            Ui.Hint("@Demo.Text.ImageIconPage.merge008"));

        var separate = Ui.Card("@Demo.Text.ImageIconPage.separate009",
            Ui.Field(L.Instance.GetResource("Demo.Text.ImageIconPage.separate010"), Ui.Key(_sourceImage)),
            Ui.Field(L.Instance.GetResource("Demo.Text.ImageIconPage.separate011"), Ui.Key(Path.Combine(_outputDirectory, "icons"))),
            Ui.Field(L.Instance.GetResource("Demo.Text.ImageIconPage.separate012"), _separateSizes),
            Ui.Row(Ui.Button("SeparateGenerateIcon", Separate)),
            _output);

        var others = Ui.Card("@Demo.Text.ImageIconPage.others013",
            Ui.Code("@Demo.Text.ImageIconPage.others014"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.ImageIconPage.Content015"),
            Ui.Sub("@Demo.Text.ImageIconPage.Content016"),
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
        DemoProperty.Category("ic.opt", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties017"), L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties018"),
            DemoProperty.ReadOnly("ic.sizes", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties019"), string.Join(",", DefaultSizes)),
            DemoProperty.Bool("ic.compress", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties020"), true, L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties021")),
            DemoProperty.Number("ic.quality", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties022"), 92, 1, 100, 1, "%", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties023"))),
        DemoProperty.Category("ic.io", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties024"), L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties025"),
            DemoProperty.ReadOnly("ic.source", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties026"), "Assets/logo.png"),
            DemoProperty.ReadOnly("ic.dest", L.Instance.GetResource("Demo.Text.ImageIconPage.BuildProperties027"), Path.Combine("%TEMP%", "CodeWFToolkitDemo", "image"))));

    private async void Merge()
    {
        try
        {
            Directory.CreateDirectory(_outputDirectory);
            if (!File.Exists(_sourceImage))
            {
                _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ImageIconPage.Merge028"), _sourceImage);
                return;
            }

            var dest = Path.Combine(_outputDirectory, Path.GetFileName(_destIcon.Text ?? "logo.ico"));
            await ImageHelper.MergeGenerateIcon(_sourceImage, dest, _sizes);
            var size = new FileInfo(dest).Length;
            _output.Text = $"MergeGenerateIcon(\"logo.png\", \"{Path.GetFileName(dest)}\", [{string.Join(",", _sizes)}])\n"
                           + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ImageIconPage.Merge029"), dest, size, _sizes.Length);
            FillPreview();
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ImageIconPage.Merge030"), _sizes.Length, size));
        }
        catch (Exception ex)
        {
            _output.Text = L.Instance.GetResource("Demo.Text.ImageIconPage.Merge031") + ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Text.ImageIconPage.Merge032"), ex);
        }
    }

    private async void Separate()
    {
        try
        {
            if (!File.Exists(_sourceImage))
            {
                _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ImageIconPage.Separate033"), _sourceImage);
                return;
            }

            var sizes = ParseSizes(_separateSizes.Text);
            var folder = Path.Combine(_outputDirectory, "icons");
            Directory.CreateDirectory(folder);
            await ImageHelper.SeparateGenerateIcon(_sourceImage, folder, sizes);

            var files = Directory.GetFiles(folder, "*.ico").Select(Path.GetFileName).OrderBy(name => name).ToList();
            _output.Text = $"SeparateGenerateIcon(\"logo.png\", \"icons\", [{string.Join(",", sizes)}])\n"
                           + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ImageIconPage.Separate034"), string.Join("\n  ", files));
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.ImageIconPage.Separate035"), files.Count));
        }
        catch (Exception ex)
        {
            _output.Text = L.Instance.GetResource("Demo.Text.ImageIconPage.Separate036") + ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Text.ImageIconPage.Separate037"), ex);
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
