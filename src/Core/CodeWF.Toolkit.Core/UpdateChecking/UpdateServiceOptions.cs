using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>
/// 应用的更新服务配置：仓库地址、产品名与运行平台。
/// 默认值按「dotnet9 组织的 GitHub Releases + 当前进程所在平台」推导，应用一般只填 <see cref="Repository"/>。
/// </summary>
public sealed class UpdateServiceOptions
{
    /// <summary>GitHub 组织/用户，默认 <c>dotnet9</c>。</summary>
    public string Owner { get; set; } = "dotnet9";

    /// <summary>GitHub 仓库名，必填（如 <c>Vex</c>）。</summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>产品名：User-Agent 前缀、下载目录名与界面文案都用它。</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Releases API 基地址（仅在 <see cref="AllowApiFallback"/> 打开时使用）。</summary>
    public string ApiBase { get; set; } = "https://api.github.com";

    /// <summary>网页端点基地址。</summary>
    public string WebBase { get; set; } = "https://github.com";

    /// <summary>优先取平台安装包（Windows 的 setup.exe）；关掉则退化为便携 zip。</summary>
    public bool PreferInstaller { get; set; } = true;

    /// <summary>目标运行平台标识（如 <c>win-x64</c>），默认按当前进程的 OS 与架构推导。</summary>
    public string? RuntimeIdentifier { get; set; }

    /// <summary>检查缓存（ETag 与上次采用过的 Release）落盘路径，默认 <c>%LOCALAPPDATA%\{ProductName}\update-state.json</c>。</summary>
    public string? StateFile { get; set; }

    /// <summary>网页端点失败后是否退回 GitHub API（会消耗未认证配额，默认关闭）。</summary>
    public bool AllowApiFallback { get; set; }

    /// <summary>单次 HTTP 请求超时。</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(12);

    /// <summary>下载安装包的目录，默认 <c>%TEMP%\{ProductName}\updates</c>。</summary>
    public string? DownloadDirectory { get; set; }

    /// <summary>日志回调；下载异常和取消仍由调用方处理。</summary>
    public Action<string>? Log { get; set; }

    /// <summary>AppContext.BaseDirectory：数据目录不可用时作为兜底。</summary>
    public string BaseDirectory { get; set; } = AppContext.BaseDirectory;

    /// <summary>校验必填项；缺少 <see cref="Repository"/> 时抛 <see cref="InvalidOperationException"/>。</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Repository))
        {
            throw new InvalidOperationException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.RepositoryRequired"));
        }
    }

    /// <summary>当前（或已显式指定）的平台标识。</summary>
    public string? EffectiveRuntimeIdentifier
        => string.IsNullOrWhiteSpace(RuntimeIdentifier) ? CurrentRuntimeIdentifier() : RuntimeIdentifier;

    /// <summary>检查缓存的落盘路径；未配置时按产品名与本地应用数据目录推导。</summary>
    public string EffectiveStateFile => !string.IsNullOrWhiteSpace(StateFile)
        ? StateFile!
        : Path.Combine(ResolveDataDirectory(), "update-state.json");

    /// <summary>安装包下载目录；未配置时按产品名与系统临时目录推导。</summary>
    public string EffectiveDownloadDirectory => !string.IsNullOrWhiteSpace(DownloadDirectory)
        ? DownloadDirectory!
        : Path.Combine(Path.GetTempPath(), string.IsNullOrWhiteSpace(ProductName) ? Repository : ProductName, "updates");

    /// <summary>User-Agent：GitHub 要求带可识别的客户端标识。</summary>
    public string UserAgent => (string.IsNullOrWhiteSpace(ProductName) ? Repository : ProductName) + "-Updater";

    /// <summary>默认数据目录：<c>%LOCALAPPDATA%\{ProductName}</c>（Linux/macOS 取各自标准位置）。</summary>
    private string ResolveDataDirectory()
    {
        string appName = string.IsNullOrWhiteSpace(ProductName) ? Repository : ProductName;
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return string.IsNullOrWhiteSpace(root) ? BaseDirectory : Path.Combine(root, appName);
    }

    /// <summary>按当前进程的 OS 与架构推导平台标识，认不出时返回 null。</summary>
    public static string? CurrentRuntimeIdentifier()
    {
        string os = OperatingSystem.IsWindows()
            ? "win"
            : OperatingSystem.IsLinux()
                ? "linux"
                : OperatingSystem.IsMacOS()
                    ? "osx"
                    : string.Empty;

        string architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => string.Empty
        };

        return os.Length == 0 || architecture.Length == 0 ? null : os + "-" + architecture;
    }
}
