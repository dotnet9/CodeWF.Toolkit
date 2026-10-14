using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Styling;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.Infrastructure;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Logging;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>
/// Shell 视图模型：Ribbon 分组、导航树镜像、Dock 文档区、右侧 PropertyGrid 与底部日志面板的调度中枢。
/// 页面与文档 VM 不进 Prism 容器，按演示目录工厂直接构造。
/// </summary>
public partial class MainViewModel : ReactiveObject
{
    private DemoDocumentViewModel? _activeDocument;
    private DemoItemViewModel? _selectedNavItem;
    private string _statusText = string.Empty;
    private bool _ribbonExpanded = true;
    private AppearanceOption _selectedPalette;
    private bool _isDarkMode = true;
    private CultureOption _selectedCulture;
    private DemoGroupViewModel? _selectedGroup;
    private PropertyItem? _densityItem;
    private PropertyItem? _paletteItem;
    private PropertyItem? _modeItem;
    private PropertyItem? _langItem;
    private DemoDockFactory? _dockFactory;
    private RootDock? _layout;
    private Tool? _navTool;
    private Tool? _propTool;
    private Tool? _logTool;
    private DocumentDock? _docDock;

    public MainViewModel()
    {
        Groups = DemoCatalog.CreateGroups();
        Documents = new ObservableCollection<DemoDocumentViewModel>();

        OpenItemCommand = ReactiveCommand.Create<DemoItemViewModel>(OpenItem);
        CloseDocumentCommand = ReactiveCommand.Create<DemoDocumentViewModel>(CloseDocument);
        ToggleRibbonCommand = ReactiveCommand.Create(() => { RibbonExpanded = !RibbonExpanded; });
        TogglePanelCommand = ReactiveCommand.Create<string>(TogglePanel);
        QuickSaveCommand = ReactiveCommand.Create(SaveLayout);
        QuickResetCommand = ReactiveCommand.Create(ResetLayout);
        QuickSettingsCommand = ReactiveCommand.Create(() => TogglePanel("props"));
        WriteLogsCommand = ReactiveCommand.Create(WriteSampleLogs);

        Palettes = new ObservableCollection<AppearanceOption>
        {
            AppearanceOption.DotNet(),
            AppearanceOption.Aurora(),
            AppearanceOption.Ember(),
            AppearanceOption.Default(),
            AppearanceOption.Aquatic(),
            AppearanceOption.Desert(),
            AppearanceOption.Dusk(),
            AppearanceOption.NightSky(),
            AppearanceOption.HighContrast(),
        };
        _selectedPalette = Palettes[0];

        Cultures = new ObservableCollection<CultureOption>
        {
            new("zh-CN", L.Instance.GetResource("Demo.Text.MainViewModel.Content001")),
            new("en-US", "English"),
        };
        _selectedCulture = Cultures[0];

        GlobalProperties = BuildGlobalProperties();
        ApplyAppearance();

        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(StatusText));

        BuildLayout();
        OpenItem(Groups[0].Items[0]);


