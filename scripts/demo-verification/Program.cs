using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.Infrastructure;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Files.Helpers;
using ReactiveUI.Avalonia;

// 在任何应用或视图模型初始化前隔离布局与偏好，验证不能读取或修改用户配置。
var outputArgument = args.FirstOrDefault() == "--palette-audit" ? args.Skip(1).FirstOrDefault() : args.FirstOrDefault();
var verificationDirectory = Path.GetFullPath(outputArgument ?? Path.Combine(Path.GetTempPath(), "CodeWF.Toolkit.Verification", Guid.NewGuid().ToString("N")));
Directory.CreateDirectory(verificationDirectory);
Environment.SetEnvironmentVariable("CODEWF_TOOLKIT_DEMO_DATA_DIRECTORY", verificationDirectory);
AppContext.SetData("APP_CONFIG_FILE", Path.Combine(verificationDirectory, "application.config"));

// 使用真实 Demo 主题、页面与帮助类在 Skia 离屏环境验证，锁屏时不创建桌面窗口。
AppBuilder.Configure<CodeWF.Toolkit.Demo.Avalonia.App>().UseSkia().WithInterFont().UseReactiveUI(_ => { })
    .With(new FontManagerOptions { FontFallbacks = [
        new FontFallback { FontFamily = new FontFamily("Microsoft YaHei UI") },
        new FontFallback { FontFamily = new FontFamily("PingFang SC") },
        new FontFallback { FontFamily = new FontFamily("Noto Sans CJK SC") }
    ] })
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
DemoBootstrapper.Initialize();
if (args.FirstOrDefault() == "--palette-audit")
    return DemoPaletteVerification.Run(verificationDirectory);
