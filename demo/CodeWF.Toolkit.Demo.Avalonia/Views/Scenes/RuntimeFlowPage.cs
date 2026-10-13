using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Scenes;

/// <summary>
/// 场景一：运行态工作台。
/// 步骤 = 启动服务端 / 客户端连接 / 发布订阅 / 查询与集成 / 观测日志，
/// 把「总线 + 日志」串成一条连续流程，每一步复用能力字典里的同一批页面。
/// </summary>
public sealed class RuntimeFlowPage : ScenePageBase
{
    public RuntimeFlowPage()
    {
        AddStep(DemoKeys.StepServer, new EventBusPage());
        AddStep(DemoKeys.StepConnect, new EventBusIntegrationsPage());
        AddStep(DemoKeys.StepPublish, new EventBusPage());
        AddStep(DemoKeys.StepQuery, new EventBusIntegrationsPage());
        AddStep(DemoKeys.StepObserve, new LoggingCorePage());
        AttachSceneShell();
    }

    public override string TitleKey => DemoKeys.ItemRuntimeFlow;

    public override string DescriptionKey => DemoKeys.DescRuntimeFlow;

    protected override string SummaryKey => DemoKeys.SummaryRuntime;

    public override string XamlCode => L.Instance.GetResource("Demo.Text.RuntimeFlowPage.XamlCode001");

    public override string CSharpCode => L.Instance.GetResource("Demo.Text.RuntimeFlowPage.CSharpCode002");
}
