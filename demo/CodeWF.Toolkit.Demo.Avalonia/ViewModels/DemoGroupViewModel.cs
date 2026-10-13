using System.Collections.ObjectModel;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Demo.Avalonia.Views.Scenes;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>演示目录项（导航树叶子 / Ribbon 命令按钮）。</summary>
public class DemoItemViewModel : ReactiveObject
{
    private string? _customTitle;
    private bool _isRenaming;
    private bool _isFilteredOut;

    public DemoItemViewModel(string titleKey, Func<DemoPageBase> pageFactory)
    {
        TitleKey = titleKey;
        PageFactory = pageFactory;
        I18nManager.Instance.CultureChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(Title));
            this.RaisePropertyChanged(nameof(DisplayTitle));
        };
    }

    /// <summary>标题的 Lang 键，同时作为文档唯一标识。</summary>
    public string TitleKey { get; }

    public Func<DemoPageBase> PageFactory { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    /// <summary>导航树就地重命名后的标题；为空表示沿用资源标题。</summary>
    public string? CustomTitle
    {
        get => _customTitle;
        set
        {
            if (Equals(_customTitle, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _customTitle, value);
            this.RaisePropertyChanged(nameof(DisplayTitle));
            this.RaisePropertyChanged(nameof(HasCustomTitle));
        }
    }

    public bool HasCustomTitle => !string.IsNullOrEmpty(CustomTitle);

    /// <summary>界面显示名：自定义优先，否则资源标题。</summary>
    public string DisplayTitle => string.IsNullOrEmpty(CustomTitle) ? Title : CustomTitle!;

    /// <summary>导航树是否正把该节点渲染成输入框。</summary>
    public bool IsRenaming
    {
        get => _isRenaming;
        set => this.RaiseAndSetIfChanged(ref _isRenaming, value);
    }

    /// <summary>被导航过滤框排除（仅隐藏，不改变集合）。</summary>
    public bool IsFilteredOut
    {
        get => _isFilteredOut;
        set => this.RaiseAndSetIfChanged(ref _isFilteredOut, value);
    }

    public string Icon => DemoIcons.ResolveItem(TitleKey);

    public override string ToString() => DisplayTitle;
}

/// <summary>演示分组（左树组节点 = Ribbon 页签名）。</summary>
public class DemoGroupViewModel : ReactiveObject
{
    private bool _isExpanded = true;
    private bool _isFilteredOut;

    public DemoGroupViewModel(string titleKey, string icon, IEnumerable<DemoItemViewModel> items)
    {
        TitleKey = titleKey;
        Icon = icon;
        Items = new ObservableCollection<DemoItemViewModel>(items);
        ToggleExpandCommand = ReactiveCommand.Create(() => { IsExpanded = !IsExpanded; });
        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(Title));
    }

    public string TitleKey { get; }

    public string Icon { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public ObservableCollection<DemoItemViewModel> Items { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary>被导航过滤框排除（仅隐藏，不改变集合）。</summary>
    public bool IsFilteredOut
    {
        get => _isFilteredOut;
        set => this.RaiseAndSetIfChanged(ref _isFilteredOut, value);
    }

    public ReactiveCommand<RxVoid, RxVoid> ToggleExpandCommand { get; }

    /// <summary>该分组下被就地重命名过的叶子数量。</summary>
    public int RenamedCount => Items.Count(i => i.HasCustomTitle);

    public override string ToString() => Title;
}

/// <summary>
/// 图标映射（与设计原型一致）。使用几何符号而非 Emoji：
/// 字形在浅深 / 6 色板下颜色统一，且不会出现彩色 Emoji 与主题冲突。
/// 实现期若要换成矢量图标，可整体替换为 Semi / Ursa / Iconica 的图标控件。
/// </summary>
public static class DemoIcons
{
    public static string ResolveItem(string titleKey) => titleKey switch
    {
        DemoKeys.ItemWelcome => "⌂",
        DemoKeys.ItemRuntimeFlow => "▶",
        DemoKeys.ItemLabFlow => "⚗",
        DemoKeys.ItemCore => "◈",
        DemoKeys.ItemBus => "◉",
        DemoKeys.ItemBusIntegrations => "⇄",
        DemoKeys.ItemFilesFormat => "▤",
        DemoKeys.ItemFilesArchive => "▣",
        DemoKeys.ItemFilesIndex => "☰",
        DemoKeys.ItemImageIcon => "▥",
        DemoKeys.ItemImageQr => "▦",
        DemoKeys.ItemLogCore => "☰",
        DemoKeys.ItemLogMicrosoft => "⊕",
        DemoKeys.ItemLoggingOptions => "⚙",
        DemoKeys.ItemNetPacket => "▦",
        DemoKeys.ItemNetSocket => "⇅",
        _ => "•",
    };

    public static string ResolveGroup(string titleKey) => titleKey switch
    {
        DemoKeys.GroupHome => "⌂",
        DemoKeys.GroupRuntime => "▶",
        DemoKeys.GroupLab => "⚗",
        DemoKeys.GroupCore => "◈",
        DemoKeys.GroupBus => "◉",
        DemoKeys.GroupFiles => "▤",
        DemoKeys.GroupLogging => "☰",
        _ => "•",
    };
}

/// <summary>
/// 演示目录：分组与 Ribbon 页签一一镜像（组节点 = 页签名，叶子 = 演示命令）。
/// 信息架构与设计原型一致 ——「场景优先（运行态工作台 / 能力实验室）+ 能力字典（核心 / 通信 / 数据 / 日志）」。
/// Prism 只做 Shell 解析，页面与 VM 不进容器，按目录工厂直接构造。
/// </summary>
public static class DemoCatalog
{
    public static ObservableCollection<DemoGroupViewModel> CreateGroups() =>
    [
        new(DemoKeys.GroupHome, "⌂",
        [
            new DemoItemViewModel(DemoKeys.ItemWelcome, () => new WelcomePage()),
        ]),
        // ── 场景页：完整工作流，内部按步骤复用下面的能力页 ──
        new(DemoKeys.GroupRuntime, "▶",
        [
            new DemoItemViewModel(DemoKeys.ItemRuntimeFlow, () => new RuntimeFlowPage()),
            new DemoItemViewModel(DemoKeys.ItemBus, () => new EventBusPage()),
            new DemoItemViewModel(DemoKeys.ItemLogCore, () => new LoggingCorePage()),
            new DemoItemViewModel(DemoKeys.ItemLogMicrosoft, () => new LoggingMicrosoftPage()),
        ]),
        new(DemoKeys.GroupLab, "⚗",
        [
            new DemoItemViewModel(DemoKeys.ItemLabFlow, () => new LabFlowPage()),
            new DemoItemViewModel(DemoKeys.ItemFilesFormat, () => new FilesFormatPage()),
            new DemoItemViewModel(DemoKeys.ItemFilesArchive, () => new FilesArchivePage()),
            new DemoItemViewModel(DemoKeys.ItemImageIcon, () => new ImageIconPage()),
            new DemoItemViewModel(DemoKeys.ItemImageQr, () => new ImageQrPage()),
        ]),
        // ── 能力字典 ──
        new(DemoKeys.GroupCore, "◈",
        [
            new DemoItemViewModel(DemoKeys.ItemCore, () => new CorePage()),
        ]),
        new(DemoKeys.GroupBus, "◉",
        [
            new DemoItemViewModel(DemoKeys.ItemBusIntegrations, () => new EventBusIntegrationsPage()),
            new DemoItemViewModel(DemoKeys.ItemNetPacket, () => new NetPacketPage()),
            new DemoItemViewModel(DemoKeys.ItemNetSocket, () => new NetSocketPage()),
        ]),
        new(DemoKeys.GroupFiles, "▤",
        [
            new DemoItemViewModel(DemoKeys.ItemFilesIndex, () => new FilesIndexPage()),
        ]),
        new(DemoKeys.GroupLogging, "☰",
        [
            new DemoItemViewModel(DemoKeys.ItemLoggingOptions, () => new LoggingOptionsPage()),
        ]),
    ];
}
