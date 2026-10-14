using System.Net;
using System.Security.Cryptography;
using CodeWF.Toolkit.Core.UpdateChecking;

namespace CodeWF.Toolkit.Tests.Core;

public sealed class UpdateDownloaderTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "CodeWF-UpdateTests-" + Guid.NewGuid().ToString("N"));
    private readonly byte[] _payload = Enumerable.Range(0, 180_000).Select(i => (byte)i).ToArray();
    private UpdateServiceOptions Options => new() { Repository = "TestApp", DownloadDirectory = _directory };
    private static UpdateInfo Update => new(new Version(2, 0), "v2.0", "Test release", null,
        "https://example.test/release", "https://example.test/package", "TestApp.zip", "https://example.test/package.sha256");

    [Fact]
    public async Task VerifiedDownloadReplacesPreviousFileAndReportsProgress()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(Path.Combine(_directory, "TestApp.zip"), "previous", TestContext.Current.CancellationToken);
        using var http = Client();
        var reports = new List<UpdateDownloadProgress>();
        var downloader = new UpdateDownloader(http, Options);
        var result = await downloader.DownloadAsync(Update, new InlineProgress(reports.Add), TestContext.Current.CancellationToken);
        Assert.Equal(_payload, await File.ReadAllBytesAsync(result.FilePath, TestContext.Current.CancellationToken));
        Assert.Equal(_payload.Length, result.BytesReceived);
        Assert.Equal(100, reports[^1].Percentage);
        Assert.True(reports.Count >= 2);
        Assert.False(File.Exists(result.FilePath + ".download"));
    }

    [Theory]
    [InlineData("mismatch")]
    [InlineData("malformed")]
    [InlineData("http-error")]
    [InlineData("canceled")]
    public async Task VerificationFailureCleansPartialAndPreservesPreviousFile(string failure)
    {
        Directory.CreateDirectory(_directory);
        var final = Path.Combine(_directory, "TestApp.zip");
        await File.WriteAllTextAsync(final, "previous", TestContext.Current.CancellationToken);
        using var http = Client(failure);
        await Assert.ThrowsAnyAsync<Exception>(() => new UpdateDownloader(http, Options).DownloadAsync(
            Update, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("previous", await File.ReadAllTextAsync(final, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(final + ".download"));
    }

    [Theory]
    [InlineData(null, "https://example.test/check", "App.zip")]
    [InlineData("http://example.test/app", "https://example.test/check", "App.zip")]
    [InlineData("https://example.test/app", null, "App.zip")]
    [InlineData("https://example.test/app", "http://example.test/check", "App.zip")]
    [InlineData("https://example.test/app", "https://example.test/check", "..")]
    [InlineData("https://example.test/app", "https://example.test/check", "")]
    public async Task InvalidInputsAreRejectedBeforeNetworkAccess(string? asset, string? checksum, string name)
    {
        using var http = new HttpClient(new StubHandler(_ => throw new Xunit.Sdk.XunitException("Network must not be called")));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new UpdateDownloader(http, Options).DownloadAsync(
            Update with { AssetUrl = asset, ChecksumUrl = checksum, AssetName = name }, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CancellationDuringTransferCleansPartialFile()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var http = Client();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new UpdateDownloader(http, Options).DownloadAsync(
            Update, new InlineProgress(_ => cts.Cancel()), cts.Token));
        Assert.False(File.Exists(Path.Combine(_directory, "TestApp.zip.download")));
        Assert.False(File.Exists(Path.Combine(_directory, "TestApp.zip")));
    }

    [Fact]
    public async Task KnownLengthMismatchCannotBecomeAnInstallablePackage()
    {
        using var http = Client();
        // ByteArrayContent provides a reliable Content-Length; a bogus release size must not override it.
        var result = await new UpdateDownloader(http, Options).DownloadAsync(Update with { AssetSize = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(_payload.Length, result.BytesReceived);
    }

    private HttpClient Client(string? failure = null) => new(new StubHandler(request =>
    {
        if (!request.RequestUri!.AbsolutePath.EndsWith(".sha256", StringComparison.Ordinal))
            return new(HttpStatusCode.OK) { Content = new ByteArrayContent(_payload) };
        if (failure == "canceled") throw new OperationCanceledException();
        var hash = failure == "malformed" ? "invalid" : failure == "mismatch" ? new string('0', 64) : Convert.ToHexString(SHA256.HashData(_payload));
        return new(failure == "http-error" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)
            { Content = new StringContent(hash + "  TestApp.zip") };
    }));

    public void Dispose() { if (Directory.Exists(_directory)) Directory.Delete(_directory, true); }
    private sealed class InlineProgress(Action<UpdateDownloadProgress> report) : IProgress<UpdateDownloadProgress>
    { public void Report(UpdateDownloadProgress value) => report(value); }
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}