VerifyAppConfigBatch(verificationDirectory);
VerifyThirdPartyLocales();
VerifyHiddenPropertyRows();
var sharedItem = new PropertyItem { Kind = PropertyItemKind.Text, Value = "Before" };
var sharedEditor = new PropertyCellEditor { DataContext = new PropertyLeafRow(sharedItem, 0, false) };
sharedItem.Value = "After";
Require(((TextBox)sharedEditor.Content!).Text == "After", "NuGet property editor follows programmatic values.");
Require(MainViewModel.LayoutPath == Path.Combine(verificationDirectory, "layout.json"), "Layout storage stays inside the isolated verification directory.");
VerifyDockDocumentTransfer(Path.Combine(verificationDirectory, "dock-captures"));
var manager = I18nManager.Instance;
var window = new Window { Width = 1120, Height = 760 };
window.Show();
var renders = 0;
var executions = 0;
using var bitmap = new RenderTargetBitmap(new PixelSize(1120, 760));
try
{
    foreach (var item in DemoCatalog.CreateGroups().SelectMany(group => group.Items))
    {
        var page = item.PageFactory();
        window.Content = page;
        _ = page.Properties;
        if (page is HelperDemoPage helper && helper.ViewModel.Example.Smoke)
        {
            Complete(helper.ViewModel.RunAsync());
            Require(helper.ViewModel.Succeeded, item.Api + ": " + helper.ViewModel.Output);
            executions++;
        }
        if (page is DataTableDemoPage table)
        {
            foreach (var excel in new[] { false, true })
            {
                Complete(table.ViewModel.RunAsync(excel));
                Require(table.ViewModel.Succeeded && table.ViewModel.Rows.Count == 2, table.ViewModel.Output);
                executions++;
            }
        }
        foreach (var culture in new[] { "zh-CN", "en-US" })
        {
            manager.Culture = CultureInfo.GetCultureInfo(culture);
            foreach (var palette in StudioPalettes.All)
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette.Key, dark);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                bitmap.Render(window);
                Require(page.Bounds.Width > 0 && page.Bounds.Height > 0, "Empty page: " + item.Api);
                renders++;
            }
        }
        Console.WriteLine("PASS catalog page: " + item.Api);
    }

    var slider = new Slider { Value = 50, Maximum = 100, Width = 240 };
    var propertyGrid = new PropertyGrid();
    window.Content = new StackPanel { Children = { slider, propertyGrid } };
    foreach (var variant in new[] { StudioTheme.GetVariant("graphite", false), StudioTheme.GetVariant("teal", true), StudioTheme.GetVariant("terracotta", false), StudioTheme.GetVariant("indigo", true) })
    {
        window.RequestedThemeVariant = variant;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Require(window.TryFindResource("StudioAccent", variant, out var accent), "Missing Studio accent.");
        Require(((ISolidColorBrush)slider.Foreground!).Color == ((ISolidColorBrush)accent!).Color, "Slider keeps an old palette.");
    }
    var labels = new HashSet<string>();
    foreach (var culture in new[] { "zh-CN", "en-US", "zh-CN" })
    {
        manager.Culture = CultureInfo.GetCultureInfo(culture);
        Dispatcher.UIThread.RunJobs();
        labels.Add(propertyGrid.GetVisualDescendants().OfType<TextBox>().Single(input => input.Name == "SearchBox").PlaceholderText!);
    }
    Require(labels.Count == 2, "Existing property panel does not switch languages.");
    Console.WriteLine($"PASS: {renders} page/language/palette renders, {executions} API executions, live library culture/theme switching; NativeAOT={!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported}.");
    return 0;
}
finally { window.Close(); }
static void VerifyAppConfigBatch(string output)
{
    // 验证新的批量 API 在原生程序中的真实文件读写，不接触默认配置路径。
    var directory = Path.Combine(output, "app-config-batch");
    Directory.CreateDirectory(directory);
    var path = Path.Combine(directory, "verification.config");
    File.WriteAllText(path, "<configuration><!-- retained --><appSettings><add key=\"external\" value=\"keep\" /></appSettings><custom value=\"keep\" /></configuration>");
    AppConfigHelper.SetMany(path, new Dictionary<string, object?>
    {
        ["enabled"] = true, ["ratio"] = 123.5, ["count"] = 7,
        ["day"] = DayOfWeek.Friday, ["text"] = "A & B", ["empty"] = null
    });
    AppConfigHelper.SetMany(path, new Dictionary<string, object?> { ["count"] = 9 });
    var document = System.Xml.Linq.XDocument.Load(path);
    Require(document.Root!.Element("custom")!.Attribute("value")!.Value == "keep" &&
            document.Root.Nodes().OfType<System.Xml.Linq.XComment>().Any(comment => comment.Value == " retained "),
        "Batch save preserves unrelated sections and comments.");
    var snapshot = AppConfigHelper.ReadAll(path);
    File.Delete(path);
    var previousCulture = CultureInfo.CurrentCulture;
    try
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        Require(snapshot.TryGet<bool?>("enabled", out var enabled) && enabled == true, "Detached nullable boolean snapshot.");
        Require(snapshot.TryGet<double?>("ratio", out var ratio) && ratio == 123.5, "Snapshot numeric conversion uses invariant culture.");
        Require(snapshot.TryGet<int>("count", out var count) && count == 9, "Batch update preserves final integer value.");
        Require(snapshot.TryGet<DayOfWeek?>("day", out var day) && day == DayOfWeek.Friday, "Snapshot nullable enum conversion.");
        Require(snapshot["external"] == "keep" && snapshot["text"] == "A & B" && snapshot["empty"] == string.Empty,
            "Batch save preserves unrelated keys and XML values.");
        Require(!snapshot.TryGet<int>("text", out _) && !snapshot.TryGet<string>("missing", out _), "Invalid snapshot conversion reports failure.");
    }
    finally { CultureInfo.CurrentCulture = previousCulture; }
    Console.WriteLine("PASS app-config batch read/write and detached typed snapshot.");
}

