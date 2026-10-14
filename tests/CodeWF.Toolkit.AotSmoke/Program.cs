using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;

// Controlled network fixtures exercise the real update pipeline without contacting a release server or installing anything.
var temporaryDirectory = Path.Combine(Path.GetTempPath(), "CodeWF-AotSmoke-" + Guid.NewGuid().ToString("N"));
try
{
    var plugin = new JsonLangPlugin { ScanResourceFolder = false };
    I18nManager.Instance.Register(plugin, CultureInfo.GetCultureInfo("en-US"));
    var english = MissingRepositoryMessage();
    I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
    var chinese = MissingRepositoryMessage();
    Require(english != chinese && !chinese.Contains("Toolkit.", StringComparison.Ordinal), "Embedded localization did not switch.");

    using var http = new HttpClient(new FixtureHandler());
    using var updates = new UpdateService(http, new UpdateServiceOptions
    {
        Repository = "FixtureApp", RuntimeIdentifier = "linux-x64", WebBase = "https://example.test",
        StateFile = Path.Combine(temporaryDirectory, "update-state.json"), DownloadDirectory = temporaryDirectory
    });
    var first = await updates.CheckAsync(new Version(1, 0));
    Require(first.Succeeded && first.Update?.AssetName == "FixtureApp-v2.0.0-linux-x64.deb", "Asset selection failed.");
    Require(first.Update!.Notes == "Fixture release notes", "Release notes parsing failed.");
    Require(File.Exists(updates.Options.StateFile), "AOT cache serialization failed.");
    var cached = await updates.CheckAsync(new Version(1, 0));
    Require(cached.Update?.AssetUrl == first.Update.AssetUrl, "AOT cache deserialization failed.");
    var download = await updates.DownloadAsync(first.Update);
    Require(File.ReadAllBytes(download.FilePath).SequenceEqual(FixtureHandler.Payload), "Verified download failed.");
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();
    var canceled = await updates.CheckAsync(cancellationToken: cancellation.Token);
    Require(!canceled.Succeeded && canceled.Update == null, "Canceled checks must not report a latest version.");
    Console.WriteLine("PASS: AOT localization, release parsing, cache, SHA-256 download and cancellation.");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
finally { if (Directory.Exists(temporaryDirectory)) Directory.Delete(temporaryDirectory, true); }

static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
static string MissingRepositoryMessage()
{
    try { new UpdateServiceOptions().Validate(); }
    catch (InvalidOperationException exception) { return exception.Message; }
    throw new InvalidOperationException("Repository validation did not run.");
}

sealed class FixtureHandler : HttpMessageHandler
{
    internal static readonly byte[] Payload = [1, 2, 3, 4];
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.AbsolutePath;
        var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
        if (path.EndsWith("/latest", StringComparison.Ordinal))
        {
            response.StatusCode = HttpStatusCode.Found;
            response.Headers.Location = new Uri("https://example.test/dotnet9/FixtureApp/releases/tag/v2.0.0");
        }
        else if (path.Contains("/expanded_assets/", StringComparison.Ordinal))
            response.Content = new StringContent("""
                <a href="/dotnet9/FixtureApp/releases/download/v2.0.0/FixtureApp-v2.0.0-linux-x64.deb">Package</a>
                <a href="/dotnet9/FixtureApp/releases/download/v2.0.0/FixtureApp-v2.0.0-linux-x64.deb.sha256">SHA-256</a>
                """);
        else if (path.EndsWith(".atom", StringComparison.Ordinal))
            response.Content = new StringContent("""
                <feed><entry><link href="https://example.test/dotnet9/FixtureApp/releases/tag/v2.0.0"/>
                <content type="html">&lt;p&gt;Fixture release notes&lt;/p&gt;</content></entry></feed>
                """);
        else if (path.EndsWith(".sha256", StringComparison.Ordinal))
            response.Content = new StringContent(Convert.ToHexString(SHA256.HashData(Payload)));
        else if (path.EndsWith(".deb", StringComparison.Ordinal)) response.Content = new ByteArrayContent(Payload);
        else throw new HttpRequestException("Unexpected fixture URL: " + request.RequestUri);
        return Task.FromResult(response);
    }
}
