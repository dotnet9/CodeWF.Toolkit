using System.Collections.ObjectModel;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>演示目录项（导航树叶子 / Ribbon 命令按钮）。</summary>
public class DemoItemViewModel : ReactiveObject
{
    public DemoItemViewModel(string titleKey, Func<DemoPageBase> pageFactory)
    {
        TitleKey = titleKey;
        PageFactory = pageFactory;
        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(Title));
    }

    /// <summary>标题的 Lang 键，同时作为文档唯一标识。</summary>
    public string TitleKey { get; }

    public Func<DemoPageBase> PageFactory { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public string Icon => DemoIcons.ResolveItem(TitleKey);

    public override string ToString() => Title;
}

/// <summary>演示分组（左树组节点 = Ribbon 页签名）。</summary>
public class DemoGroupViewModel : ReactiveObject
{
    private bool _isExpanded = true;

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

    public ReactiveCommand<RxVoid, RxVoid> ToggleExpandCommand { get; }

    public override string ToString() => Title;
}

/// <summary>图标映射（与设计原型一致）。</summary>
public static class DemoIcons
{
    public static string ResolveItem(string titleKey) => titleKey switch
    {
        DemoKeys.ItemWelcome => "🏠",
        DemoKeys.ItemCore => "🧰",
        DemoKeys.ItemBus => "📣",
        DemoKeys.ItemBusIntegrations => "🔌",
        DemoKeys.ItemFilesFormat => "📄",
        DemoKeys.ItemFilesArchive => "🗜️",
        DemoKeys.ItemImageIcon => "🎨",
        DemoKeys.ItemImageQr => "🔳",
        DemoKeys.ItemLogCore => "📋",
        DemoKeys.ItemLogMicrosoft => "🧾",
        DemoKeys.ItemNetPacket => "📦",
        DemoKeys.ItemNetSocket => "🔗",
        _ => "•",
    };

    public static string ResolveGroup(string titleKey) => titleKey switch
    {
        DemoKeys.GroupHome => "🏠",
        DemoKeys.GroupCore => "🧰",
        DemoKeys.GroupBus => "📡",
        DemoKeys.GroupFiles => "📁",
        DemoKeys.GroupImage => "🖼️",
        DemoKeys.GroupLogging => "📋",
        DemoKeys.GroupNet => "🌐",
        _ => "•",
    };
}

/// <summary>
/// 演示目录：分组与 Ribbon 页签一一镜像（组节点 = 页签名，叶子 = 演示命令）。
/// Prism 只做 Shell 解析，页面与 VM 不进容器，按目录工厂直接构造。
/// </summary>
public static class DemoCatalog
{
    public static ObservableCollection<DemoGroupViewModel> CreateGroups() =>
    [
        new(DemoKeys.GroupHome, "🏠",
        [
            new DemoItemViewModel(DemoKeys.ItemWelcome, () => new WelcomePage()),
        ]),
        new(DemoKeys.GroupCore, "🧰",
        [
            new DemoItemViewModel(DemoKeys.ItemCore, () => new CorePage()),
        ]),
        new(DemoKeys.GroupBus, "📡",
        [
            new DemoItemViewModel(DemoKeys.ItemBus, () => new EventBusPage()),
            new DemoItemViewModel(DemoKeys.ItemBusIntegrations, () => new EventBusIntegrationsPage()),
        ]),
        new(DemoKeys.GroupFiles, "📁",
        [
            new DemoItemViewModel(DemoKeys.ItemFilesFormat, () => new FilesFormatPage()),
            new DemoItemViewModel(DemoKeys.ItemFilesArchive, () => new FilesArchivePage()),
        ]),
        new(DemoKeys.GroupImage, "🖼️",
        [
            new DemoItemViewModel(DemoKeys.ItemImageIcon, () => new ImageIconPage()),
            new DemoItemViewModel(DemoKeys.ItemImageQr, () => new ImageQrPage()),
        ]),
        new(DemoKeys.GroupLogging, "📋",
        [
            new DemoItemViewModel(DemoKeys.ItemLogCore, () => new LoggingCorePage()),
            new DemoItemViewModel(DemoKeys.ItemLogMicrosoft, () => new LoggingMicrosoftPage()),
        ]),
        new(DemoKeys.GroupNet, "🌐",
        [
            new DemoItemViewModel(DemoKeys.ItemNetPacket, () => new NetPacketPage()),
            new DemoItemViewModel(DemoKeys.ItemNetSocket, () => new NetSocketPage()),
        ]),
    ];
}