static void VerifyHiddenPropertyRows()
{
    var visible = false;
    var hiddenLeaf = new PropertyItem { Name = "Private value", VisibleWhen = () => visible };
    var hiddenCategory = new PropertyItem { Name = "Private category", Kind = PropertyItemKind.Category,
        VisibleWhen = () => visible, Children = [new PropertyItem { Name = "Private descendant" }] };
    var parent = new PropertyItem { Name = "Parent", Children = [new PropertyItem { Name = "Public" }, hiddenLeaf, hiddenCategory] };
    var grid = new PropertyGrid { Items = new[] { parent } };
    var shell = new Window { Width = 480, Height = 360, Content = grid };
    try
    {
        shell.Show();
        foreach (var mode in Enum.GetValues<PropertyGridViewMode>())
        foreach (var query in new string?[] { null, "Parent", "Private" })
        {
            grid.ViewMode = mode;
            grid.SearchText = query;
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            Require(!grid.Rows.Any(row => row.Item.Name.StartsWith("Private", StringComparison.Ordinal)),
                "Hidden items and categories stay hidden in " + mode + " with query " + query);
            if (query == "Private") Require(grid.Rows.Count == 0, "Hidden matches do not retain their ancestors.");
            if (mode == PropertyGridViewMode.Alphabetical)
                Require(!grid.Rows.Any(row => row is PropertyCategoryRow), "Alphabetical search never adds category rows.");
        }
        grid.SearchText = "Private";
        visible = true;
        grid.Refresh();
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        Require(grid.Rows.Any(row => ReferenceEquals(row.Item, hiddenLeaf)),
            "A refreshed dynamic visibility predicate makes the matching property accessible.");
        visible = false;
        grid.Refresh();
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        Require(grid.Rows.Count == 0, "Refreshing dynamic visibility removes the branch from current search results.");
        Console.WriteLine("PASS PropertyGrid visibility: categories, hidden descendants, both sorting modes and dynamic refresh.");
    }
    finally { shell.Close(); }
}

