using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
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

    public override string TitleKey => DemoPages.LabFlow;

    public override string DescriptionKey => DemoKeys.DescLabFlow;

    protected override string SummaryKey => DemoKeys.SummaryLab;

    public override string XamlCode => L.Instance.GetResource(DemoResources.Content.LabFlow.XamlCode.PackageReferenceIncludeCodeWFToolkitFilesPackageReferenceIncludeCodeWF);

    public override string CSharpCode => L.Instance.GetResource(DemoResources.Content.LabFlow.CSharpCode.AddStepDemoKeysStepFormatNewFilesFormatPageAddStepDemoKeysStepArchive);
}
