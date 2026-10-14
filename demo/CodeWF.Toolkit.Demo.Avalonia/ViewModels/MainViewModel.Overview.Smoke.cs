using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

public partial class MainViewModel
{
    // 原生应用视觉验收，使用固定布局，不依赖用户保存的布局。
    public async Task CaptureStudioAsync(Window window, string directory)
    {
        Directory.CreateDirectory(directory);
        var overview = (Views.Pages.WelcomePage)Documents.First(d => d.Page is Views.Pages.WelcomePage).Page;
        // Dock 在窗口 Loaded 事件后创建文档内容。
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
        overview.Overview.ExperimentsExpanded = true;
        for (var tab = 1; tab <= 2; tab++)
        {
            overview.Overview.LabIndex = tab;
            if (tab == 2) await overview.Overview.RunPipelineAsync();
            await Task.Delay(350);
            window.UpdateLayout();
            overview.GetVisualDescendants().OfType<Expander>().First().BringIntoView();
            await Task.Delay(250);
            Save("lab-" + tab);
        }
        overview.Overview.LabIndex = 0;
        overview.Overview.ExperimentsExpanded = false;
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
        window.Width = 1440;
        window.Height = 860;
        foreach (var key in new[] { DemoResources.Item.EventClient, DemoPages.NetPacket, DemoResources.Item.JsonExtensions, DemoPages.ImageIcon, DemoPages.LogCore })
        {
            overview.Overview.NavigateCommand.Execute(key);
            await WaitForDocumentAsync();
            await Task.Delay(250);
            window.UpdateLayout();
            Save(key.Split('.').Last());
            if (ActiveDocument?.Page is Views.Pages.TransportDemoPage transportPage) await transportPage.ViewModel.StopAsync();
            if (ActiveDocument?.Page is Views.Pages.FileSystemDemoPage filePage) await filePage.ViewModel.StopAsync();
        }

        foreach (var key in new[] { DemoResources.Item.JsonExtensions, DemoResources.Item.YamlExtensions, DemoResources.Item.PrismExtensions, DemoResources.Item.FileHelper, DemoResources.Item.EventClient, DemoResources.Item.DataTableExtensions, DemoResources.Item.TcpSocketClientFileSystemFeature })
        {
            overview.Overview.NavigateCommand.Execute(key);
            await WaitForDocumentAsync();
            if (ActiveDocument?.Page is Views.Pages.HelperDemoPage helper) await helper.ViewModel.RunAsync();
            if (ActiveDocument?.Page is Views.Pages.DataTableDemoPage table) await table.ViewModel.RunAsync(true);
            if (ActiveDocument?.Page is Views.Pages.TransportDemoPage transport) { await transport.ViewModel.StartAsync(); await transport.ViewModel.SendAsync(); }
            if (ActiveDocument?.Page is Views.Pages.FileSystemDemoPage files) { await files.ViewModel.StartAsync(); await files.ViewModel.CreateAsync(); }

            await Task.Delay(250);
            window.UpdateLayout();
            await Task.Delay(250);
            window.UpdateLayout();
            if (ActiveDocument?.Page is Views.Pages.DataTableDemoPage or Views.Pages.FileSystemDemoPage)
            {
                var grid = ActiveDocument.Page.FindControl<TreeDataGrid>("PreviewGrid")!;
                var rows = grid.GetVisualDescendants().OfType<global::Avalonia.Controls.Primitives.TreeDataGridRow>().Count(row => row.Bounds.Height > 0);
                Logger.Info($"Capture grid: {key}, loaded={grid.IsLoaded}, bounds={grid.Bounds}, rows={rows}, source={grid.Source?.Rows.Count}");
                if (rows == 0) throw new InvalidOperationException("Dock document grid did not render rows: " + key);
            }
            Save(key.Split('.').Last());
            if (ActiveDocument?.Page is Views.Pages.TransportDemoPage transportPage) await transportPage.ViewModel.StopAsync();
            if (ActiveDocument?.Page is Views.Pages.FileSystemDemoPage filePage) await filePage.ViewModel.StopAsync();
        }

        async Task WaitForDocumentAsync()
        {
            var catalogReadyBy = DateTime.UtcNow.AddSeconds(10);
            while (ActiveDocument?.Page is not { } page || !page.GetVisualAncestors().Contains(window) || page.Bounds.Width <= 0 || page.Bounds.Height <= 0)
            {
                if (DateTime.UtcNow >= catalogReadyBy)
                {
                    Save("capture-timeout");
                    throw new TimeoutException($"Dock document did not load: active={ActiveDocument?.TitleKey}, loaded={ActiveDocument?.Page.IsLoaded}, bounds={ActiveDocument?.Page.Bounds}, attached={ActiveDocument?.Page.GetVisualAncestors().Contains(window)}, dock={_docDock?.ActiveDockable?.Id}");
                }
                window.UpdateLayout();
                await Task.Delay(50);
            }
        }


        foreach (var item in Groups.SelectMany(group => group.Items).Where(item => item.PrototypeId != "welcome"))
        {
            OpenItem(item);
            var catalogReadyBy = DateTime.UtcNow.AddSeconds(10);
            while (ActiveDocument?.Page is not { } page || !page.GetVisualAncestors().Contains(window) || page.Bounds.Width <= 0 || page.Bounds.Height <= 0)
            {
                if (DateTime.UtcNow >= catalogReadyBy) throw new TimeoutException("Catalog page did not load: " + item.TitleKey);
                window.UpdateLayout();
                await Task.Delay(50);
            }
            await Task.Delay(250);
            window.UpdateLayout();
            Save("catalog-" + item.PrototypeId);
            CloseDocument(ActiveDocument!);
        }
        OpenItem(Groups[0].Items[0]);
        foreach (var culture in Cultures)
        {
            SelectedCulture = culture;
            foreach (var size in new[] { new Size(1440, 860), new Size(1120, 640) })
            {
                window.Width = size.Width;
                window.Height = size.Height;
                foreach (var group in Groups)
                {
                    SelectedGroup = group;
                    await Task.Delay(200);
                    window.UpdateLayout();
                    var commands = window.FindControl<ScrollViewer>("RibbonCommands")!.GetVisualDescendants()
                        .OfType<Button>().Select(button => button.DataContext).OfType<DemoItemViewModel>().ToArray();
                    if (commands.Length != group.Items.Count || !commands.SequenceEqual(group.Items))
                        throw new InvalidOperationException("Ribbon commands do not match the catalog: " + group.TitleKey);
                    var icons = window.FindControl<ScrollViewer>("RibbonCommands")!.GetVisualDescendants()
                        .OfType<PathIcon>().Where(icon => icon.Classes.Contains("ribbon-icon")).ToArray();
                    if (icons.Length != commands.Length || icons.Any(icon => icon.Data is null || icon.Bounds.Width != 32 || icon.Bounds.Height != 32))
                        throw new InvalidOperationException("Ribbon vector icons must occupy 32 by 32 pixels.");
                    var quickIcons = window.GetVisualDescendants().OfType<Button>()
                        .Where(button => button.Classes.Contains("ribbon-quick")).Select(button => button.Content).OfType<PathIcon>().ToArray();
                    if (quickIcons.Length != 4 || quickIcons.Any(icon => icon.Data is null || icon.Bounds.Width != 24 || icon.Bounds.Height != 24))
                        throw new InvalidOperationException("Ribbon quick icons must occupy 24 by 24 pixels.");
                    Save($"ribbon-{group.TitleKey.Split('.').Last()}-{(int)size.Width}-{culture.Name}");
                }
            }
        }

        void Save(string name)
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height));
            bitmap.Render(window);
            bitmap.Save(Path.Combine(directory, name + ".png"), new PngBitmapEncoderOptions());
        }
    }

    private async Task<bool> SmokeOverviewAsync(CancellationToken cancellation)
    {
        await SmokeDocumentIsolationAsync(cancellation);
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
            var registration = new BusRegistrationViewModel();
            if (!registration.RunPrism()) throw new InvalidOperationException(registration.PrismOutput);
            if (!registration.RunAspNetCore()) throw new InvalidOperationException(registration.AspNetOutput);
            Logger.Info("Integration smoke: Prism and ASP.NET Core registration, scanning and scoped queries passed");
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
            var student = new Views.Pages.Student { Name = "0001", Age = -42, City = "false" };
            if (!global::CodeWF.Toolkit.Files.Extensions.YamlExtensions.ToYaml(student, OverviewJsonContext.Default.Student, out var yaml, out var yamlError)
                || !global::CodeWF.Toolkit.Files.Extensions.YamlExtensions.FromYaml(yaml, OverviewJsonContext.Default.Student, out var restoredStudent, out yamlError)
                || restoredStudent?.Name != student.Name || restoredStudent.City != student.City || restoredStudent.Age != student.Age)
                throw new InvalidOperationException("Generated YAML round-trip failed: " + yamlError);
            await global::CodeWF.Toolkit.Image.ImageHelper.MergeGenerateIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png"), Path.Combine(output, "sample.ico"), [16, 32]);
            if (new FileInfo(Path.Combine(output, "sample.ico")).Length <= 6) throw new InvalidOperationException("Empty ICO artifact");
            await SmokeLocalTransportsAsync(cancellation);
            Logger.Info("Artifact smoke: generated JSON/YAML metadata, quoted scalar types, ZIP, QR and ICO passed");
            foreach (var key in new[] { DemoPages.Bus, DemoResources.Item.EventClient, DemoPages.NetPacket, DemoResources.Item.JsonExtensions, DemoPages.ImageIcon, DemoPages.LogCore })
            {
                overview.Overview.NavigateCommand.Execute(key);
                if (ActiveDocument?.TitleKey != key) throw new InvalidOperationException("Overview navigation failed: " + key);
            }
            overview.Overview.NavigateCommand.Execute(DemoPages.Welcome);
            foreach (var culture in Cultures)
            {
                SelectedCulture = culture;
                await Task.Delay(30, cancellation);
                if (ActiveDocument?.DockDocument?.Title != global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoPages.Welcome))
                    throw new InvalidOperationException("Dock title was not localized: " + culture.Name);
            }
            SelectedCulture = Cultures[0];
            Logger.Info("Overview smoke: theme resources, six capability routes, and interactions passed");
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Logger.Error("Overview smoke failed", ex);
            return false;
        }
        finally { ApplyAppearance(); }
    }
}
