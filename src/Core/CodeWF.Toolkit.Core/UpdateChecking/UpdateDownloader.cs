using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>
/// 把 GitHub Release 资产下载到临时目录。使用临时文件和原子改名，
/// 避免半个安装包被误当成可执行文件；下载完成后按同名 <c>.sha256</c> 校验，
/// 校验失败或中途断开都会清掉临时文件。
/// </summary>
public sealed class UpdateDownloader
{
    private const int BufferSize = 80 * 1024;

    private readonly HttpClient _http;
    private readonly UpdateServiceOptions _options;

    public UpdateDownloader(HttpClient http, UpdateServiceOptions options)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<UpdateDownloadResult> DownloadAsync(
        UpdateInfo update,
        IProgress<UpdateDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (string.IsNullOrWhiteSpace(update.AssetUrl))
        {
            throw new InvalidOperationException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.AssetMissing"));
        }

        if (string.IsNullOrWhiteSpace(update.ChecksumUrl))
        {
            throw new InvalidOperationException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.ChecksumMissing"));
        }

        if (!IsHttpsUrl(update.AssetUrl) || !IsHttpsUrl(update.ChecksumUrl))
        {
            throw new InvalidOperationException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.HttpsRequired"));
        }

        string fileName = Path.GetFileName((update.AssetName ?? string.Empty).Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(fileName) || fileName is "." or ".." || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.InvalidAssetName"));
        }

        string directory = _options.EffectiveDownloadDirectory;
        Directory.CreateDirectory(directory);
        string finalPath = Path.Combine(directory, fileName);
        string partialPath = finalPath + ".download";

        using var request = new HttpRequestMessage(HttpMethod.Get, update.AssetUrl);
        request.Headers.TryAddWithoutValidation("Accept", "application/octet-stream");
        request.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);
        using HttpResponseMessage response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength ?? update.AssetSize;
        long received = 0;
        try
        {
            await using (Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (FileStream target = new(
                partialPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: BufferSize,
                useAsync: true))
            {
                byte[] buffer = new byte[BufferSize];
                int read;
                while ((read = await source.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    received += read;
                    progress?.Report(new UpdateDownloadProgress(received, totalBytes));
                }

                await target.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            if (totalBytes is { } expectedBytes && received != expectedBytes)
            {
                throw new InvalidDataException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.LengthMismatch"));
            }

            await VerifyChecksumAsync(partialPath, update.ChecksumUrl!, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(partialPath, finalPath, overwrite: true);
        }
        catch
        {
            TryDelete(partialPath);
            throw;
        }

        progress?.Report(new UpdateDownloadProgress(received, totalBytes ?? received));
        _options.Log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format("Toolkit.Core.Update.Downloaded", finalPath));
        return new UpdateDownloadResult(finalPath, fileName, received);
    }

    /// <summary>拉取同名 .sha256 并比对；文件格式不对或哈希不一致都抛 <see cref="InvalidDataException"/>。</summary>
    private async Task VerifyChecksumAsync(string path, string checksumUrl, CancellationToken cancellationToken)
    {
        using var checksumRequest = new HttpRequestMessage(HttpMethod.Get, checksumUrl);
        checksumRequest.Headers.TryAddWithoutValidation("Accept", "application/octet-stream");
        checksumRequest.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);
        using HttpResponseMessage checksumResponse = await _http
            .SendAsync(checksumRequest, cancellationToken)
            .ConfigureAwait(false);
        checksumResponse.EnsureSuccessStatusCode();

        string checksumText = await checksumResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        string[] checksumTokens = checksumText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (checksumTokens.Length == 0 || checksumTokens[0].Length != 64 || !checksumTokens[0].All(Uri.IsHexDigit))
        {
            throw new InvalidDataException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.InvalidChecksum"));
        }

        await using FileStream downloaded = File.OpenRead(path);
        byte[] actualHash = await SHA256.HashDataAsync(downloaded, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(Convert.ToHexString(actualHash), checksumTokens[0], StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(global::CodeWF.Toolkit.Core.LibraryText.Get("Toolkit.Core.Update.ChecksumMismatch"));
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // 残留的 .download 会在下次下载时被覆盖，无害
        }
    }

    private static bool IsHttpsUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out Uri? parsed) && parsed.Scheme == Uri.UriSchemeHttps;
}
