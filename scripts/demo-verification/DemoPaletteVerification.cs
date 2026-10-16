using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Demo.Avalonia.Views.Scenes;

/// <summary>验证 NuGet 消费端真实外壳、命令搜索、通知和工作流的主题状态。</summary>
internal static class DemoPaletteVerification
{
    public static int Run(string output)
    {
        Directory.CreateDirectory(output);
        var failures = new List<string>();
        var measurements = new List<string> { "mode\tstate\tforeground\tbackground\tcontrast" };
        var modes = Enum.GetValues<StudioPalette>().SelectMany(palette => new[] { false, true }
            .Select(dark => (Palette: palette, Dark: dark, Name: palette + (dark ? "-dark" : "-light")))).ToArray();
        var shell = new MainWindow();
        var model = (MainViewModel)shell.DataContext!;
        var scene = new RuntimeFlowPage();
        scene.GoToStep(1);
        var sceneWindow = new Window { Width = 1100, Height = 720, Content = scene };
        var iconPage = new ImageIconPage();
        var iconWindow = new Window { Width = 1100, Height = 720, Content = iconPage };
        try
        {
            shell.Show();
            model.ResetLayout();
            sceneWindow.Show();
            iconWindow.Show();
            Pump(shell, sceneWindow, iconWindow);
            foreach (var mode in modes)
            {
                var previousFailures = failures.Count;
                model.SelectedPalette = model.Palettes.Single(palette => palette.Palette == mode.Palette);
                model.IsDarkMode = mode.Dark;
                foreach (var window in new[] { shell, sceneWindow, iconWindow })
                    window.RequestedThemeVariant = StudioTheme.GetVariant(mode.Palette, mode.Dark);
                Pump(shell, sceneWindow, iconWindow);
                var canvas = Resource(shell, "StudioBackground");
                var raised = Resource(shell, "StudioRaised");
                var accent = Resource(shell, "StudioAccent");
                model.SelectedGroup = model.Groups[0];
                model.CommandPaletteOpen = false;
                model.Notifications.Clear();
                Pump(shell);
                var commands = shell.FindControl<ScrollViewer>("RibbonCommands")!.GetVisualDescendants().OfType<Button>().ToArray();
                var selected = commands.Single(button => button.Classes.Contains("selected"));
                var ordinary = commands.First(button => !button.Classes.Contains("selected"));
                shell.MouseMove(new Point(2, shell.Bounds.Height - 2));
                Pump(shell);
                CheckText(selected, canvas, mode.Name, "Ribbon/selected");
                CheckText(ordinary, canvas, mode.Name, "Ribbon/normal");
                Require(ColorOf(Presenter(selected).Foreground!, canvas) == accent, mode.Name + "/Ribbon selected accent");
                var center = ordinary.TranslatePoint(new Point(ordinary.Bounds.Width / 2, ordinary.Bounds.Height / 2), shell)!.Value;
                shell.MouseMove(center);
                Pump(shell);
                CheckText(ordinary, canvas, mode.Name, "Ribbon/hover");
                Require(ColorOf(Presenter(ordinary).Background!, canvas) == raised, mode.Name + "/Ribbon hover surface");
                shell.MouseDown(center, MouseButton.Left);
                Pump(shell);
                CheckText(ordinary, canvas, mode.Name, "Ribbon/pressed");
                Require(ColorOf(Presenter(ordinary).Background!, canvas) == Resource(shell, "StudioBorder"), mode.Name + "/Ribbon pressed surface");
                // 在按钮外抬起，避免改变待检查文档。
                shell.MouseMove(new Point(2, shell.Bounds.Height - 2));
                shell.MouseUp(new Point(2, shell.Bounds.Height - 2), MouseButton.Left);
                ordinary.IsEnabled = false;
                Pump(shell);
                Require(!ordinary.IsEffectivelyEnabled, mode.Name + "/Ribbon disabled is effective");
                Require(Presenter(ordinary).Foreground is ISolidColorBrush, mode.Name + "/Ribbon disabled has a resolved foreground");
                Require(ColorOf(Presenter(ordinary).Foreground!, canvas) != Resource(shell, "StudioText"),
                    mode.Name + "/Ribbon disabled text differs from normal text");
                var disabledIcon = ordinary.GetVisualDescendants().OfType<PathIcon>().Single(icon => icon.Classes.Contains("ribbon-icon"));
                Require(ColorOf(disabledIcon.Foreground!, canvas) != accent, mode.Name + "/Ribbon disabled icon differs from normal accent");
                Save(shell, Path.Combine(output, mode.Name + "-ribbon.png"));
                ordinary.ClearValue(InputElement.IsEnabledProperty);

                model.CommandQuery = string.Empty;
                model.CommandPaletteOpen = true;
                foreach (var kind in Enum.GetValues<DemoNotificationKind>())
                    model.Notify(kind, kind.ToString(), "CodeWF.Toolkit");
                Pump(shell);
                var overlay = shell.GetVisualDescendants().OfType<ShellOverlay>().Single();
                var countBadge = shell.FindControl<Border>("NotificationCountBadge")!;
                var countText = shell.FindControl<TextBlock>("NotificationCountText")!;
                Require(countBadge.IsEffectivelyVisible && countText.Text == model.NotificationCount.ToString(CultureInfo.CurrentCulture),
                    mode.Name + $"/Notification count reflects the visible notifications; visible={countBadge.IsEffectivelyVisible}, text={countText.Text}, count={model.NotificationCount}");
                Check(countText.Foreground!, ColorOf(countBadge.Background!, canvas), mode.Name, "Notification/count");
                var commandBox = overlay.FindControl<Border>("CommandBox")!;
                Require(ColorOf(commandBox.Background!, canvas) == Resource(shell, "StudioSurface"), mode.Name + "/Command popup surface");
                var results = overlay.FindControl<ListBox>("CommandResults")!;
                Require(results.SelectedIndex == 0, mode.Name + "/Command initial visual selection");
                model.MoveCommandSelection(1);
                Pump(shell);
                Require(results.SelectedIndex == 1 && results.ContainerFromIndex(1) is ListBoxItem { IsSelected: true },
                    mode.Name + "/Command keyboard index updates the visible selection");
                var selectedResult = (ListBoxItem)results.ContainerFromIndex(1)!;
                CheckText(selectedResult, Resource(shell, "StudioSurface"), mode.Name, "Command/selected");
                var hint = overlay.FindControl<TextBlock>("CommandHintText")!;
                Check(hint.Foreground!, Resource(shell, "StudioSurface"), mode.Name, "Command/hint");
                var command = results.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("cpm-item")).ElementAt(2);
                var commandPoint = command.TranslatePoint(new Point(command.Bounds.Width / 2, command.Bounds.Height / 2), shell)!.Value;
                shell.MouseMove(commandPoint);
                Pump(shell);
                CheckText(command, Resource(shell, "StudioSurface"), mode.Name, "Command/hover");
                shell.MouseDown(commandPoint, MouseButton.Left);
                Pump(shell);
                CheckText(command, Resource(shell, "StudioSurface"), mode.Name, "Command/pressed");
                Require(ColorOf(Presenter(command).Background!, canvas) == Resource(shell, "StudioBorder"), mode.Name + "/Command pressed surface");
                shell.MouseMove(new Point(2, shell.Bounds.Height - 2));
                shell.MouseUp(new Point(2, shell.Bounds.Height - 2), MouseButton.Left);
                Pump(shell);
                foreach (var card in overlay.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("notification-card")))
                {
                    var notification = (DemoNotification)card.DataContext!;
                    var key = notification.Kind switch
                    {
                        DemoNotificationKind.Success => "SemiColorSuccess",
                        DemoNotificationKind.Warning => "SemiColorWarning",
                        DemoNotificationKind.Error => "SemiColorDanger",
                        _ => "SemiColorInformation",
                    };
                    var glyph = card.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("notification-glyph"));
                    var background = ColorOf(card.Background!, canvas);
                    Require(ColorOf(glyph.Foreground!, background) == Resource(shell, key), mode.Name + "/Notification/" + notification.Kind + "/dynamic semantic color");
                    Check(glyph.Foreground!, background, mode.Name, "Notification/" + notification.Kind);
                    foreach (var label in card.GetVisualDescendants().OfType<TextBlock>().Where(label => !label.Classes.Contains("notification-glyph")))
                        Check(label.Foreground!, background, mode.Name, "Notification/" + notification.Kind + "/text-" + label.FontSize, label.Opacity);
                }
                Save(shell, Path.Combine(output, mode.Name + "-overlay.png"));
                model.CommandPaletteOpen = false;

