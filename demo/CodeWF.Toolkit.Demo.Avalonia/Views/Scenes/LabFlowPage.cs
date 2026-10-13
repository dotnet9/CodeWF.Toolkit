using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Scenes;

/// <summary>
/// 场景二：能力实验室。
/// 步骤 = 格式转换 / 归档与配置 / 图标与二维码 / 导出，
/// 把 Files / Image 的能力编排成一条流水线，每一步复用能力字典里的同一批页面。
/// </summary>
public sealed class LabFlowPage : ScenePageBase
{
    public LabFlowPage()
    {
        AddStep(DemoKeys.StepFormat, new FilesFormatPage());
        AddStep(DemoKeys.StepArchive, new FilesArchivePage());
        AddStep(DemoKeys.StepImage, new ImageIconPage());
        AddStep(DemoKeys.StepExport, new ImageQrPage());
        AttachSceneShell();
    }

    public override string TitleKey => DemoKeys.ItemLabFlow;

    public override string DescriptionKey => DemoKeys.DescLabFlow;

    protected override string SummaryKey => DemoKeys.SummaryLab;

    public override string XamlCode => """
        <!-- 能力实验室：Files / Image 的入口 API 一览 -->
        <PackageReference Include="CodeWF.Toolkit.Files" />
        <PackageReference Include="CodeWF.Toolkit.Image" />

        await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [24, 32, 48]);
        QrCodeGenerator.GenerateQrCode("CodeWF", "https://codewf.com", "qr.png");
        """;

    public override string CSharpCode => """
        // 场景页把「格式转换 → 归档 → 图标/二维码 → 导出」编成一条流水线
        AddStep(DemoKeys.StepFormat, new FilesFormatPage());
        AddStep(DemoKeys.StepArchive, new FilesArchivePage());
        AddStep(DemoKeys.StepImage, new ImageIconPage());
        AddStep(DemoKeys.StepExport, new ImageQrPage());
        AttachSceneShell();

        // 各步底层就是能力字典里那批 API：
        var yaml = student.ToYaml(out var error);
        var compressor = new SevenZipCompressor(httpClient);
        compressor.Zip(sourceDirectory, "archive.7z");
        await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [16, 24, 32, 48, 64, 128, 256]);
        QrCodeGenerator.GenerateQrCode("CodeWF", "https://codewf.com", "qrcode.png");
        """;
}
