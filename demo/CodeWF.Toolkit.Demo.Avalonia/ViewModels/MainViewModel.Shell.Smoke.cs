using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>
/// 外壳实体化件的冒烟用例（随 <c>--smoke</c> 一起跑）：
/// 通知中心容量、命令面板过滤与执行、密度切换、布局存档往返、导航过滤与就地重命名。
/// 任一用例失败都会让 <c>--smoke</c> 退出码非 0。
/// </summary>
public partial class MainViewModel
{
    private async Task<bool> SmokeShellFeaturesAsync(CancellationToken cancellation)
    {
        var ok = true;

        ok &= Check("Language changes preserve property command identity", () =>
        {
            var originalCulture = SelectedCulture;
            var commit = GlobalValueCommittedCommand;
            var browse = GlobalBrowseRequestedCommand;
            try
            {
                foreach (var culture in Cultures)
                {
                    SelectedCulture = culture;
                    Expect(ReferenceEquals(commit, GlobalValueCommittedCommand), "Commit command stays bound");
                    Expect(ReferenceEquals(browse, GlobalBrowseRequestedCommand), "Browse command stays bound");
                    Expect(GlobalProperties.Count > 0, "Translated settings remain available");
                }
            }
            finally { SelectedCulture = originalCulture; }
        });

        ok &= Check("Active document selection follows Dock activation", () =>
        {
            var items = Groups.SelectMany(group => group.Items).Take(3).ToArray();
            OpenItem(items[1]);
            OpenItem(items[2]);
            _docDock!.ActiveDockable = Documents.First(document => document.TitleKey == items[1].TitleKey).DockDocument;
            Expect(items[1].IsSelected && !items[2].IsSelected, "Ribbon and navigation selection follow the active document");
            Expect(Groups.SelectMany(group => group.Items).Count(item => item.IsSelected) == 1, "Exactly one item selected");
            Expect(ActivePropertyItems.Count > 0, "Property panel always has meaningful settings");
            OpenItem(items[0]);
        });

        ok &= Check("Notification capacity and clear", () =>
        {
            ClearNotifications();
            for (var i = 0; i < 6; i++)
            {
                Notify(DemoNotificationKind.Info, "Notification " + i);
            }

            Expect(Notifications.Count == 4, "At most four notifications retained", Notifications.Count);
            Expect(HasNotifications && NotificationCount == 4, "Badge count updated", NotificationCount);
            ClearNotifications();
            Expect(!HasNotifications, "Badge hidden after clear");
        });

        ok &= Check("Command filtering and execution", () =>
        {
            var all = FilteredCommands;
            Expect(all.Count >= 20, "All document and shell commands listed by default", all.Count);

            CommandQuery = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoResources.Tool.Log);
            var filtered = FilteredCommands;
            Expect(filtered.Count > 0 && filtered.Count < all.Count, "Search returns a narrower result set", filtered.Count);
            MoveCommandSelection(1);
            Expect(CommandIndex >= 0 && CommandIndex < filtered.Count, "Arrow selection remains in range", CommandIndex);

            CommandQuery = "zzz-no-match";
            Expect(FilteredCommands.Count == 0, "No match produces empty list");
            CommandQuery = string.Empty;
        });

        ok &= Check("Density toggle", () =>
        {
            Density = DemoDensity.Comfort;
            ToggleDensityCommand.Execute().Subscribe();
            Expect(Density == DemoDensity.Compact && IsCompactDensity, "Switch to compact", Density);
            Expect(DensityLabel.Length > 0, "Density label localized", DensityLabel);
            ToggleDensityCommand.Execute().Subscribe();
            Expect(Density == DemoDensity.Comfort, "Restore comfortable density", Density);
        });

        ok &= Check("Overlay shortcuts", () =>
        {
            CommandPaletteOpen = true;
            EscapeShellOverlay();
            Expect(!CommandPaletteOpen, "Escape closes the command palette");

            Notify(DemoNotificationKind.Warning, "Pending cleanup");
            EscapeShellOverlay();
            Expect(!HasNotifications, "Escape clears notifications");
        });

