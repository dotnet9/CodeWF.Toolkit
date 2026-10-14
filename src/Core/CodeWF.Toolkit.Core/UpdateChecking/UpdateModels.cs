using System;
namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>检查结果，区分无更新和检查失败。</summary>
public sealed record UpdateCheckResult(UpdateInfo? Update, bool Succeeded, string? Error)
{
    public static UpdateCheckResult Latest() => new(null, true, null);

    public static UpdateCheckResult Failed(string error) => new(null, false, error);
}

/// <summary>发现新版本时的信息。</summary>
public sealed record UpdateInfo(
    Version Version,
    string Tag,
    string Title,
    string? Notes,
    string PageUrl,
    string? AssetUrl,
    string? AssetName,
    string? ChecksumUrl = null,
    long? AssetSize = null);

/// <summary>下载进度：总长度未知时 <see cref="TotalBytes"/> 为 null，界面只显示已下载量。</summary>
public sealed record UpdateDownloadProgress(long BytesReceived, long? TotalBytes)
{
    public double? Percentage => TotalBytes is > 0
        ? BytesReceived * 100d / TotalBytes.Value
        : null;
}

/// <summary>下载完成的安装包：<see cref="FilePath"/> 可直接交给系统安装程序。</summary>
public sealed record UpdateDownloadResult(string FilePath, string FileName, long BytesReceived);
