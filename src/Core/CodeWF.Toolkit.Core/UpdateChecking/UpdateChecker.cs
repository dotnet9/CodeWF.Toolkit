using LibraryResources = global::CodeWF.Toolkit.Core.I18n.ResourceKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>检查 GitHub Releases 更新。</summary>
public interface IUpdateChecker
{
    Task<UpdateCheckResult> CheckAsync(Version current, CancellationToken cancellationToken = default);
}

/// <summary>更新检查缓存：ETag 配 API 条件请求；release 内容配网页检查——tag 没变时直接复用做版本比较。</summary>
public sealed record UpdateCheckState(string? Etag, GitHubRelease? Release);

/// <summary>
/// 检查 GitHub Releases 更新，默认路径完全不碰 api.github.com 的每小时配额：
/// - releases/latest 的 302 落点就是最新发布 tag（未认证 API 的配额只有 60 次/小时/IP，
///   走代理时被同出口用户共享、极易耗尽，网页端点没有这个限制）；
/// - expanded_assets/{tag} 是发布页懒加载资产列表的接口，从中解析资产名与下载直链；
/// - releases.atom 提供该 tag 的发布说明；
/// - tag 与上次缓存一致时连资产请求都省掉，用缓存的 release 做版本比较。
/// 网页端点失败且显式启用 <c>allowApiFallback</c> 时才退回 Releases API：ETag 条件请求 304 不计配额，
/// 403/429 按 Retry-After / X-RateLimit-Reset 退避。任何网络/解析异常都吞掉返回失败结果，不打扰用户。
/// </summary>
public sealed class UpdateChecker : IUpdateChecker
{
    private const string DefaultApiBase = "https://api.github.com";
    private const string DefaultWebBase = "https://github.com";

    private readonly HttpClient _http;
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _apiBase;
    private readonly string _webBase;
    private readonly string _userAgent;
    private readonly Action<string>? _log;
    private readonly bool _preferInstaller;
    private readonly string? _runtimeIdentifier;
    private readonly string? _stateFile;
    private readonly bool _allowApiFallback;
    private DateTime _blockedUntilUtc = DateTime.MinValue;

    /// <summary>便利构造：内部创建 HttpClient（网页端点优先的默认配置）。</summary>
    public UpdateChecker(string owner, string repo, Action<string>? log = null)
        : this(new HttpClient { Timeout = TimeSpan.FromSeconds(12) }, owner, repo, log: log)
    {
    }

    public UpdateChecker(
        HttpClient http,
        string owner,
        string repo,
        string apiBase = DefaultApiBase,
        Action<string>? log = null,
        bool preferInstaller = true,
        string? runtimeIdentifier = null,
        string? stateFile = null,
        string? webBase = null,
        bool allowApiFallback = false,
        string? userAgent = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _owner = owner;
        _repo = repo;
        _apiBase = apiBase.TrimEnd('/');
        _webBase = (webBase ?? DefaultWebBase).TrimEnd('/');
        _userAgent = string.IsNullOrWhiteSpace(userAgent) ? repo + "-UpdateChecker" : userAgent!;
        _log = log;
        _preferInstaller = preferInstaller;
        _runtimeIdentifier = runtimeIdentifier;
        _stateFile = stateFile;
        _allowApiFallback = allowApiFallback;
    }

    /// <summary>按应用配置构造（仓库、产品名、平台、缓存位置都来自 <see cref="UpdateServiceOptions"/>）。</summary>
    public UpdateChecker(HttpClient http, UpdateServiceOptions options)
        : this(http, options ?? throw new ArgumentNullException(nameof(options)), validated: true)
    {
    }

    private UpdateChecker(HttpClient http, UpdateServiceOptions options, bool validated)
        : this(
            http,
            options.Owner,
            CheckRepository(options),
            options.ApiBase,
            options.Log,
            options.PreferInstaller,
            options.EffectiveRuntimeIdentifier,
            options.EffectiveStateFile,
            options.WebBase,
            options.AllowApiFallback,
            options.UserAgent)
    {
    }

