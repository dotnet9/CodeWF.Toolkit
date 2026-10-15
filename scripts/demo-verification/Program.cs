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
using ReactiveUI.Avalonia;

// 使用真实 Demo 主题、页面与帮助类在 Skia 离屏环境验证，锁屏时不创建桌面窗口。
AppBuilder.Configure<CodeWF.Toolkit.Demo.Avalonia.App>().UseSkia().WithInterFont().UseReactiveUI(_ => { })
    .With(new FontManagerOptions { FontFallbacks = [
        new FontFallback { FontFamily = new FontFamily("Microsoft YaHei UI") },
        new FontFallback { FontFamily = new FontFamily("PingFang SC") },
        new FontFallback { FontFamily = new FontFamily("Noto Sans CJK SC") }
    ] })
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
DemoBootstrapper.Initialize();
VerifyDockDocumentTransfer(Path.Combine(Path.GetFullPath(args.FirstOrDefault() ?? Path.GetTempPath()), "dock-captures"));
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
            foreach (var palette in Enum.GetValues<StudioPalette>())
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
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
    foreach (var variant in new[] { StudioTheme.DotNetLight, StudioTheme.AuroraDark, StudioTheme.EmberLight, StudioTheme.DotNetDark })
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

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void VerifyDockDocumentTransfer(string captures)
{
    // 使用真实外壳验证 Document 分屏与浮动，内容和属性面板始终是同一组实例。
    var shell = new global::CodeWF.Toolkit.Demo.Avalonia.Views.MainWindow { Width = 1600, Height = 960 };
    shell.Show();
    var model = (global::CodeWF.Toolkit.Demo.Avalonia.ViewModels.MainViewModel)shell.DataContext!;
    try
    {
        VerifyTitleBar(shell, model);
        model.ResetLayout();
        model.OpenItem(model.Groups.SelectMany(group => group.Items).Single(item => item.Api == "DoubleExtensions"));
        var document = model.ActiveDocument!;
        var dockable = document.DockDocument!;
        var view = (global::CodeWF.Toolkit.Demo.Avalonia.Views.DemoDocumentView)dockable.Context!;
        var properties = view.FindControl<PropertyGrid>("DocumentProperties")!;
        model.OpenItem(model.Groups[0].Items[0]);
        var target = (global::Dock.Model.Core.IDock)model.ActiveDocument!.DockDocument!.Owner!;
        var factory = model.Layout!.Factory!;
        var service = new global::Dock.Model.DockService();
        Require(service.SplitDockable(dockable, (global::Dock.Model.Core.IDock)dockable.Owner!, target,
            global::Dock.Model.Core.DockOperation.Right, bExecute: true), "The standard Dock service splits the document.");
        PumpDock(() => ReferenceEquals(TopLevel.GetTopLevel(view), shell) && properties.Bounds.Width > 0);
        Require(!target.VisibleDockables!.Contains(dockable), "Splitting removes the document from its original tab group.");
        Require(ReferenceEquals(properties.Items, document.PropertyItems), "Split document keeps its own property panel.");
        Capture(shell, "split.png");
        factory.FloatDockable(dockable);
        PumpDock(() => TopLevel.GetTopLevel(view) is Window floating && !ReferenceEquals(floating, shell));
        Require(ReferenceEquals(properties.Items, document.PropertyItems), "Floating document keeps its own property panel.");
        Capture((Window)TopLevel.GetTopLevel(view)!, "floating.png");
        factory.MoveDockable((global::Dock.Model.Core.IDock)dockable.Owner!, target, dockable, null);
        PumpDock(() => ReferenceEquals(TopLevel.GetTopLevel(view), shell) && properties.Bounds.Width > 0);
        Require(ReferenceEquals(dockable.Context, view), "Dock transfer preserves the actual document view.");
        Require(target.VisibleDockables!.Count(item => ReferenceEquals(item, dockable)) == 1,
            "Re-docking creates exactly one tab for the document.");
        Capture(shell, "redocked.png");
        Console.WriteLine("PASS actual shell: split, float and re-dock preserve document content and properties.");
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

static void VerifyTitleBar(global::CodeWF.Toolkit.Demo.Avalonia.Views.MainWindow shell, MainViewModel model)
{
    var toggle = shell.FindControl<Ursa.Controls.ThemeToggleButton>("TitleBarThemeToggle")!;
    var language = shell.FindControl<ComboBox>("TitleBarLanguageSelector")!;
    Require(toggle.IsThreeState, "Title bar uses the existing three-state Ursa control.");
    var button = toggle.GetVisualDescendants().OfType<Button>().Single(item => item.Name == "PART_ThemeButton");
    foreach (var width in new[] { 1120, 1440 })
    {
        shell.Width = width;
        foreach (var palette in model.Palettes)
        {
            model.SelectedPalette = palette;
            model.ThemeModeIndex = (int)StudioThemeMode.System;
            foreach (var expected in new[] { StudioThemeMode.Light, StudioThemeMode.Dark, StudioThemeMode.System })
            {
                Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
                button.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
                Require(model.ThemeModeIndex == (int)expected, "Ursa button cycles all three persisted modes: " + expected);
                Require(ReferenceEquals(model.SelectedPalette, palette), "Mode changes preserve the selected palette.");
                var actual = Application.Current!.RequestedThemeVariant;
                Require(actual == palette.ResolveVariant(model.IsDarkMode), "Ursa control preserves the Studio palette variant: " + actual);
            }
        }
        language.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        Require(language.IsDropDownOpen && language.Items.Count == model.Cultures.Count, "Language icon opens the complete language list.");
        foreach (var culture in model.Cultures)
        {
            language.SelectedItem = culture;
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            Require(I18nManager.Instance.Culture.Name == culture.Name, "Language popup uses the global manager.");
            Require(global::Avalonia.Controls.ToolTip.GetTip(button) is TextBlock tip && tip.Text == model.ThemeModes[model.ThemeModeIndex],
                "The existing Ursa button tooltip follows the host language.");
        }
        language.IsDropDownOpen = false;
        Require(toggle.Bounds.Width == 30 && language.Bounds.Height == 30, "Title-bar controls keep compact dimensions.");
        Require(!shell.GetVisualDescendants().OfType<ComboBox>().Any(item => ReferenceEquals(item.ItemsSource, model.Palettes)),
            "The title bar has one palette entry, with no duplicate dropdown.");
    }
    Console.WriteLine("PASS title bar: eight palettes, actual Ursa three-state clicks, language dropdown and compact widths.");
}