        ok &= Check("Navigation filtering", () =>
        {
            var before = FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut));
            NavFilter = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoResources.Tool.Log);
            var hits = FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut));
            Expect(hits > 0 && hits < before, "Filtering reduces visible leaves", hits);
            NavFilter = "zzz-no-match";
            Expect(IsNavEmpty, "No match displays empty state");
            NavFilter = string.Empty;
            Expect(FilteredGroups.Sum(g => g.Items.Count(i => !i.IsFilteredOut)) == before, "Clearing search restores all items");
        });

        ok &= Check("Navigation rename", () =>
        {
            var first = Groups.SelectMany(g => g.Items).First();
            var original = first.DisplayTitle;

            BeginRename(first);
            Expect(IsRenaming && ReferenceEquals(RenameTarget, first), "Enter editing state", RenameTarget?.TitleKey);
            RenameText = "My capability page";
            CommitRename();
            Expect(!IsRenaming, "Editing ends after commit");
            Expect(first.DisplayTitle == "My capability page", "Display title updated", first.DisplayTitle);
            Expect(HasRenames, "Rename marker is visible");

            ResetRenames();
            Expect(first.DisplayTitle == original, "Original title restored", first.DisplayTitle);
            Expect(!HasRenames, "Marker cleared");
        });

        ok &= Check("Layout snapshot round-trip", () =>
        {
            var temp = Path.Combine(Path.GetTempPath(), "codewf-toolkit-smoke-layout.json");
            var item = Groups.SelectMany(g => g.Items).First();

            Density = DemoDensity.Compact;
            IsDarkMode = true;
            BeginRename(item);
            RenameText = "Smoke title";
            CommitRename();

            Expect(SaveLayoutTo(temp), "Write snapshot file", temp);
            var raw = File.ReadAllText(temp);
            Expect(raw.Contains("Smoke title", StringComparison.Ordinal), "Snapshot contains custom title");
            Expect(raw.Contains("Compact", StringComparison.Ordinal), "Snapshot contains density");

            Density = DemoDensity.Comfort;
            IsDarkMode = false;
            ResetRenames();

            Expect(LoadLayoutFrom(temp), "Restore layout snapshot");
            Expect(Density == DemoDensity.Compact, "Density restored", Density);
            Expect(IsDarkMode, "Appearance mode restored", IsDarkMode);
            Expect(item.DisplayTitle == "Smoke title", "Custom title restored", item.DisplayTitle);

            File.Delete(temp);
            item.CustomTitle = null;
            Density = DemoDensity.Comfort;
            IsDarkMode = false;
            Logger.Info("Layout round-trip completed, temporary file deleted: " + temp);
        });

        // 外壳新控件的渲染（导航树面板 + 命令面板 / 通知中心浮层 + 重命名输入框）
        if (!await SmokeRenderShellChromeAsync(cancellation))
        {
            ok = false;
        }

        return ok;
    }

    /// <summary>
    /// 把外壳新控件挂进可视化树渲染一遍：能力页渲染覆盖不到外壳，
    /// 而命令面板与通知中心正是本次新增的 XAML 与绑定。
    /// </summary>
    private async Task<bool> SmokeRenderShellChromeAsync(CancellationToken cancellation)
    {
        var host = await AttachSmokeHostAsync(cancellation);
        if (host is null)
        {
            Logger.Warn("Smoke: no window; shell rendering skipped");
            return true;
        }

        var rendered = 0;
        var failed = 0;
        var item = Groups.SelectMany(g => g.Items).First();

        foreach (var compact in new[] { false, true })
        {
            Density = compact ? DemoDensity.Compact : DemoDensity.Comfort;
            foreach (var paletteOpen in new[] { false, true })
            {
                CommandPaletteOpen = paletteOpen;
                NavFilter = paletteOpen ? global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoResources.Tool.Log) : string.Empty;
                Notify(DemoNotificationKind.Success, "Shell rendering", compact ? "Compact" : "Comfort");

                var chrome = new global::Avalonia.Controls.Grid();
                chrome.Children.Add(new Views.Tools.NavToolView { DataContext = this });
                chrome.Children.Add(new Views.ShellOverlay { DataContext = this });
                host.Content = chrome;

                try
                {
                    LayoutOnce(host);
                    rendered++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Logger.Warn(
                        $"Smoke: shell render failed (compact={compact}, palette={paletteOpen}): {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        BeginRename(item);
        var renameHost = new global::Avalonia.Controls.Grid();
        renameHost.Children.Add(new Views.Tools.NavToolView { DataContext = this });
        host.Content = renameHost;
        try
        {
            LayoutOnce(host);
            rendered++;
        }
        catch (Exception ex)
        {
            failed++;
            Logger.Warn($"Smoke: rename render failed: {ex.GetType().Name}: {ex.Message}");
        }

        CancelRename();
        CommandPaletteOpen = false;
        NavFilter = string.Empty;
        Density = DemoDensity.Comfort;
        host.Content = null;
        ClearNotifications();

        Logger.Info($"Smoke: {rendered} shell renders, {failed} failures");
        return failed == 0;
    }

    private bool Check(string name, Action action)
    {
        try
        {
            action();
            Logger.Info("Smoke: shell check passed: " + name);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Smoke: shell check failed ({name}): {ex.Message}");
            return false;
        }
    }

    private static void Expect(bool condition, string what, object? actual = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(actual is null ? what : $"{what} (actual: {actual})");
        }
    }
}
