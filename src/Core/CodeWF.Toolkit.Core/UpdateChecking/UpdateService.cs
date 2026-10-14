using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>
/// 「检查 → 下载 → 安装」的完整更新流程。应用只需注入它，界面按状态机展示结果：
/// 所有网络、选包、校验与安装逻辑都在 Toolkit 里，应用侧不重复实现。
/// </summary>
public interface IUpdateService
{
    /// <summary>应用配置（仓库、产品名、平台、缓存位置）。</summary>
    UpdateServiceOptions Options { get; }

    /// <summary>当前应用版本，取自程序集的 <c>AssemblyInformationalVersion</c>。</summary>
    Version CurrentVersion { get; }

    /// <summary>当前应用版本的可显示文本（如 <c>1.5.3</c>）。</summary>
    string CurrentVersionText { get; }

    /// <summary>检查更新；不传 <paramref name="current"/> 时用 <see cref="CurrentVersion"/> 比较。</summary>
    Task<UpdateCheckResult> CheckAsync(Version? current = null, CancellationToken cancellationToken = default);

    /// <summary>下载 <see cref="UpdateInfo.AssetUrl"/> 指向的安装包，带进度回调与 sha256 校验。</summary>
    Task<UpdateDownloadResult> DownloadAsync(
        UpdateInfo update,
        IProgress<UpdateDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>把已下载的安装包交给系统（Windows 安装向导 / Linux 包管理器 / macOS dmg）。</summary>
    Task<UpdateInstallResult> InstallAsync(
        UpdateDownloadResult download,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 应用级更新门面：内部持一份 <see cref="HttpClient"/> 与一个 <see cref="UpdateChecker"/>，
/// 检查失败返回结果；下载或取消通过异常通知调用方，由应用状态机统一展示。
/// </summary>
public sealed class UpdateService : IUpdateService, IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly UpdateChecker _checker;
    private readonly UpdateDownloader _downloader;
    private readonly UpdateInstaller _installer;
    private readonly Assembly? _assembly;

    /// <summary>便利构造：内部创建 HttpClient（超时取 <see cref="UpdateServiceOptions.HttpTimeout"/>）。</summary>
    public UpdateService(UpdateServiceOptions options)
        : this(new HttpClient { Timeout = (options ?? throw new ArgumentNullException(nameof(options))).HttpTimeout }, options, ownsHttpClient: true)
    {
    }

    public UpdateService(HttpClient http, UpdateServiceOptions options, bool ownsHttpClient = false)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.Validate();
        _ownsHttpClient = ownsHttpClient;
        _checker = new UpdateChecker(_http, Options);
        _downloader = new UpdateDownloader(_http, Options);
        _installer = new UpdateInstaller(Options.Log);
    }

    /// <summary>指定当前应用程序集（默认取入口程序集），版本比较与关于窗口都以它为准。</summary>
    public UpdateService(HttpClient http, UpdateServiceOptions options, Assembly assembly, bool ownsHttpClient = false)
        : this(http, options, ownsHttpClient)
    {
        _assembly = assembly;
    }

    public UpdateServiceOptions Options { get; }

    public Version CurrentVersion => UpdateVersion.Current(_assembly);

    public string CurrentVersionText => UpdateVersion.Display(_assembly);

    public Task<UpdateCheckResult> CheckAsync(Version? current = null, CancellationToken cancellationToken = default)
        => _checker.CheckAsync(current ?? CurrentVersion, cancellationToken);

    public Task<UpdateDownloadResult> DownloadAsync(
        UpdateInfo update,
        IProgress<UpdateDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => _downloader.DownloadAsync(update, progress, cancellationToken);

    public Task<UpdateInstallResult> InstallAsync(
        UpdateDownloadResult download,
        CancellationToken cancellationToken = default)
        => _installer.InstallAsync(download?.FilePath ?? string.Empty, cancellationToken);

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }
    }
}
