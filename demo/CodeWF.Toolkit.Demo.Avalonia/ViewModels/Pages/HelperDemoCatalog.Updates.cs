using System.Net;
using System.Security.Cryptography;
using System.Text;
using CodeWF.Toolkit.Core.UpdateChecking;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

public static partial class HelperDemoCatalog
{
    private static async Task<string> UpdatePipeline(string text)
    {
        var directory = TemporaryDirectory();
        try
        {
            var payload = Encoding.UTF8.GetBytes(text);
            using var http = new HttpClient(new UpdateFixtureHandler(payload));
            using var updates = new UpdateService(http, new UpdateServiceOptions
            {
                Repository = "DemoApp", RuntimeIdentifier = "win-x64", WebBase = "https://example.test",
                StateFile = Path.Combine(directory, "state.json"), DownloadDirectory = directory
            });
            var check = await updates.CheckAsync(new Version(1, 0));
            if (!check.Succeeded || check.Update is null) throw new InvalidOperationException(check.Error);
            var download = await updates.DownloadAsync(check.Update);
            var verified = File.ReadAllBytes(download.FilePath).SequenceEqual(payload);
            if (!verified) throw new InvalidDataException("Download round trip failed.");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var canceled = await updates.CheckAsync(cancellationToken: cancellation.Token);
            if (canceled.Succeeded) throw new InvalidOperationException("Canceled check reported success.");
            // 通过缺失文件验证安装保护，不启动安装器或打开目录。
            File.Delete(download.FilePath);
            var install = await updates.InstallAsync(download);
            if (install.Kind != UpdateInstallKind.Unsupported || install.ShouldExitApplication)
                throw new InvalidOperationException("Missing-file install guard failed.");
            return $"Tag = {check.Update.Tag}\nAsset = {check.Update.AssetName}\nBytes = {download.BytesReceived}\nSHA256 = {Convert.ToHexString(SHA256.HashData(payload))}\nVerified = {verified}\nCancellation = {!canceled.Succeeded}\nInstallKind = {install.Kind}\n{install.Message}";
        }
        finally { Directory.Delete(directory, true); }
    }

    private sealed class UpdateFixtureHandler(byte[] payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = request.RequestUri!.AbsolutePath;
            var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
            if (path.EndsWith("/latest", StringComparison.Ordinal))
            {
                response.StatusCode = HttpStatusCode.Found;
                response.Headers.Location = new Uri("https://example.test/dotnet9/DemoApp/releases/tag/v2.0.0");
            }
            else if (path.Contains("/expanded_assets/", StringComparison.Ordinal))
                response.Content = new StringContent("""
                    <a href="/dotnet9/DemoApp/releases/download/v2.0.0/DemoApp-v2.0.0-win-x64.zip">Package</a>
                    <a href="/dotnet9/DemoApp/releases/download/v2.0.0/DemoApp-v2.0.0-win-x64.zip.sha256">SHA-256</a>
                    """);
            else if (path.EndsWith(".atom", StringComparison.Ordinal))
                response.Content = new StringContent("""
                    <feed><entry><link href="https://example.test/dotnet9/DemoApp/releases/tag/v2.0.0"/>
                    <content type="html">&lt;p&gt;Demo release&lt;/p&gt;</content></entry></feed>
                    """);
            else if (path.EndsWith(".sha256", StringComparison.Ordinal))
                response.Content = new StringContent(Convert.ToHexString(SHA256.HashData(payload)));
            else if (path.EndsWith(".zip", StringComparison.Ordinal)) response.Content = new ByteArrayContent(payload);
            else throw new HttpRequestException("Unexpected fixture URL: " + request.RequestUri);
            return Task.FromResult(response);
        }
    }
}
