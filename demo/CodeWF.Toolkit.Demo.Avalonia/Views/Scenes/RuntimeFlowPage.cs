using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Scenes;

/// <summary>
/// A guided tour of independent event and logging examples, each with its own state.
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

    public override string TitleKey => DemoPages.RuntimeFlow;

    public override string DescriptionKey => DemoKeys.DescRuntimeFlow;

    protected override string SummaryKey => DemoKeys.SummaryRuntime;

    public override string XamlCode => L.Instance.GetResource(DemoResources.Content.RuntimeFlow.XamlCode.BuilderServicesAddEventBusAssembliesAppUseEventBusVarServer);

    public override string CSharpCode => L.Instance.GetResource(DemoResources.Content.RuntimeFlow.CSharpCode.AddStepDemoKeysStepServerNewEventBusPageAddStepDemoKeysStepConnect);
}