        // 外壳实体化件：通知中心 / 命令面板 / 密度 / 布局记忆 / 就地重命名。
        InitializeShellServices();
        if (LoadLayout())
        {
            Notify(DemoNotificationKind.Info, T(DemoKeys.ShellLayoutLoaded), LayoutPath);
        }
    }

    /// <summary>Ribbon 分组（左树镜像同一集合）。</summary>
    public IReadOnlyList<DemoRibbonSection> RibbonSections => SelectedGroup?.RibbonSections ?? [];

    public IReadOnlyList<DemoItemViewModel> RibbonItems => SelectedGroup?.Items.ToArray() ?? [];


    public ObservableCollection<DemoGroupViewModel> Groups { get; }

    /// <summary>已打开的演示文档。</summary>
    public ObservableCollection<DemoDocumentViewModel> Documents { get; }

    /// <summary>Dock 工作区布局（DockControl 绑定目标；重置布局时整体重建）。</summary>
    public RootDock? Layout => _layout;

    /// <summary>导航树当前选中叶子（TwoWay；程序侧同步时经 OpenItem 打开文档）。</summary>
    public DemoItemViewModel? SelectedNavItem
    {
        get => _selectedNavItem;
        set
        {
            if (Equals(_selectedNavItem, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedNavItem, value);
            if (value is not null)
            {
                OpenItem(value);
            }
        }
    }

    /// <summary>状态栏文本（打开/关闭文档、切换语言时刷新）。</summary>
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    /// <summary>Ribbon 当前页签（命令区随之切换；导航树点击时同步高亮）。</summary>
    public DemoGroupViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (ReferenceEquals(_selectedGroup, value)) return;
            this.RaiseAndSetIfChanged(ref _selectedGroup, value);
            if (value is not null) value.IsExpanded = true;
            this.RaisePropertyChanged(nameof(RibbonItems));
            this.RaisePropertyChanged(nameof(RibbonSections));
        }
    }

    /// <summary>当前活动文档；为空时右侧面板展示全局外观/语言属性（PropertyGrid 自我演示）。</summary>
    public DemoDocumentViewModel? ActiveDocument
    {
        get => _activeDocument;
        set
        {
            if (Equals(_activeDocument, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _activeDocument, value);
            foreach (var group in Groups)
            {
                foreach (var item in group.Items)
                    item.IsSelected = item.TitleKey == value?.TitleKey;
            }
            var selected = Groups.SelectMany(group => group.Items).FirstOrDefault(item => item.IsSelected);
            _selectedNavItem = selected;
            this.RaisePropertyChanged(nameof(SelectedNavItem));
            if (selected is not null)
                SelectedGroup = Groups.First(group => group.Items.Contains(selected));
            this.RaisePropertyChanged(nameof(ActivePropertyItems));
            this.RaisePropertyChanged(nameof(ActiveValueCommittedCommand));
            this.RaisePropertyChanged(nameof(ActiveBrowseRequestedCommand));
        }
    }

    private bool UsesGlobalProperties => ActiveDocument?.Page is Views.Pages.WelcomePage
        || ActiveDocument?.Page.Properties.Count is not > 0;

    public IReadOnlyList<PropertyItem> ActivePropertyItems =>
        UsesGlobalProperties ? GlobalProperties : ActiveDocument?.Page.Properties ?? GlobalProperties;

    public ICommand ActiveValueCommittedCommand =>
        UsesGlobalProperties ? GlobalValueCommittedCommand : ActiveDocument?.ValueCommittedCommand ?? GlobalValueCommittedCommand;

    public ICommand ActiveBrowseRequestedCommand =>
        UsesGlobalProperties ? GlobalBrowseRequestedCommand : ActiveDocument?.BrowseRequestedCommand ?? GlobalBrowseRequestedCommand;

    /// <summary>无文档打开时的全局设置属性（外观 / 语言），由 PropertyGrid 直接驱动 Shell。</summary>
    public IReadOnlyList<PropertyItem> GlobalProperties { get; private set; }

    public ReactiveCommand<PropertyValueChange, RxVoid> GlobalValueCommittedCommand { get; private set; } = null!;

    public ReactiveCommand<PropertyItem, RxVoid> GlobalBrowseRequestedCommand { get; private set; } = null!;

    public ReactiveCommand<DemoItemViewModel, RxVoid> OpenItemCommand { get; }

    public ReactiveCommand<DemoDocumentViewModel, RxVoid> CloseDocumentCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> ToggleRibbonCommand { get; }

    /// <summary>面板显隐切换（参数 nav / props / log）。</summary>
    public ReactiveCommand<string, RxVoid> TogglePanelCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> QuickSaveCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> QuickResetCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> QuickSettingsCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> WriteLogsCommand { get; }

    /// <summary>Ribbon 命令区折叠状态（双击页签切换）。</summary>
    public bool RibbonExpanded
    {
        get => _ribbonExpanded;
        set => this.RaiseAndSetIfChanged(ref _ribbonExpanded, value);
    }

    /// <summary>日志数据源：Logger 的 UI 事件流。</summary>
    public LogEventFeed LogSource => Logger.Events;

    public string LogFolder => DemoBootstrapper.LogFolder;

    public ObservableCollection<AppearanceOption> Palettes { get; }

    public AppearanceOption SelectedPalette
    {
        get => _selectedPalette;
        set
        {
            if (Equals(_selectedPalette, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedPalette, value);
            ApplyAppearance();
        }
    }

    /// <summary>浅/深模式；对固定配色的色板（水生等）无效果，切回默认色板时生效。</summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (Equals(_isDarkMode, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isDarkMode, value);
            ApplyAppearance();
        }
    }

    public ObservableCollection<CultureOption> Cultures { get; }

    public CultureOption SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (Equals(_selectedCulture, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedCulture, value);
            I18nManager.Instance.Culture = new CultureInfo(value.Name);
            this.RaisePropertyChanged(nameof(RibbonSections));
            SyncToolTitles();
            foreach (var palette in Palettes) palette.RefreshTitle();
            GlobalProperties = BuildGlobalProperties();
            SyncGlobalItems();
            foreach (var document in Documents)
            {
                document.Page.RefreshProperties();
                document.RefreshTitle();
                if (document.Page is Views.Pages.WelcomePage overview) overview.Overview.RefreshCulture();
            }
            this.RaisePropertyChanged(nameof(ActivePropertyItems));
            this.RaisePropertyChanged(nameof(NavFilterPlaceholder));
            this.RaisePropertyChanged(nameof(NavEmptyHint));
            this.RaisePropertyChanged(nameof(RenameHint));
            this.RaisePropertyChanged(nameof(ResetRenamesHint));
            this.RaisePropertyChanged(nameof(DensityLabel));
            this.RaisePropertyChanged(nameof(CommandPlaceholder));
            this.RaisePropertyChanged(nameof(CommandHint));
            this.RaisePropertyChanged(nameof(NotificationDismissHint));
            RaiseCommandState();
            StatusText = I18nManager.Instance.GetResource("Demo.Shell.Ready");
            Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.SelectedCulture002") + value.Name);
        }
    }

    /// <summary>单击导航叶子或 Ribbon 命令：打开（或激活）对应演示文档。</summary>
    public void OpenItem(DemoItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var existing = Documents.FirstOrDefault(d => d.TitleKey == item.TitleKey);
        if (existing is not null)
        {
            ActivateDocument(existing);
            return;
        }

        var page = item.PageFactory();
        if (page is Views.Pages.WelcomePage overview)
        {
            overview.Navigate = key =>
            {
                var destination = Groups.SelectMany(g => g.Items).FirstOrDefault(i => i.TitleKey == key);
                if (destination is not null) OpenItem(destination);
            };
            overview.TogglePanel = TogglePanel;
        }
        var document = new DemoDocumentViewModel(item, page);
        var dockDocument = _dockFactory?.CreateDocument() ?? new Document();
        dockDocument.Id = "Doc." + item.TitleKey;
        dockDocument.Title = document.DisplayTitle;
        dockDocument.Context = new Views.DemoDocumentView { DataContext = document };
        document.DockDocument = dockDocument;

        Documents.Add(document);
        if (_docDock is not null && _dockFactory is not null)
        {
            _dockFactory.AddDockable(_docDock, dockDocument);
            _dockFactory.SetActiveDockable(dockDocument);
            _dockFactory.SetFocusedDockable(_docDock, dockDocument);
        }

        ActiveDocument = document;
        page.OnDocumentOpened();
        SelectedGroup = Groups.FirstOrDefault(g => g.Items.Contains(item));

        if (!Equals(_selectedNavItem, item))
        {
            _selectedNavItem = item;
            this.RaisePropertyChanged(nameof(SelectedNavItem));
        }

        StatusText = I18nManager.Instance.GetResource(DemoKeys.ShellReady);
        Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.OpenItem003") + item.Title);
    }

    public void CloseDocument(DemoDocumentViewModel document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (_dockFactory is not null && document.DockDocument is not null)
        {
            _dockFactory.CloseDockable(document.DockDocument);
        }
        else
        {
            SyncDocumentClosed(document);
        }
    }

    private void ActivateDocument(DemoDocumentViewModel document)
    {
        if (_dockFactory is not null && document.DockDocument is { } dockDocument)
        {
            _dockFactory.SetActiveDockable(dockDocument);
            if (dockDocument.Owner is IDock owner)
                _dockFactory.SetFocusedDockable(owner, dockDocument);
        }

        ActiveDocument = document;
        SelectedGroup = Groups.FirstOrDefault(g => g.Items.Any(i => i.TitleKey == document.TitleKey));
    }

    /// <summary>Dock 侧关闭（页签 ✕）后的状态同步入口，仅由 <see cref="DemoDockFactory"/> 回调。</summary>
    public Task ShutdownAsync() => Task.WhenAll(Documents.Select(document => document.Page.OnDocumentClosedAsync()));

    private void SyncDocumentClosed(DemoDocumentViewModel document)
    {
        Documents.Remove(document);
        _ = document.Page.OnDocumentClosedAsync();
        if (ActiveDocument == document)
        {
            ActiveDocument = Documents.LastOrDefault();
        }

        StatusText = I18nManager.Instance.GetResource(DemoKeys.ShellReady);
        Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.SyncDocumentClosed004") + document.Title);
    }

    /// <summary>面板显隐切换：隐藏 = 移入 RootDock 隐藏集合，恢复 = 按 Id 取回。</summary>
    public void TogglePanel(string panel)
    {
        if (_dockFactory is null)
        {
            return;
        }

        switch (panel)
        {
            case "nav":
                ToggleTool(_navTool, I18nManager.Instance.GetResource(DemoKeys.ToolNavigation));
                break;
            case "props":
                ToggleTool(_propTool, I18nManager.Instance.GetResource(DemoKeys.ToolProperties));
                break;
            case "log":
                ToggleTool(_logTool, I18nManager.Instance.GetResource(DemoKeys.ToolLog));
                break;
        }
    }

    private void ToggleTool(Tool? tool, string title)
    {
        if (tool is null || _dockFactory is null || _layout is null)
        {
            return;
        }

        var hidden = _layout.HiddenDockables?.Contains(tool) == true;
        if (hidden)
        {
            _dockFactory.RestoreDockable(tool.Id);
            Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.ToggleTool005") + title);
        }
        else
        {
            _dockFactory.HideDockable(tool);
            Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.ToggleTool006") + title);
        }
    }

    /// <summary>重置工作区：整体重建 Dock 布局并回到欢迎文档。</summary>
    public void ResetLayout()
    {
        _ = ShutdownAsync();
        Documents.Clear();
        ActiveDocument = null;
        _selectedNavItem = null;
        BuildLayout();
        OpenItem(Groups[0].Items[0]);
        Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.ResetLayout007"));
    }

    /// <summary>
    /// 构建 Dock 工作区：左导航树 / 右属性面板 / 下日志为 ToolDock，中央为 DocumentDock（演示文档页签）。
    /// </summary>
    private void BuildLayout()
    {
        _dockFactory = new DemoDockFactory(this);

        _navTool = new Tool
        {
            Id = "Tool.Nav",
            Title = T(DemoKeys.ToolNavigation),
            Context = new Views.Tools.NavToolView { DataContext = this },
        };
        _propTool = new Tool
        {
            Id = "Tool.Props",
            Title = T(DemoKeys.ToolProperties),
            Context = new Views.Tools.PropertyToolView { DataContext = this },
        };
        _logTool = new Tool
        {
            Id = "Tool.Log",
            Title = T(DemoKeys.ToolLog),
            Context = new Views.Tools.LogToolView { DataContext = this },
        };
        var navDock = new ToolDock
        {
            Id = "Dock.Nav",
            ActiveDockable = _navTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_navTool),
            Dock = DockMode.Left,
            Proportion = 0.16,
        };
        var propDock = new ToolDock
        {
            Id = "Dock.Props",
            ActiveDockable = _propTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_propTool),
            Dock = DockMode.Right,
            Proportion = 0.17,
            MinWidth = 220,
        };
        var logDock = new ToolDock
        {
            Id = "Dock.Log",
            ActiveDockable = _logTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_logTool),
            Dock = DockMode.Bottom,
            Proportion = 0.14,
        };

        _docDock = new DocumentDock
        {
            Id = "Dock.Docs",
            VisibleDockables = _dockFactory.CreateList<IDockable>(),
            CanCreateDocument = false,
            Proportion = 0.67,
            IsCollapsable = false,
        };
        _docDock.PropertyChanged += OnDocDockPropertyChanged;

        var mainHorizontal = new ProportionalDock
        {
            Id = "Dock.Main",
            Orientation = global::Dock.Model.Core.Orientation.Horizontal,
            Proportion = 0.86,
            VisibleDockables = _dockFactory.CreateList<IDockable>(
                navDock,
                new ProportionalDockSplitter(),
                _docDock,
                new ProportionalDockSplitter(),
                propDock),
            ActiveDockable = _docDock,
        };
        var workspace = new ProportionalDock
        {
            Id = "Dock.Workspace",
            Orientation = global::Dock.Model.Core.Orientation.Vertical,
            VisibleDockables = _dockFactory.CreateList<IDockable>(
                mainHorizontal,
                new ProportionalDockSplitter(),
                logDock),
            ActiveDockable = mainHorizontal,
        };

        _layout = new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = _dockFactory.CreateList<IDockable>(workspace),
            ActiveDockable = workspace,
            IsCollapsable = false,
        };
        _dockFactory.InitLayout(_layout);
        this.RaisePropertyChanged(nameof(Layout));
    }

    private void OnDocDockPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDock.ActiveDockable) && _docDock is not null)
        {
            ActiveDocument = Documents.FirstOrDefault(document => ReferenceEquals(document.DockDocument, _docDock.ActiveDockable));
        }
    }

    private void SyncToolTitles()
    {
        if (_navTool is not null) _navTool.Title = T(DemoKeys.ToolNavigation);
        if (_propTool is not null) _propTool.Title = T(DemoKeys.ToolProperties);
        if (_logTool is not null) _logTool.Title = T(DemoKeys.ToolLog);
    }

    private string T(string key) => I18nManager.Instance.GetResource(key);

    /// <summary>接管 Dock 侧文档关闭（页签 ✕），与 Documents/ActiveDocument 双向同步。</summary>
    private sealed class DemoDockFactory(MainViewModel owner) : Factory
    {
        public override void CloseDockable(IDockable dockable)
        {
            base.CloseDockable(dockable);
            if (dockable is Document { Context: Views.DemoDocumentView { DataContext: DemoDocumentViewModel document } })
            {
                owner.SyncDocumentClosed(document);
            }
        }
    }

    public void WriteSampleLogs()
    {
        Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.WriteSampleLogs008"));
        Logger.Warn(L.Instance.GetResource("Demo.Text.MainViewModel.WriteSampleLogs009"));
        Logger.Error(L.Instance.GetResource("Demo.Text.MainViewModel.WriteSampleLogs010"), new InvalidOperationException(L.Instance.GetResource("Demo.Text.MainViewModel.WriteSampleLogs011")));
    }

    /// <summary>冒烟：写三类日志并跑一遍核心演示，返回是否全部成功（CI / AOT 冒烟用）。</summary>
    public async Task<bool> RunSmokeAsync(CancellationToken cancellation)
    {
        DemoBootstrapper.Initialize();
        Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.RunSmokeAsync012"));

        var ok = true;
        WriteSampleLogs();
        var overviewOk = await SmokeOverviewAsync(cancellation);
        ok &= await SmokeWalkCatalogAsync(cancellation);
        ok &= await SmokeShellFeaturesAsync(cancellation);

        // 核心演示（同步、无网络）：扩展方法 / 事件总线 / 序列化
        try
        {
            var busy = L.Instance.GetResource("Demo.Text.MainViewModel.RunSmokeAsync013").Length > 8;
            var bus = new global::CodeWF.Toolkit.EventBus.EventBus();
            var received = false;
            bus.Subscribe<Views.Pages.GreetCommand>(_ => received = true);
            bus.Publish(new Views.Pages.GreetCommand("smoke"));

            var query = new Views.Pages.TimeQuery();
            bus.Subscribe(new Views.Pages.TimeQueryHandler());
            bus.Query(query);

            var packet = new Views.Pages.SamplePacket { Id = 9527, Score = 99.5f, Title = L.Instance.GetResource("Demo.Text.MainViewModel.RunSmokeAsync014") };
            var buffer = global::CodeWF.Toolkit.NetWeaver.SerializeHelper.Serialize(packet, 0);
            var restored = global::CodeWF.Toolkit.NetWeaver.SerializeHelper.Deserialize<Views.Pages.SamplePacket>(buffer);

            ok = ok && busy && received && query.Result != default
                 && restored.Id == packet.Id
                 && Math.Abs(restored.Score - packet.Score) < 0.001f
                 && restored.Title == packet.Title;

            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.MainViewModel.RunSmokeAsync015"), received, query.Result, ok));
        }
        catch (Exception ex)
        {
            ok = false;
            Logger.Error(L.Instance.GetResource("Demo.Text.MainViewModel.RunSmokeAsync016"), ex);
        }

        await Task.Delay(300, cancellation);
        Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModel.RunSmokeAsync017"));
        await Logger.ShutdownAsync();
        return ok && overviewOk;
    }

    private void ApplyAppearance()
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = SelectedPalette.ResolveVariant(IsDarkMode);
        SyncGlobalItems();
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.MainViewModel.ApplyAppearance020"), SelectedPalette.Title, (IsDarkMode ? L.Instance.GetResource("Demo.Text.MainViewModel.ApplyAppearance018") : L.Instance.GetResource("Demo.Text.MainViewModel.ApplyAppearance019"))));
    }

    private IReadOnlyList<PropertyItem> BuildGlobalProperties()
    {
        GlobalValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(OnGlobalPropertyCommitted);
        GlobalBrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(_ => { });

        _densityItem = new PropertyItem
        {
            Id = "shell.density",
            Name = T(DemoKeys.ShellDensity),
            Kind = PropertyItemKind.Choice,
            Options = [T(DemoKeys.ShellDensityComfort), T(DemoKeys.ShellDensityCompact)],
            Value = DensityLabel,
            DefaultValue = T(DemoKeys.ShellDensityComfort),
            Description = T("Demo.Studio.DensityDescription"),
        };
        _paletteItem = new PropertyItem
        {
            Id = "shell.palette",
            Name = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties021"),
            Kind = PropertyItemKind.Choice,
            Options = Palettes.Select(p => p.Title).ToArray(),
            Value = Palettes[0].Title,
            DefaultValue = Palettes[0].Title,
            SummaryProvider = () => I18nManager.Instance.GetResource(SelectedPalette.TitleKey),
            Description = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties022"),
        };
        _modeItem = new PropertyItem
        {
            Id = "shell.mode",
            Name = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties023"),
            Kind = PropertyItemKind.Radio,
            Options = [L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties024"), L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties025")],
            Value = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties026"),
            DefaultValue = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties027"),
            Description = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties028"),
        };
        _langItem = new PropertyItem
        {
            Id = "shell.lang",
            Name = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties029"),
            Kind = PropertyItemKind.Choice,
            Options = Cultures.Select(c => c.DisplayName).ToArray(),
            Value = Cultures[0].DisplayName,
            DefaultValue = Cultures[0].DisplayName,
            SummaryProvider = () => SelectedCulture.DisplayName,
            Description = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties030"),
        };

        var appearance = new PropertyItem
        {
            Id = "shell.appearance",
            Name = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties031"),
            Kind = PropertyItemKind.Category,
            Description = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties032"),
            Children = [_densityItem, _paletteItem, _modeItem],
        };
        var language = new PropertyItem
        {
            Id = "shell.language",
            Name = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties033"),
            Kind = PropertyItemKind.Category,
            Description = L.Instance.GetResource("Demo.Text.MainViewModel.BuildGlobalProperties034"),
            Children = [_langItem],
        };

        foreach (var root in new[] { appearance, language })
        {
            foreach (var node in root.SelfAndDescendants())
            {
                node.RefreshSummary();
            }

            root.RecalculateModifiedDeep();
        }

        return [appearance, language];
    }

    private void OnGlobalPropertyCommitted(PropertyValueChange change)
    {
        var value = change.NewValue?.ToString();
        switch (change.Item.Id)
        {
            case "shell.density" when value is not null:
                Density = value == T(DemoKeys.ShellDensityCompact) ? DemoDensity.Compact : DemoDensity.Comfort;
                break;
            case "shell.palette" when value is not null:
                SelectedPalette = Palettes.FirstOrDefault(p => p.Title == value) ?? SelectedPalette;
                break;
            case "shell.mode" when value is not null:
                IsDarkMode = value == L.Instance.GetResource("Demo.Text.MainViewModel.OnGlobalPropertyCommitted035");
                break;
            case "shell.lang" when value is not null:
                var culture = Cultures.FirstOrDefault(c => c.DisplayName == value);
                if (culture is not null)
                {
                    SelectedCulture = culture;
                }

                break;
        }
    }

    private void SyncGlobalItems()
    {
        void Sync(PropertyItem? item, object? value)
        {
            if (item is null || Equals(item.Value, value))
            {
                return;
            }

            item.Value = value;
            item.RefreshSummary();
        }

        Sync(_densityItem, DensityLabel);
        Sync(_paletteItem, SelectedPalette.Title);
        Sync(_modeItem, IsDarkMode ? L.Instance.GetResource("Demo.Text.MainViewModel.SyncGlobalItems036") : L.Instance.GetResource("Demo.Text.MainViewModel.SyncGlobalItems037"));
        Sync(_langItem, SelectedCulture.DisplayName);
    }
}
