using System.Globalization;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Toolkit.Tests.Core;

[Collection("Localization")]
public sealed class UpdateLocalizationTests : IDisposable
{
    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo? _defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
    private readonly CultureInfo? _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

    [Theory]
    [InlineData("en-US", "The update has no downloadable asset.", "The installer no longer exists. Download it again.")]
    [InlineData("zh-CN", "更新没有可下载的资产。", "安装包不存在或已被删除，请重新下载。")]
    [InlineData("fr-FR", "The update has no downloadable asset.", "The installer no longer exists. Download it again.")]
    public async Task UpdateServiceUsesEmbeddedDefaultsWithoutApplicationResources(string culture, string downloadMessage, string installMessage)
    {
        I18nManager.Instance.Register(new JsonLangPlugin { ScanResourceFolder = false }, CultureInfo.GetCultureInfo(culture));
        using var updates = new UpdateService(new UpdateServiceOptions { Repository = "TestApp" });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => updates.DownloadAsync(
            MissingAsset, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(downloadMessage, error.Message);
        var install = await updates.InstallAsync(new UpdateDownloadResult(string.Empty, "missing.exe", 0), TestContext.Current.CancellationToken);
        Assert.Equal(installMessage, install.Message);
        Assert.False(install.ShouldExitApplication);
    }

    [Fact]
    public async Task ApplicationCanOverrideOneEmbeddedKeyWhileOthersKeepLibraryDefaults()
    {
        var plugin = new JsonLangPlugin { ScanResourceFolder = false };
        plugin.AddResource(typeof(UpdateLocalizationTests).Assembly);
        I18nManager.Instance.Register(plugin, CultureInfo.GetCultureInfo("en-US"));
        using var updates = new UpdateService(new UpdateServiceOptions { Repository = "TestApp" });

        var overridden = await Assert.ThrowsAsync<InvalidOperationException>(() => updates.DownloadAsync(
            MissingAsset, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("Custom application download message.", overridden.Message);
        var fallback = await Assert.ThrowsAsync<InvalidOperationException>(() => updates.DownloadAsync(
            MissingAsset with { AssetUrl = "https://example.test/app.exe" }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("The update asset has no SHA-256 checksum file.", fallback.Message);

        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        var switched = await Assert.ThrowsAsync<InvalidOperationException>(() => updates.DownloadAsync(
            MissingAsset with { AssetUrl = "https://example.test/app.exe" }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("更新资产缺少 SHA-256 校验文件。", switched.Message);
    }

    private static UpdateInfo MissingAsset => new(new Version(2, 0), "v2.0", "Test release", null, "https://example.test/release", null, null);

    public void Dispose()
    {
        I18nManager.Instance.Register(new JsonLangPlugin { ScanResourceFolder = false }, _uiCulture);
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _uiCulture;
        CultureInfo.DefaultThreadCurrentCulture = _defaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
    }
}
