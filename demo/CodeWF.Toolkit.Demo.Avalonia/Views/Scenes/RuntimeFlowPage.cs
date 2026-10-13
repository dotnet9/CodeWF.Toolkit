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

    public override string XamlCode => """
        <!-- 运行态工作台：同一套处理器模型，进程内 / 集成 / 分布式三种形态 -->
        builder.Services.AddEventBus(assemblies);
        app.UseEventBus();

        var server = new EventServer();
        server.Start("127.0.0.1", 39561);
        await client.ConnectAsync("127.0.0.1", 39561);
        """;

    public override string CSharpCode => """
        // 场景页把「启动 → 连接 → 发布 → 查询 → 观测」编成一条工作流，
        // 每一步复用能力字典里的同一个页面实例：
        AddStep(DemoKeys.StepServer, new EventBusPage());
        AddStep(DemoKeys.StepConnect, new EventBusIntegrationsPage());
        AddStep(DemoKeys.StepPublish, new EventBusPage());
        AddStep(DemoKeys.StepQuery, new EventBusIntegrationsPage());
        AddStep(DemoKeys.StepObserve, new LoggingCorePage());
        AttachSceneShell();

        // 运行态的日志与总线是同一个进程里的两侧：
        Logger.Info("EventServer 启动成功，服务地址是：127.0.0.1:39561");
        client.Publish("demo.greeting", "hello", out _);
        """;
}