    private static string CheckRepository(UpdateServiceOptions options)
    {
        options.Validate();
        return options.Repository;
    }

    public async Task<UpdateCheckResult> CheckAsync(Version current, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateCheckResult viaWeb = await CheckViaWebAsync(current, cancellationToken).ConfigureAwait(false);
            if (viaWeb.Succeeded || !_allowApiFallback)
            {
                return viaWeb;
            }

            cancellationToken.ThrowIfCancellationRequested();
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.ApiFallback, viaWeb.Error));
            return await CheckViaApiAsync(current, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return UpdateCheckResult.Failed(global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.Canceled));
        }
        catch (Exception ex)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.CheckFailed, ex.Message));
            return UpdateCheckResult.Failed(ex.Message);
        }
    }

    /// <summary>网页端点检查：版本与安装包解析分开，安装包列表失败不影响已确认的新版本。</summary>
    private async Task<UpdateCheckResult> CheckViaWebAsync(Version current, CancellationToken cancellationToken)
    {
        try
        {
            string? tag = await FetchLatestTagViaRedirectAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(tag) || VersionUtil.Parse(tag) is not { } candidate)
            {
                return UpdateCheckResult.Failed(global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.InvalidReleaseVersion));
            }

            if (!VersionUtil.IsNewer(candidate, current))
            {
                return UpdateCheckResult.Latest();
            }

            UpdateCheckState? state = TryReadState();
            if (state?.Release is { } cached
                && cached.Assets is { Length: > 0 }
                && string.Equals(cached.TagName, tag, StringComparison.OrdinalIgnoreCase))
            {
                // 没发新版：缓存的 release（含发布说明）直接做版本比较，资产与说明请求都省掉
                if (!string.IsNullOrWhiteSpace(cached.Body))
                {
                    return EvaluateRelease(cached, current);
                }

                // 旧版缓存只有资产、没有发布说明：只补一次说明，资产继续用缓存
                cached.Body = await FetchReleaseNotesAsync(tag, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(cached.Body))
                {
                    TrySaveState(new UpdateCheckState(Etag: null, cached));
                }

                return EvaluateRelease(cached, current);
            }

            GitHubRelease? release = await FetchWebReleaseAsync(tag, cancellationToken).ConfigureAwait(false);
            release ??= new GitHubRelease
            {
                TagName = tag,
                HtmlUrl = $"{_webBase}/{_owner}/{_repo}/releases/tag/{Uri.EscapeDataString(tag)}"
            };

            // 资产端点不可用时保留发布页入口（不转去消耗 API 配额），但发布说明另有网页来源，仍要尝试
            if (string.IsNullOrWhiteSpace(release.Body))
            {
                release.Body = await FetchReleaseNotesAsync(tag, cancellationToken).ConfigureAwait(false);
            }

            if (release.Assets is { Length: > 0 })
            {
                TrySaveState(new UpdateCheckState(Etag: null, release));
            }

            return EvaluateRelease(release, current);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient 超时也抛 TaskCanceledException：按失败展示，让界面给出提示
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.WebTimedOut));
            return UpdateCheckResult.Failed(global::CodeWF.Toolkit.Core.LibraryText.Get(LibraryResources.Update.WebTimeout));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound && !_allowApiFallback)
        {
            return UpdateCheckResult.Latest();
        }
        catch (Exception ex)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.WebFailed, ex.Message));
            return UpdateCheckResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// 发布说明的网页来源：releases.atom 每个版本一个 entry，正文就在 content 里，
    /// 与资产列表一样走网页端点，不占 GitHub API 配额；说明只作展示，失败不影响更新检查本身。
    /// </summary>
    private async Task<string?> FetchReleaseNotesAsync(string tag, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_webBase}/{_owner}/{_repo}/releases.atom");
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            using HttpResponseMessage response = await _http
                .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.NotesHttpFailed, (int)response.StatusCode));
                return null;
            }

            string feed = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            string? notes = ParseReleaseNotes(feed, tag);
            if (string.IsNullOrWhiteSpace(notes))
            {
                _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.NotesEmpty, tag));
            }

            return notes;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.NotesFailed, ex.Message));
            return null;
        }
    }

    /// <summary>
    /// 从 releases.atom 抠出指定 tag 的正文：定位该 tag 的入口块（到下一个 entry 为止），
    /// 解码 content 里的实体并去掉 HTML 标签。保持字符串扫描，不引 HTML/XML 解析器。
    /// </summary>
    private static string? ParseReleaseNotes(string feed, string tag)
    {
        const string entryMarker = "<entry>";
        const string linkMarker = "/releases/tag/";
        string target = Uri.EscapeDataString(tag);
        int index = 0;
        while ((index = feed.IndexOf(entryMarker, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            int end = feed.IndexOf("</entry>", index, StringComparison.OrdinalIgnoreCase);
            if (end < 0)
            {
                end = feed.Length;
            }

            string entry = feed[index..end];
            index = end + 1;
            int link = entry.IndexOf(linkMarker, StringComparison.Ordinal);
            int tagStart = link + linkMarker.Length;
            int tagEnd = tagStart + target.Length;
            if (link < 0 || !entry.AsSpan(tagStart).StartsWith(target, StringComparison.OrdinalIgnoreCase)
                || (tagEnd < entry.Length && entry[tagEnd] is not ('"' or '\'' or '<' or '/' or '?' or '#')))
            {
                continue;
            }

            int content = entry.IndexOf("<content", StringComparison.OrdinalIgnoreCase);
            int open = content < 0 ? -1 : entry.IndexOf('>', content);
            int close = entry.IndexOf("</content>", StringComparison.OrdinalIgnoreCase);
            if (open < 0 || close <= open)
            {
                return null;
            }

            return ToPlainText(entry[(open + 1)..close]);
        }

        return null;
    }

    /// <summary>atom 的 content 是转义后的 HTML：先还原实体，再把块级标签换成换行、其余标签去掉。</summary>
    private static string ToPlainText(string html)
    {
        string decoded = WebUtility.HtmlDecode(html ?? string.Empty);
        if (decoded.Length == 0)
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder(decoded.Length);
        int index = 0;
        while (index < decoded.Length)
        {
            int open = decoded.IndexOf('<', index);
            if (open < 0)
            {
                builder.Append(decoded, index, decoded.Length - index);
                break;
            }

            builder.Append(decoded, index, open - index);
            int close = decoded.IndexOf('>', open);
            if (close < 0)
            {
                break;
            }

            string raw = decoded[(open + 1)..close].Trim();
            bool closing = raw.StartsWith("/", StringComparison.Ordinal);
            string name = (closing ? raw[1..] : raw).TrimEnd('/').Trim().ToLowerInvariant();
            if (name == "li")
            {
                // 列表项没有符号，补一个「- 」，读起来与发布页一致
                builder.Append(closing ? '\n' : "\n- ");
            }
            else if (name is "br" or "hr" or "p" or "ul" or "ol" or "pre" or "blockquote"
                or "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
            {
                builder.Append('\n');
            }

            index = close + 1;
        }

        var lines = new List<string>();
        foreach (string line in builder.ToString().Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.Length > 0)
            {
                lines.Add(trimmed);
            }
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    /// <summary>releases/latest 的 302 落点就是 /releases/tag/{tag}；只取响应头，不下载页面。</summary>
    private async Task<string?> FetchLatestTagViaRedirectAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_webBase}/{_owner}/{_repo}/releases/latest");
        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        using HttpResponseMessage response = await _http
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!IsRedirect(response.StatusCode))
        {
            response.EnsureSuccessStatusCode();
        }

        // HttpClient 自动跟随重定向时最终地址写在 RequestMessage 上；跟随被禁用时读 Location 头
        Uri? finalUri = IsRedirect(response.StatusCode)
            ? response.Headers.Location
            : response.RequestMessage?.RequestUri;
        string? tag = ExtractTag(finalUri);
        if (tag is null)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.TagMissing, (int)response.StatusCode));
        }

        return tag;
    }

    /// <summary>expanded_assets 是发布页懒加载资产列表的接口；从中拼出 release 模型。</summary>
    private async Task<GitHubRelease?> FetchWebReleaseAsync(string tag, CancellationToken cancellationToken)
    {
        try
        {
            return await FetchWebAssetsAsync(tag, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.AssetsUnavailable, ex.Message));
            return null;
        }
    }

    private async Task<GitHubRelease?> FetchWebAssetsAsync(string tag, CancellationToken cancellationToken)
    {
        string escapedTag = Uri.EscapeDataString(tag);
        string url = $"{_webBase}/{_owner}/{_repo}/releases/expanded_assets/{escapedTag}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.AssetsHttpFailed, (int)response.StatusCode));
            return null;
        }

        string html = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        List<GitHubAsset> assets = ParseDownloadAssets(html, tag, _webBase);
        return new GitHubRelease
        {
            TagName = tag,
            HtmlUrl = $"{_webBase}/{_owner}/{_repo}/releases/tag/{escapedTag}",
            Assets = assets.ToArray()
        };
    }

    /// <summary>回退：Releases API。ETag 条件请求 304 不计配额；403/429 按指示退避。</summary>
    private async Task<UpdateCheckResult> CheckViaApiAsync(Version current, CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow < _blockedUntilUtc)
        {
            return UpdateCheckResult.Failed(RateLimitMessage);
        }

        string url = $"{_apiBase}/repos/{_owner}/{_repo}/releases/latest";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        UpdateCheckState? state = TryReadState();
        if (!string.IsNullOrEmpty(state?.Etag))
        {
            request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(state.Etag));
        }

        using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            // 最新发布没变：条件请求不计配额，用缓存的 release 内容做版本比较
            return state?.Release is { } cached
                ? EvaluateRelease(cached, current)
                : UpdateCheckResult.Latest();
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return UpdateCheckResult.Latest();
        }

        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
        {
            _blockedUntilUtc = ParseBlockedUntil(response);
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.ApiRateLimited, (int)response.StatusCode));
            return UpdateCheckResult.Failed(RateLimitMessage);
        }

        if (!response.IsSuccessStatusCode)
        {
            _log?.Invoke(global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.CheckHttpFailed, (int)response.StatusCode));
            return UpdateCheckResult.Failed($"HTTP {(int)response.StatusCode}");
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        GitHubRelease? release = JsonSerializer.Deserialize(json, UpdateJsonContext.Default.GitHubRelease);
        if (release is null || release.Draft || release.Prerelease)
        {
            return UpdateCheckResult.Latest();
        }

        TrySaveState(new UpdateCheckState(response.Headers.ETag?.Tag, release));
        return EvaluateRelease(release, current);
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.Moved
            or HttpStatusCode.Found
            or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect
            or HttpStatusCode.PermanentRedirect;

    /// <summary>从 …/releases/tag/{tag} 的落点地址抠出 tag；跟随与未跟随两种响应都要顾及。</summary>
    private static string? ExtractTag(Uri? uri)
    {
        if (uri is null)
        {
            return null;
        }

        const string marker = "/releases/tag/";
        string path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;
        int start = path.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        string tag = Uri.UnescapeDataString(path[(start + marker.Length)..]).TrimEnd('/');
        return tag.Length > 0 ? tag : null;
    }

    /// <summary>从 expanded_assets 的 HTML 里抠出 /releases/download/{tag}/{文件名} 资产；
    /// 简单字符串扫描而不引 HTML 解析器，保持 AOT 友好。</summary>
    private static List<GitHubAsset> ParseDownloadAssets(string html, string tag, string webBase)
    {
        var assets = new List<GitHubAsset>();
        const string marker = "/releases/download/";
        int index = 0;
        while ((index = html.IndexOf("href=\"", index, StringComparison.Ordinal)) >= 0)
        {
            int start = index + "href=\"".Length;
            int end = html.IndexOf('"', start);
            if (end < 0)
            {
                break;
            }

            index = end + 1;
            string href = html[start..end];
            int pathStart = href.IndexOf(marker, StringComparison.Ordinal);
            if (pathStart < 0)
            {
                continue;
            }

            string remainder = href[(pathStart + marker.Length)..]; // {tag}/{文件名}
            int separator = remainder.IndexOf('/');
            if (separator <= 0)
            {
                continue;
            }

            string hrefTag = Uri.UnescapeDataString(remainder[..separator]);
            string name = Uri.UnescapeDataString(remainder[(separator + 1)..]);
            if (name.Length == 0 || !string.Equals(hrefTag, tag, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 同一资产在页面里可能以根相对与绝对两种形式各出现一次
            if (assets.Any(a => string.Equals(a.Name, name, StringComparison.Ordinal)))
            {
                continue;
            }

            bool absolute = href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            assets.Add(new GitHubAsset
            {
                Name = name,
                BrowserDownloadUrl = absolute ? href : webBase + href
            });
        }

        return assets;
    }

    private UpdateCheckResult EvaluateRelease(GitHubRelease release, Version current)
    {
        Version? candidate = VersionUtil.Parse(release.TagName);
        if (!VersionUtil.IsNewer(candidate, current))
        {
            return UpdateCheckResult.Latest();
        }

        UpdateAsset? asset = PickAsset(release, _runtimeIdentifier ?? UpdateServiceOptions.CurrentRuntimeIdentifier(), _preferInstaller);
        string title = string.IsNullOrWhiteSpace(release.Name) ? release.TagName ?? string.Empty : release.Name!;

        return new UpdateCheckResult(new UpdateInfo(
            candidate!,
            release.TagName ?? string.Empty,
            title,
            release.Body,
            release.HtmlUrl ?? $"{_webBase}/{_owner}/{_repo}/releases",
            asset?.BrowserDownloadUrl,
            asset?.Name,
            asset?.ChecksumUrl,
            asset?.Size), true, null);
    }

    private string RateLimitMessage
        => global::CodeWF.Toolkit.Core.LibraryText.Format(LibraryResources.Update.RateLimitedUntil, _blockedUntilUtc.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture));

    /// <summary>从 Retry-After 或 X-RateLimit-Reset 推算限流恢复时间，兜底 10 分钟，最长封顶 1 小时。</summary>
    private static DateTime ParseBlockedUntil(HttpResponseMessage response)
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan? retryAfter = response.Headers.RetryAfter?.Delta;
        if (retryAfter is null && response.Headers.RetryAfter?.Date is { } retryDate)
        {
            retryAfter = retryDate - DateTimeOffset.UtcNow;
        }

        DateTime blocked = retryAfter is { } delay && delay > TimeSpan.Zero
            ? now + delay
            : response.Headers.TryGetValues("X-RateLimit-Reset", out var values)
              && long.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long resetSeconds)
                ? DateTimeOffset.FromUnixTimeSeconds(resetSeconds).UtcDateTime
                : now + TimeSpan.FromMinutes(10);

        blocked += TimeSpan.FromSeconds(30);
        return blocked > now + TimeSpan.FromHours(1) ? now + TimeSpan.FromHours(1) : blocked;
    }

    private UpdateCheckState? TryReadState()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_stateFile) || !File.Exists(_stateFile))
            {
                return null;
            }

            string json = File.ReadAllText(_stateFile);
            return JsonSerializer.Deserialize(json, UpdateJsonContext.Default.UpdateCheckState);
        }
        catch
        {
            // 缓存读不出来就当作没有，走普通请求
            return null;
        }
    }

    private void TrySaveState(UpdateCheckState state)
    {
        if (string.IsNullOrWhiteSpace(_stateFile))
        {
            return;
        }

        try
        {
            string? dir = Path.GetDirectoryName(_stateFile);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_stateFile, JsonSerializer.Serialize(state, UpdateJsonContext.Default.UpdateCheckState));
        }
        catch
        {
            // 缓存写失败不影响本次检查结果
        }
    }

    /// <summary>
    /// 选择当前系统/架构的可下载资产：Windows 的 <c>-setup.exe</c> → Linux 的 <c>-linux-{arch}.deb</c>
    /// → macOS 的 <c>-osx-{arch}.dmg</c>（旧发布为 pkg）→ 各自的 <c>-{rid}.zip</c> 兜底，
    /// 并配对同名 <c>.sha256</c>。没有匹配包时返回 null，交给界面显示「打开发布页」。
    /// </summary>
    internal static UpdateAsset? PickAsset(GitHubRelease release, string? runtimeIdentifier, bool preferInstaller)
    {
        if (release.Assets is null || release.Assets.Length == 0 || string.IsNullOrWhiteSpace(runtimeIdentifier))
        {
            return null;
        }

        string marker = "-" + runtimeIdentifier;
        GitHubAsset? preferred = null;

        // 安装向导只随 Windows x64 发布；其余平台没有时继续往下找
        if (preferInstaller && string.Equals(runtimeIdentifier, "win-x64", StringComparison.OrdinalIgnoreCase))
        {
            preferred = Find(release, marker, "-setup.exe");
        }

        string nativeExtension = runtimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase)
            ? ".deb"
            : runtimeIdentifier.StartsWith("osx-", StringComparison.OrdinalIgnoreCase)
                ? ".dmg"
                : ".zip";

        preferred ??= Find(release, marker, nativeExtension);

        // macOS 早期发布只有 pkg；Windows/Linux 早期发布只有 zip，都要能升上来
        if (runtimeIdentifier.StartsWith("osx-", StringComparison.OrdinalIgnoreCase))
        {
            preferred ??= Find(release, marker, ".pkg");
        }

        preferred ??= Find(release, marker, ".zip");

        if (preferred is null)
        {
            return null;
        }

        GitHubAsset? checksum = release.Assets.FirstOrDefault(asset =>
            string.Equals(asset.Name, preferred.Name + ".sha256", StringComparison.OrdinalIgnoreCase));
        return new UpdateAsset(
            preferred.Name!,
            preferred.BrowserDownloadUrl!,
            checksum?.BrowserDownloadUrl,
            preferred.Size > 0 ? preferred.Size : null,
            IsInstaller(preferred.Name!));
    }

    private static GitHubAsset? Find(GitHubRelease release, string marker, string extension)
        => release.Assets!.FirstOrDefault(a =>
            IsAssetForRuntime(a, marker) &&
            a.Name!.EndsWith(extension, StringComparison.OrdinalIgnoreCase));

    /// <summary>安装包（而非便携压缩包）：下载完成后可以交给系统安装，而不只是解压使用。</summary>
    private static bool IsInstaller(string assetName)
        => !assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

    private static bool IsAssetForRuntime(GitHubAsset asset, string marker)
    {
        return !string.IsNullOrWhiteSpace(asset.Name) &&
            !string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl) &&
            (asset.Name!.Contains(marker + ".", StringComparison.OrdinalIgnoreCase) ||
             asset.Name.Contains(marker + "-", StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>选定的更新资产。</summary>
internal sealed record UpdateAsset(
    string Name,
    string BrowserDownloadUrl,
    string? ChecksumUrl,
    long? Size,
    bool IsInstaller);

/// <summary>版本号比较。tag 形如 v1.2.3 / 1.2 / 1.2.3.4 都能吃。</summary>
public static class VersionUtil
{
    /// <summary>把 tag 解析成可比较的 Version，失败返回 null。</summary>
    public static Version? Parse(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        var text = tag.Trim();
        if (text.Length > 0 && (text[0] == 'v' || text[0] == 'V'))
        {
            text = text[1..];
        }

        var dash = text.IndexOfAny(new[] { '-', '+' });
        if (dash > 0)
        {
            text = text[..dash];
        }

        return Version.TryParse(text, out var version) ? Normalize(version) : null;
    }

    /// <summary>candidate 是否比 current 新。两边补齐到四段再比。</summary>
    public static bool IsNewer(Version? candidate, Version? current)
    {
        if (candidate is null)
        {
            return false;
        }

        return current is null || Normalize(candidate) > Normalize(current);
    }

    private static Version Normalize(Version version)
        => new(
            Math.Max(version.Major, 0),
            Math.Max(version.Minor, 0),
            Math.Max(version.Build, 0),
            Math.Max(version.Revision, 0));
}
