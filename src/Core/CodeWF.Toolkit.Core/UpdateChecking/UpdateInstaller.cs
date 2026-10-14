using LibraryResources = global::CodeWF.Toolkit.Core.I18n.ResourceKeys;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>安装包的处置方式。</summary>
public enum UpdateInstallKind
{
    /// <summary>已交给系统安装程序：Windows 的 setup.exe、Linux 的包管理器、macOS 的 dmg。</summary>
    Launched,

    /// <summary>系统里没有可用的安装器（如 Linux 缺 xdg-open），需要用户手动安装。</summary>
    Manual,

    /// <summary>便携包解压即用，只打开所在目录。</summary>
    OpenedFolder,

    /// <summary>当前平台没有可执行的处置方式，把安装包路径交给界面显示。</summary>
    Unsupported
}

/// <summary>安装结果：<see cref="Message"/> 可直接显示给用户。</summary>
public sealed record UpdateInstallResult(UpdateInstallKind Kind, string Message)
{
    /// <summary>安装程序已拉起，调用方应随后退出应用（Windows 的 Inno 安装包会等待文件释放）。</summary>
    public bool ShouldExitApplication { get; init; } = Kind == UpdateInstallKind.Launched && OperatingSystem.IsWindows();
}

/// <summary>
/// 跨平台安装分发：
/// - Windows：把 <c>-setup.exe</c> 交给系统安装向导（Inno 的 CloseApplications 会处理占用），随后应用应退出；
/// - Linux：<c>.deb</c> 交给系统包管理器界面（xdg-open）；
/// - macOS：打开 dmg 并提示把应用拖入「应用程序」，不退出当前进程；
/// - 便携 zip：只打开所在目录。
/// </summary>
public sealed class UpdateInstaller
{
    private readonly Action<string>? _log;
    private readonly OSPlatform _platform;
    private readonly Func<ProcessStartInfo, bool> _startProcess;

    public UpdateInstaller(Action<string>? log = null) : this(
        OperatingSystem.IsWindows() ? OSPlatform.Windows : OperatingSystem.IsMacOS() ? OSPlatform.OSX : OSPlatform.Linux,
        StartProcess, log)
    {
    }

    internal UpdateInstaller(OSPlatform platform, Func<ProcessStartInfo, bool> startProcess, Action<string>? log = null)
    {
        _platform = platform;
        _startProcess = startProcess;
        _log = log;
    }

    public Task<UpdateInstallResult> InstallAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult(new UpdateInstallResult(
                UpdateInstallKind.Unsupported, global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.InstallFileMissing)));
        }

        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".zip")
        {
            return Task.FromResult(OpenFolder(filePath));
        }

        if (_platform == OSPlatform.Windows)
        {
            return Task.FromResult(extension == ".exe" ? Launch(filePath) : Manual(filePath));
        }

        if (_platform == OSPlatform.Linux)
        {
            return Task.FromResult(extension == ".deb" ? LaunchVia("xdg-open", filePath) : Manual(filePath));
        }

        if (_platform == OSPlatform.OSX)
        {
            return Task.FromResult(extension is ".dmg" or ".pkg"
                ? LaunchVia("open", filePath, global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.MacInstallerOpened))
                : Manual(filePath));
        }

        return Task.FromResult(Manual(filePath));
    }

    /// <summary>启动安装程序本身（Windows：双击 setup.exe）。</summary>
    private UpdateInstallResult Launch(string filePath)
    {
        if (!TryStart(new ProcessStartInfo(filePath) { UseShellExecute = true }))
        {
            return Manual(filePath);
        }

        _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.InstallerStartedLog, filePath));
        return new UpdateInstallResult(
            UpdateInstallKind.Launched, global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.WindowsInstallerStarted)) { ShouldExitApplication = true };
    }

    /// <summary>交给系统处理器打开（Linux 的 xdg-open、macOS 的 open）；命令不存在时降级为手动。</summary>
    private UpdateInstallResult LaunchVia(string command, string filePath, string? message = null)
    {
        if (TryStart(new ProcessStartInfo(command) { UseShellExecute = false, ArgumentList = { filePath } }))
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.OpenedWith, command, filePath));
            return new UpdateInstallResult(
                UpdateInstallKind.Launched,
                message ?? global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.SystemInstallerStarted)) { ShouldExitApplication = false };
        }

        _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.CommandUnavailableLog, command, filePath));
        return Manual(filePath, global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.CommandUnavailable, command, filePath));
    }

    private static UpdateInstallResult Manual(string filePath, string? message = null)
        => new(UpdateInstallKind.Manual, message ?? global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.InstallManually, filePath));

    private UpdateInstallResult OpenFolder(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directory))
        {
            return new UpdateInstallResult(UpdateInstallKind.Unsupported, global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.InstallerDownloaded, filePath));
        }

        ProcessStartInfo info = _platform == OSPlatform.Windows
            ? new ProcessStartInfo("explorer.exe") { UseShellExecute = false, ArgumentList = { "/select,", filePath } }
            : _platform == OSPlatform.OSX
                ? new ProcessStartInfo("open") { UseShellExecute = false, ArgumentList = { directory } }
                : new ProcessStartInfo("xdg-open") { UseShellExecute = false, ArgumentList = { directory } };
        if (!TryStart(info))
        {
            return Manual(filePath);
        }
        return new UpdateInstallResult(UpdateInstallKind.OpenedFolder, global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.PortableFolderOpened, directory));
    }

    private bool TryStart(ProcessStartInfo info)
    {
        try
        {
            return _startProcess(info);
        }
        catch (Exception ex)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.InstallerFailed, ex.Message));
            return false;
        }
    }

    private static bool StartProcess(ProcessStartInfo info)
    {
        using Process? process = Process.Start(info);
        return process is not null;
    }
}