                var steps = scene.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("scene-step")).ToArray();
                Require(steps[1].Classes.Contains("active"), mode.Name + "/Scene active state");
                Require(ColorOf(steps[1].BorderBrush!, canvas) == accent, mode.Name + "/Existing scene active border follows palette");
                Require(ColorOf(steps[2].BorderBrush!, canvas) == Resource(shell, "StudioBorder"), mode.Name + "/Existing scene normal border follows palette");
                var doneMarker = steps[0].GetVisualDescendants().OfType<Border>().Single(border => border.Classes.Contains("step-no"));
                var doneText = (TextBlock)doneMarker.Child!;
                Require(doneMarker.Classes.Contains("done") && ColorOf(doneText.Foreground!, raised) == Resource(shell, "SemiColorSuccess"),
                    mode.Name + "/Existing scene completed marker follows semantic palette");
                Check(doneText.Foreground!, ColorOf(doneMarker.Background!, canvas), mode.Name, "Scene/completed");
                CheckText(steps[1], canvas, mode.Name, "Scene/active");
                foreach (var preview in iconPage.GetVisualDescendants().OfType<Border>().Where(border => border.Child is TextBlock && border.CornerRadius.TopLeft == 6))
                {
                    Require(ColorOf(preview.Background!, canvas) == Resource(shell, "StudioPrimary"), mode.Name + "/Generated icon follows palette");
                    Check(((TextBlock)preview.Child!).Foreground!, ColorOf(preview.Background!, canvas), mode.Name, "Icon/generated");
                }
                Save(sceneWindow, Path.Combine(output, mode.Name + "-scene.png"));
                Console.WriteLine($"{(failures.Count == previousFailures ? "PASS" : "FAIL")} Toolkit palette states: {mode.Name}, failures={failures.Count - previousFailures}");
            }
            Console.WriteLine($"Toolkit palette audit: {modes.Length} modes, {measurements.Count - 1} contrast measurements, {failures.Count} failures; {output}");
            if (failures.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, failures));
            return 0;
        }
        finally
        {
            File.WriteAllLines(Path.Combine(output, "contrast.tsv"), measurements);
            File.WriteAllLines(Path.Combine(output, "failures.txt"), failures);
            iconWindow.Close();
            sceneWindow.Close();
            shell.Close();
        }
        void Require(bool condition, string context) { if (!condition) failures.Add(context); }
        void Check(IBrush foreground, Color background, string mode, string state, double opacity = 1)
        {
            var text = ColorOf(foreground, background, opacity);
            var contrast = Contrast(text, background);
            measurements.Add(string.Join('\t', mode, state, text, background, contrast.ToString("F3", CultureInfo.InvariantCulture)));
            Require(contrast >= 4.5, $"{mode}/{state}: {text} on {background} = {contrast:F2}:1");
        }
        void CheckText(Control control, Color behind, string mode, string state)
        {
            var presenter = control.GetVisualDescendants().OfType<ContentPresenter>().First(item => item.Name == "PART_ContentPresenter");
            var background = ColorOf(presenter.Background ?? Brushes.Transparent, behind);
            Check(presenter.Foreground!, background, mode, state);
            foreach (var label in control.GetVisualDescendants().OfType<TextBlock>())
                Check(label.Foreground!, background, mode, state + "/label-" + label.FontSize, label.Opacity);
        }
    }

    private static ContentPresenter Presenter(Button button) => button.GetVisualDescendants().OfType<ContentPresenter>()
        .Single(presenter => presenter.Name == "PART_ContentPresenter");

    private static Color Resource(Control control, string key)
    {
        if (!control.TryFindResource(key, control.ActualThemeVariant, out var resource) || resource is not ISolidColorBrush brush)
            throw new InvalidOperationException("Missing solid theme resource: " + key);
        return brush.Color;
    }

    private static void Pump(params Window[] windows)
    {
        for (var pass = 0; pass < 3; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            foreach (var window in windows) window.UpdateLayout();
        }
    }

    private static void Save(Window window, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var bitmap = new RenderTargetBitmap(new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height));
        bitmap.Render(window);
        using var stream = File.Create(path);
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
    }

    // 透明画刷先合成到底色，再计算相对亮度与文本对比度。
    private static Color ColorOf(IBrush brush, Color background, double opacity = 1)
    {
        if (brush is not ISolidColorBrush solid) throw new InvalidOperationException("Expected resolved solid brush: " + brush);
        var alpha = solid.Color.A / 255d * solid.Opacity * opacity;
        byte Blend(byte front, byte back) => (byte)Math.Round(front * alpha + back * (1 - alpha));
        return Color.FromRgb(Blend(solid.Color.R, background.R), Blend(solid.Color.G, background.G), Blend(solid.Color.B, background.B));
    }

    private static double Contrast(Color first, Color second)
    {
        static double Linear(byte component)
        {
            var value = component / 255d;
            return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
        }
        static double Light(Color color) => .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
        return (Math.Max(Light(first), Light(second)) + .05) / (Math.Min(Light(first), Light(second)) + .05);
    }
}