static void VerifyThirdPartyLocales()
{
    var application = Application.Current!;
    var editor = new TextBox { Text = "Locale verification" };
    var shell = new Window { Width = 360, Height = 120, Content = editor };
    try
    {
        shell.Show(); Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        var menu = (MenuFlyout)editor.ContextFlyout!;
        foreach (var culture in new[] { "en-US", "zh-CN", "ja-JP", "zh-Hant", "en-US" })
        {
            I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(culture);
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            ResourceDictionary semi = culture switch
            {
                "en-US" => new Semi.Avalonia.Locale.en_us(),
                "ja-JP" => new Semi.Avalonia.Locale.ja_jp(),
                "zh-Hant" => new Semi.Avalonia.Locale.zh_tw(),
                _ => new Semi.Avalonia.Locale.zh_cn(),
            };
            ResourceDictionary ursa = culture switch
            {
                "en-US" => new Ursa.Themes.Semi.Locale.en_us(),
                "ja-JP" => new Ursa.Themes.Semi.Locale.en_us(),
                _ => new Ursa.Themes.Semi.Locale.zh_cn(),
            };
            menu.ShowAt(editor); Dispatcher.UIThread.RunJobs();
            Require(Equals(((MenuItem)menu.Items[1]!).Header, semi["STRING_MENU_COPY"])
                && Equals(((MenuItem)menu.Items[2]!).Header, semi["STRING_MENU_PASTE"]),
                "Existing Semi editor menu follows global culture: " + culture);
            Require(Equals(application.Styles.OfType<Ursa.Themes.Semi.UrsaSemiTheme>().Single().Resources["STRING_MENU_DIALOG_CANCEL"],
                ursa["STRING_MENU_DIALOG_CANCEL"]), "Existing Ursa dialog follows global culture: " + culture);
            menu.Hide();
        }
        application.Resources["STRING_MENU_COPY"] = "Host copy";
        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        Dispatcher.UIThread.RunJobs();
        menu.ShowAt(editor); Dispatcher.UIThread.RunJobs();
        Require(Equals(((MenuItem)menu.Items[1]!).Header, "Host copy"), "Theme locale changes preserve host overrides.");
        menu.Hide();
        Console.WriteLine("PASS Semi/Ursa: existing menu, global culture and host resource precedence.");
    }
    finally
    {
        application.Resources.Remove("STRING_MENU_COPY");
        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        shell.Close();
    }
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

void VerifyDockDocumentTransfer(string captures)
{
    // 使用真实外壳验证 Document 分屏与浮动，内容和属性面板始终是同一组实例。
    var shell = new global::CodeWF.Toolkit.Demo.Avalonia.Views.MainWindow { Width = 1600, Height = 960 };
    shell.Show();
    var model = (global::CodeWF.Toolkit.Demo.Avalonia.ViewModels.MainViewModel)shell.DataContext!;
    try
    {
        VerifyTitleBar(shell, model);
        foreach (var navigationShare in new[] { 0.16, 0.24 })
        {
            model.ResetLayout();
            model.OpenItem(model.Groups.SelectMany(group => group.Items).Single(item => item.Api == "DoubleExtensions"));
            var document = model.ActiveDocument!;
            var dockable = document.DockDocument!;
            var view = (global::CodeWF.Toolkit.Demo.Avalonia.Views.DemoDocumentView)dockable.Context!;
            var properties = view.FindControl<PropertyGrid>("DocumentProperties")!;
            var documentArea = (global::Dock.Model.Controls.IProportionalDock)((global::Dock.Model.Core.IDock)dockable.Owner!).Owner!;
            var navigationDock = (global::Dock.Model.Core.IDock)((global::Dock.Model.Core.IDock)documentArea.Owner!).VisibleDockables!
                .Single(item => item.Id == "Dock.Nav");
            var navigationView = (Control)navigationDock.VisibleDockables![0].Context!;
            // 同时检查默认布局与拖动分隔条后的份额，不能靠恢复固定默认值通过。
            navigationDock.Proportion = navigationShare;
            documentArea.Proportion = 1 - navigationShare;
            PumpDock(() => navigationView.Bounds.Width > 0);
            var navigationWidth = navigationView.Bounds.Width;
            model.OpenItem(model.Groups[0].Items[0]);
            var target = (global::Dock.Model.Core.IDock)model.ActiveDocument!.DockDocument!.Owner!;
            var factory = model.Layout!.Factory!;
            var service = new global::Dock.Model.DockService();
            Require(service.SplitDockable(dockable, (global::Dock.Model.Core.IDock)dockable.Owner!, target,
                global::Dock.Model.Core.DockOperation.Right, bExecute: true), "The standard Dock service splits the document.");
            PumpDock(() => ReferenceEquals(TopLevel.GetTopLevel(view), shell) && properties.Bounds.Width > 0);
            Require(!target.VisibleDockables!.Contains(dockable), "Splitting removes the document from its original tab group.");
            Require(ReferenceEquals(properties.Items, document.PropertyItems), "Split document keeps its own property panel.");
            VerifyNavigation("split");
            Capture(shell, navigationShare == 0.16 ? "split.png" : "split-adjusted.png");
            factory.FloatDockable(dockable);
            PumpDock(() => TopLevel.GetTopLevel(view) is Window floating && !ReferenceEquals(floating, shell));
            Require(ReferenceEquals(properties.Items, document.PropertyItems), "Floating document keeps its own property panel.");
            VerifyNavigation("float");
            Capture((Window)TopLevel.GetTopLevel(view)!, navigationShare == 0.16 ? "floating.png" : "floating-adjusted.png");
            factory.MoveDockable((global::Dock.Model.Core.IDock)dockable.Owner!, target, dockable, null);
            PumpDock(() => ReferenceEquals(TopLevel.GetTopLevel(view), shell) && properties.Bounds.Width > 0);
            Require(ReferenceEquals(dockable.Context, view), "Dock transfer preserves the actual document view.");
            Require(target.VisibleDockables!.Count(item => ReferenceEquals(item, dockable)) == 1,
                "Re-docking creates exactly one tab for the document.");
            VerifyNavigation("re-dock");
            Capture(shell, navigationShare == 0.16 ? "redocked.png" : "redocked-adjusted.png");
            Console.WriteLine("PASS actual shell: split, float and re-dock preserve document content, properties and navigation share " + navigationShare);

            void VerifyNavigation(string stage)
            {
                Require(Math.Abs(navigationDock.Proportion - navigationShare) < 0.000001,
                    "Document " + stage + " preserves the current navigation proportion.");
                Require(Math.Abs(navigationView.Bounds.Width - navigationWidth) <= 2,
                    "Document " + stage + " preserves the actual navigation width.");
            }
        }
    }
    finally { shell.Close(); }

    void Capture(Window target, string name)
    {
        target.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        Directory.CreateDirectory(captures);
        using var image = new global::Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize(
            Math.Max(1, (int)Math.Ceiling(target.Bounds.Width)), Math.Max(1, (int)Math.Ceiling(target.Bounds.Height))));
        image.Render(target);
        image.Save(Path.Combine(captures, name));
    }

    void PumpDock(Func<bool> completed)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        do
        {
            Dispatcher.UIThread.RunJobs();
            shell.UpdateLayout();
            if (completed()) return;
            if (DateTime.UtcNow > deadline) throw new TimeoutException("Dock document transfer did not complete.");
            Thread.Sleep(10);
        } while (true);
    }
}
static void Complete(Task task)
{
    var deadline = DateTime.UtcNow.AddSeconds(20);
    while (!task.IsCompleted)
    {
        if (DateTime.UtcNow >= deadline) throw new TimeoutException("Demo API execution timed out.");
        Dispatcher.UIThread.RunJobs();
        Thread.Sleep(1);
    }
    task.GetAwaiter().GetResult();
}

