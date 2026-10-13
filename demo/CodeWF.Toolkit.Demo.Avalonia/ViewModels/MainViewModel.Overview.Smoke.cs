using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

public partial class MainViewModel
{
    // Deterministic visual QA of the native application, independent of persisted user layout.
    public async Task CaptureStudioAsync(Window window, string directory)
    {
        Directory.CreateDirectory(directory);
        var overview = (Views.Pages.WelcomePage)Documents.First(d => d.Page is Views.Pages.WelcomePage).Page;
        // Dock creates document content after the window's Loaded event.
        var readyBy = DateTime.UtcNow.AddSeconds(10);
        while (!overview.IsLoaded || overview.Bounds.Width <= 0 || overview.Bounds.Height <= 0)
        {
            if (DateTime.UtcNow >= readyBy)
                throw new TimeoutException("Overview did not load before visual capture.");
            await Task.Delay(50);
            window.UpdateLayout();
        }
        foreach (var palette in Palettes.Take(3))
        {
            SelectedPalette = palette;
            foreach (var dark in new[] { true, false })
            {
                IsDarkMode = dark;
                overview.Overview.LabIndex = 0;
                await Task.Delay(350);
                window.UpdateLayout();
                Save("" + palette.TitleKey.Split('.').Last() + (dark ? "-dark" : "-light"));
            }
        }
        SelectedPalette = Palettes[0];
        IsDarkMode = true;
        for (var tab = 1; tab <= 2; tab++)
        {
            overview.Overview.LabIndex = tab;
            if (tab == 2) await overview.Overview.RunPipelineAsync();
            await Task.Delay(350);
            window.UpdateLayout();
            Save("lab-" + tab);
        }
        overview.Overview.LabIndex = 0;
        var scroll = overview.GetVisualDescendants().OfType<ScrollViewer>().First();
        scroll.Offset = new Vector(0, scroll.Extent.Height);
        await Task.Delay(250);
        window.UpdateLayout();
        Save("capabilities");
        scroll.Offset = default;
        SelectedCulture = Cultures[1];
        await Task.Delay(250);
        window.UpdateLayout();
        Save("english");
        SelectedCulture = Cultures[0];
        window.Width = 1120;
        window.Height = 640;
        await Task.Delay(250);
        window.UpdateLayout();
        Save("compact-window");

        void Save(string name)
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height));
            bitmap.Render(window);
            bitmap.Save(Path.Combine(directory, name + ".png"), new PngBitmapEncoderOptions());
        }
    }

    private async Task<bool> SmokeOverviewAsync(CancellationToken cancellation)
    {
        try
        {
            foreach (var palette in Palettes.Take(3))
            {
                foreach (var dark in new[] { false, true })
                {
                    var variant = palette.ResolveVariant(dark);
                    Application.Current!.RequestedThemeVariant = variant;
                    if (!Application.Current.TryFindResource("StudioAccent", variant, out var brush) || brush is not IBrush)
                        throw new InvalidOperationException("Missing StudioAccent: " + variant.Key);
                }
            }
            var overview = (Views.Pages.WelcomePage)Documents.First(d => d.Page is Views.Pages.WelcomePage).Page;
            var vm = overview.Overview;
            await vm.PublishAsync();
            if (vm.ReceivedCount != 1) throw new InvalidOperationException("No subscriber delivery");
            vm.ToggleSubscription();
            await vm.PublishAsync();
            if (vm.IsSubscribed || vm.ReceivedCount != 1) throw new InvalidOperationException("Unsubscribe failed");
            vm.ToggleSubscription();
            foreach (var value in new[] { "0", "-1", "42", "2147483647", "-2147483648" })
            {
                vm.IntegerText = value;
                vm.Encode();
                if (!vm.ByteOutput.Contains(value + " → " + value)) throw new InvalidOperationException("Int32 round-trip: " + value);
            }
            vm.IntegerText = "invalid";
            vm.Encode();
            if (vm.IsIntegerValid) throw new InvalidOperationException("Int32 validation failed");
            vm.IntegerText = "42";
            vm.Encode();
            await vm.RunPipelineAsync();
            if (vm.PipelineStep != 4 || !vm.HasOutput) throw new InvalidOperationException(vm.PipelineOutput);
            var output = vm.OutputDirectory!;
            using (var zip = System.IO.Compression.ZipFile.OpenRead(Path.Combine(output, "bundle.zip")))
            {
                if (zip.Entries.Count != 2) throw new InvalidOperationException("ZIP artifact entries");
            }
            if (new FileInfo(Path.Combine(output, "qr.png")).Length == 0) throw new InvalidOperationException("Empty QR artifact");
            foreach (var key in new[] { DemoKeys.ItemCore, DemoKeys.ItemBus, DemoKeys.ItemFilesFormat, DemoKeys.ItemImageQr, DemoKeys.ItemLogCore, DemoKeys.ItemNetPacket })
            {
                overview.Overview.NavigateCommand.Execute(key);
                if (ActiveDocument?.TitleKey != key) throw new InvalidOperationException("Overview navigation failed: " + key);
            }
            overview.Overview.NavigateCommand.Execute(DemoKeys.ItemWelcome);
            foreach (var culture in Cultures)
            {
                SelectedCulture = culture;
                await Task.Delay(30, cancellation);
                if (ActiveDocument?.DockDocument?.Title != global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoKeys.ItemWelcome))
                    throw new InvalidOperationException("Dock title was not localized: " + culture.Name);
            }
            SelectedCulture = Cultures[0];
            Logger.Info("Overview smoke: theme resources, six capability routes, and interactions passed");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("Overview smoke failed", ex);
            return false;
        }
        finally { ApplyAppearance(); }
    }
}
