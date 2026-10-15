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
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
DemoBootstrapper.Initialize();
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