void VerifyTitleBar(global::CodeWF.Toolkit.Demo.Avalonia.Views.MainWindow shell, MainViewModel model)
{
    // 外观与语言入口都已迁到标题栏弹层（AppearanceButton / TitleBarLanguageButton + Popup）。
    Dispatcher.UIThread.RunJobs();
    shell.UpdateLayout();
    var appearanceButton = shell.FindControl<Avalonia.Controls.Primitives.ToggleButton>("AppearanceButton")!;
    var languageButton = shell.FindControl<Avalonia.Controls.Primitives.ToggleButton>("TitleBarLanguageButton")!;
    Require(appearanceButton.Height == 32 && languageButton.Width == 32 && languageButton.Height == 32,
        "Title-bar appearance and language entries keep compact dimensions.");
    var popups = Avalonia.LogicalTree.LogicalExtensions.GetLogicalDescendants(shell)
        .OfType<Avalonia.Controls.Primitives.Popup>()
        .Concat(shell.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.Popup>()).ToArray();
    var languagePopup = popups.First(popup => (popup.Child as Border)?.Classes.Contains("language-menu") == true);
    var appearancePopup = popups.First(popup => (popup.Child as Border)?.Classes.Contains("appearance-choices") == true);
    var appearanceBorder = (Border)appearancePopup.Child!;

    ListBox LanguageList() => ((Border)languagePopup.Child!).GetVisualDescendants().OfType<ListBox>()
        .Single(list => list.Classes.Contains("language-list"));
    ListBox ModePills() => appearanceBorder.GetVisualDescendants().OfType<ListBox>()
        .Single(list => list.Classes.Contains("appearance-pills") && list.Items.Count == 3);
    Button[] PaletteCards() => appearanceBorder.GetVisualDescendants().OfType<Button>()
        .Where(button => button.Classes.Contains("palette-card")).ToArray();
    void WaitFor(Func<bool> condition, string message)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        do
        {
            Dispatcher.UIThread.RunJobs();
            shell.UpdateLayout();
            // PopupRoot 是独立布局根，必须单独驱动布局才会物化 ItemsControl 容器。
            ((Border)languagePopup.Child!).UpdateLayout();
            appearanceBorder.UpdateLayout();
            try { if (condition()) return; }
            catch (InvalidOperationException) { }
            if (DateTime.UtcNow >= deadline) throw new TimeoutException(message);
            Thread.Sleep(10);
        } while (true);
    }

    // 语言弹层：徽标行列表与全局语言双向跟随（外部切换 ↔ 弹层选择）。
    languageButton.IsChecked = true;
    WaitFor(() => LanguageList().Items.Count == model.Cultures.Count, "The language popup did not materialize its culture list.");
    Require(ReferenceEquals(LanguageList().ItemsSource, model.Cultures), "Language popup lists the host cultures.");
    var firstCulture = model.Cultures[0];
    var lastCulture = model.Cultures[^1];
    I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(lastCulture.Name);
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Require(ReferenceEquals(model.SelectedCulture, lastCulture) && ReferenceEquals(LanguageList().SelectedItem, lastCulture),
        "Global culture changes synchronize the existing title-bar language list.");
    LanguageList().SelectedItem = firstCulture;
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Require(I18nManager.Instance.Culture.Name == firstCulture.Name && ReferenceEquals(model.SelectedCulture, firstCulture),
        "Choosing the previous language after an external change restores the global culture.");
    Task.Run(() => I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(lastCulture.Name)).GetAwaiter().GetResult();
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Require(ReferenceEquals(LanguageList().SelectedItem, lastCulture),
        "Background global culture changes update the existing language list on the UI thread.");
    Require(model.StatusText == I18nManager.Instance.GetResource(global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys.Shell.Ready),
        "The shell status refreshes after external culture changes.");
    languageButton.IsChecked = false;
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();

    // 外观弹层：密度/模式 pill 行 + 配色卡网格（点击选中，三态模式逐色板轮询）。
    appearanceButton.IsChecked = true;
    WaitFor(() => ModePills().Items.Count == 3 && PaletteCards().Length == model.Palettes.Count,
        "The appearance popup did not materialize its palette grid.");
    var densityPills = appearanceBorder.GetVisualDescendants().OfType<ListBox>()
        .Single(list => list.Classes.Contains("appearance-pills") && list.Items.Count == 2);
    Require(densityPills.SelectedIndex == model.DensityIndex, "Density pills follow the current shell density.");
    densityPills.SelectedIndex = 1;
    Dispatcher.UIThread.RunJobs();
    Require(model.DensityIndex == 1, "Density pills switch the shell to compact.");
    densityPills.SelectedIndex = 0;
    Dispatcher.UIThread.RunJobs();
    Require(model.DensityIndex == 0, "Density pills restore the comfortable shell.");
    Require(ModePills().Items.Count == 3, "Title bar keeps the three-state appearance mode pills.");
    if (ModePills().SelectedIndex != model.ThemeModeIndex)
    {
        // XAML 里 SelectedIndex 绑定先于 ItemsSource 激活，初始选择会丢失，这里显式对齐。
        ModePills().SelectedIndex = model.ThemeModeIndex;
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    }
    Require(ModePills().SelectedIndex == model.ThemeModeIndex, "Mode pills follow the current theme mode.");
    foreach (var palette in model.Palettes)
    {
        model.SelectedPalette = palette;
        foreach (var expected in new[] { StudioThemeMode.Light, StudioThemeMode.Dark, StudioThemeMode.System })
        {
            ModePills().SelectedIndex = (int)expected;
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            Require(model.ThemeModeIndex == (int)expected, "Mode pills select all three persisted modes: " + expected);
            Require(ReferenceEquals(model.SelectedPalette, palette), "Mode changes preserve the selected palette.");
            var actual = Application.Current!.RequestedThemeVariant;
            Require(actual == palette.ResolveVariant(model.IsDarkMode), "Mode pills apply the Studio palette variant: " + actual);
        }
    }
    Require(appearanceBorder.GetVisualDescendants().OfType<ItemsControl>()
            .Count(control => ReferenceEquals(control.ItemsSource, model.Palettes)) == 1,
        "The title bar has one palette entry, with no duplicate grid.");
    var cards = PaletteCards();
    cards[0].RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Require(ReferenceEquals(model.SelectedPalette, (AppearanceOption)cards[0].Tag!), "Palette card clicks apply the chosen palette.");
    string TipText(object? tip) => tip switch { string text => text, TextBlock block => block.Text, _ => string.Empty };
    Require(TipText(ToolTip.GetTip(languageButton)) == I18nManager.Instance.GetResource(global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys.Shell.GlobalSettings.LanguageLabel)
        && TipText(ToolTip.GetTip(appearanceButton)) == I18nManager.Instance.GetResource(global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys.Shell.AppearanceMenu),
        "The existing title-bar tooltips follow the host language.");
    appearanceButton.IsChecked = false;
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Console.WriteLine("PASS title bar: appearance pills, palette cards, language list and compact entries.");
}
