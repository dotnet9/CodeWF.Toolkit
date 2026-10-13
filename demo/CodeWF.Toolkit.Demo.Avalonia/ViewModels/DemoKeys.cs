namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>
/// 演示台的全部 Lang 键（与 design/CodeWF.Toolkit.Demo原型.html 的信息架构一致：
/// 场景优先 —— 运行态工作台 / 能力实验室，其余按能力域进「能力字典」）。
/// </summary>
public static class DemoKeys
{
    // 分组（Ribbon 页签 / 导航树组节点）
    public const string GroupHome = "Demo.Group.Home";
    public const string GroupRuntime = "Demo.Group.Runtime";
    public const string GroupLab = "Demo.Group.Lab";
    public const string GroupCore = "Demo.Group.Core";
    public const string GroupBus = "Demo.Group.Bus";
    public const string GroupFiles = "Demo.Group.Files";
    public const string GroupLogging = "Demo.Group.Logging";

    // 场景页（工作流）
    public const string ItemRuntimeFlow = "Demo.Item.RuntimeFlow";
    public const string ItemLabFlow = "Demo.Item.LabFlow";

    // 页面（标题）
    public const string ItemWelcome = "Demo.Item.Welcome";
    public const string ItemCore = "Demo.Item.Core";
    public const string ItemBus = "Demo.Item.Bus";
    public const string ItemBusIntegrations = "Demo.Item.BusIntegrations";
    public const string ItemFilesFormat = "Demo.Item.FilesFormat";
    public const string ItemFilesArchive = "Demo.Item.FilesArchive";
    public const string ItemFilesIndex = "Demo.Item.FilesIndex";
    public const string ItemImageIcon = "Demo.Item.ImageIcon";
    public const string ItemImageQr = "Demo.Item.ImageQr";
    public const string ItemLogCore = "Demo.Item.LogCore";
    public const string ItemLogMicrosoft = "Demo.Item.LogMicrosoft";
    public const string ItemLoggingOptions = "Demo.Item.LoggingOptions";
    public const string ItemNetPacket = "Demo.Item.NetPacket";
    public const string ItemNetSocket = "Demo.Item.NetSocket";

    // 页面（说明）
    public const string DescWelcome = "Demo.Page.Welcome.Desc";
    public const string DescRuntimeFlow = "Demo.Page.RuntimeFlow.Desc";
    public const string DescLabFlow = "Demo.Page.LabFlow.Desc";
    public const string DescCore = "Demo.Page.Core.Desc";
    public const string DescBus = "Demo.Page.Bus.Desc";
    public const string DescBusIntegrations = "Demo.Page.BusIntegrations.Desc";
    public const string DescFilesFormat = "Demo.Page.FilesFormat.Desc";
    public const string DescFilesArchive = "Demo.Page.FilesArchive.Desc";
    public const string DescFilesIndex = "Demo.Page.FilesIndex.Desc";
    public const string DescImageIcon = "Demo.Page.ImageIcon.Desc";
    public const string DescImageQr = "Demo.Page.ImageQr.Desc";
    public const string DescLogCore = "Demo.Page.LogCore.Desc";
    public const string DescLogMicrosoft = "Demo.Page.LogMicrosoft.Desc";
    public const string DescLoggingOptions = "Demo.Page.LoggingOptions.Desc";
    public const string DescNetPacket = "Demo.Page.NetPacket.Desc";
    public const string DescNetSocket = "Demo.Page.NetSocket.Desc";

    // 场景步骤
    public const string StepServer = "Demo.Scene.Step.Server";
    public const string StepConnect = "Demo.Scene.Step.Connect";
    public const string StepPublish = "Demo.Scene.Step.Publish";
    public const string StepQuery = "Demo.Scene.Step.Query";
    public const string StepObserve = "Demo.Scene.Step.Observe";
    public const string StepFormat = "Demo.Scene.Step.Format";
    public const string StepArchive = "Demo.Scene.Step.Archive";
    public const string StepImage = "Demo.Scene.Step.Image";
    public const string StepExport = "Demo.Scene.Step.Export";
    public const string SummaryRuntime = "Demo.Scene.Summary.Runtime";
    public const string SummaryLab = "Demo.Scene.Summary.Lab";

    // 工具面板
    public const string ToolNavigation = "Demo.Tool.Navigation";
    public const string ToolProperties = "Demo.Tool.Properties";
    public const string ToolLog = "Demo.Tool.Log";

    // 外壳
    public const string ShellPalette = "Demo.Shell.Palette";
    public const string ShellDark = "Demo.Shell.Dark";
    public const string ShellDemoShell = "Demo.Shell.DemoShell";
    public const string ShellReady = "Demo.Shell.Ready";
    public const string ShellReset = "Demo.Shell.Reset";
    public const string ShellWriteLogs = "Demo.Shell.WriteLogs";
    public const string ShellQuickSave = "Demo.Shell.QuickSave";
    public const string ShellQuickSettings = "Demo.Shell.QuickSettings";
    public const string ShellQuickGuide = "Demo.Shell.QuickGuide";
    public const string ShellStatusReady = "Demo.Shell.StatusReady";

    // 外壳实体化件：通知中心 / 命令面板 / 密度 / 布局记忆 / 就地重命名
    public const string ShellGroup = "Demo.Shell.Group";
    public const string ShellCommand = "Demo.Shell.Command";
    public const string ShellCommandPlaceholder = "Demo.Shell.CommandPlaceholder";
    public const string ShellCommandHint = "Demo.Shell.CommandHint";
    public const string ShellDensity = "Demo.Shell.Density";
    public const string ShellDensityCompact = "Demo.Shell.DensityCompact";
    public const string ShellDensityComfort = "Demo.Shell.DensityComfort";
    public const string ShellLayoutSaved = "Demo.Shell.LayoutSaved";
    public const string ShellLayoutSaveFailed = "Demo.Shell.LayoutSaveFailed";
    public const string ShellLayoutLoaded = "Demo.Shell.LayoutLoaded";
    public const string ShellResetRenames = "Demo.Shell.ResetRenames";
    public const string ShellResetRenamesHint = "Demo.Shell.ResetRenamesHint";
    public const string ShellRenameHint = "Demo.Shell.RenameHint";
    public const string ShellNotificationDismiss = "Demo.Shell.NotificationDismiss";
    public const string ShellNotifyDemo = "Demo.Shell.NotifyDemo";
    public const string ShellNotifyDemoHint = "Demo.Shell.NotifyDemoHint";
    public const string ShellNavFilter = "Demo.Shell.NavFilter";
    public const string ShellNavEmpty = "Demo.Shell.NavEmpty";

    // ShowMeCode 三段
    public const string SegmentDemo = "Demo.Segment.Demo";
    public const string SegmentXaml = "Demo.Segment.Xaml";
    public const string SegmentCSharp = "Demo.Segment.CSharp";

    // 色板
    public const string PaletteDefault = "Demo.Palette.Default";
    public const string PaletteAquatic = "Demo.Palette.Aquatic";
    public const string PaletteDesert = "Demo.Palette.Desert";
    public const string PaletteDusk = "Demo.Palette.Dusk";
    public const string PaletteNightSky = "Demo.Palette.NightSky";
    public const string PaletteHighContrast = "Demo.Palette.HighContrast";
}
