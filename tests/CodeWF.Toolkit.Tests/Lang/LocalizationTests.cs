using System.Globalization;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using CodeWF.Toolkit.Lang.Resx;
using CodeWF.Toolkit.Lang.Xml;
using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.UpdateChecking;

namespace CodeWF.Toolkit.Tests.Lang;

[CollectionDefinition("Localization", DisableParallelization = true)]
public sealed class LocalizationCollection;

[Collection("Localization")]
public sealed class LocalizationTests : IDisposable
{
    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo? _defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
    private readonly CultureInfo? _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

    public LocalizationTests() => I18nManager.Instance.Register(EmptyPlugin(), CultureInfo.GetCultureInfo("en-US"));

    [Fact]
    public void AssemblyHasNoAvaloniaOrThirdPartyRuntimeDependency()
    {
        var dependencies = typeof(I18nManager).Assembly.GetReferencedAssemblies();
        Assert.All(dependencies, dependency => Assert.True(
            dependency.Name == "System.Private.CoreLib" || dependency.Name!.StartsWith("System.", StringComparison.Ordinal),
            $"Unexpected dependency: {dependency.Name}"));
    }

    [Theory]
    [InlineData("zh-CN", "你好 Ada")]
    [InlineData("zh-Hant-TW", "Traditional Ada")]
    [InlineData("fr-FR", "Hello Ada")]
    public void EmbeddedResourcesSupportExplicitCultureParentAndDefaultFallback(string culture, string expected)
    {
        var localizer = Localizer();
        Assert.Equal(expected, string.Format(localizer.GetResource("Library.Greeting", culture), "Ada"));
        Assert.Equal("Default", localizer.GetResource("Library.Fallback", culture));
        Assert.Equal(string.Empty, localizer.GetResource("Library.Empty", culture));
        Assert.Equal("Missing.Key", localizer.GetResource("Missing.Key", culture));
    }

    [Fact]
    public void LibraryFollowsRegisteredApplicationCultureAndAllowsOverrides()
    {
        var plugin = EmptyPlugin();
        I18nManager.Instance.Register(plugin, CultureInfo.GetCultureInfo("en-US"));
        var localizer = Localizer();
        var changes = 0;
        System.ComponentModel.PropertyChangedEventHandler handler = (_, e) => { if (e.PropertyName == "Culture") changes++; };
        I18nManager.Instance.PropertyChanged += handler;
        try
        {
            I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
            Assert.Equal("你好 Ada", localizer.Format("Library.Greeting", "Ada"));
            Assert.Equal("zh-CN", CultureInfo.CurrentUICulture.Name);
            Assert.Equal(1, changes);
            plugin.Resources["zh-CN"] = new LocalizationLanguage { CultureName = "zh-CN" };
            plugin.Resources["zh-CN"].Languages["Library.Greeting"] = "App {0}";
            Assert.Equal("App Ada", localizer.Format("Library.Greeting", "Ada"));
        }
        finally { I18nManager.Instance.PropertyChanged -= handler; }
    }

    [Fact]
    public void GenericLoaderPreservesCustomMetadataAndReturnsIndependentSnapshots()
    {
        var plugin = new JsonLangPlugin<CustomLanguage> { ScanResourceFolder = false };
        plugin.AddResource(typeof(LocalizationTests).Assembly);
        plugin.Load(CultureInfo.GetCultureInfo("en-US"));
        CustomLanguage language = plugin.Resources["en-US"];
        var snapshot = plugin.GetLanguages()!.Single(item => item.CultureName == "en-US");
        snapshot.Languages["Library.Fallback"] = "Mutated";
        Assert.Equal("Default", language.Languages["Library.Fallback"]);
        Assert.IsType<CustomLanguage>(((ILangPlugin)plugin).GetLanguages()![0]);
    }

    [Fact]
    public void ResourceAdditionNotifiesAndFailedRegistrationPreservesPreviousPlugin()
    {
        I18nManager.Instance.Register(EmptyPlugin(), CultureInfo.GetCultureInfo("en-US"));
        var version = I18nManager.Instance.ResourceVersion;
        var changes = 0;
        EventHandler<EventArgs> handler = (_, _) => changes++;
        I18nManager.Instance.ResourcesChanged += handler;
        try
        {
            I18nManager.Instance.AddResource(typeof(LocalizationTests).Assembly);
            Assert.Equal(version + 1, I18nManager.Instance.ResourceVersion);
            Assert.Equal(1, changes);
            Assert.False(I18nManager.Instance.Register(new FailingPlugin(), CultureInfo.GetCultureInfo("fr-FR"), out var error));
            Assert.Contains("Test load failure", error);
            Assert.Equal("en-US", I18nManager.Instance.Culture!.Name);
            Assert.Equal("Default", I18nManager.Instance.GetResource("Library.Fallback"));
        }
        finally { I18nManager.Instance.ResourcesChanged -= handler; }
    }

    [Fact]
    public void XmlAndResxUseTheSameTypedMetadataContract()
    {
        Assert.IsAssignableFrom<ILangPlugin>(new XmlLangPlugin<CustomLanguage>());
        var plugin = new ResxLangPlugin<CustomLanguage>();
        plugin.Resources["en-US"] = new CustomLanguage { CultureName = "en-US" };
        plugin.Resources["en-US"].Languages["Empty"] = string.Empty;
        Assert.Equal(string.Empty, plugin.GetResource("Empty", "en-US"));
    }

    [Fact]
    public void ToolkitErrorsFollowLanguageWhileChineseNumeralAlgorithmsRemainInvariant()
    {
        var english = Assert.Throws<InvalidOperationException>(() => new UpdateServiceOptions().Validate()).Message;
        Assert.Equal("Specify a GitHub repository (UpdateServiceOptions.Repository).", english);
        var numeral = 123.45m.ToChineseNumber();
        var money = NumberExtension.ToChineseMoney(123.45m);
        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        var chinese = Assert.Throws<InvalidOperationException>(() => new UpdateServiceOptions().Validate()).Message;
        Assert.NotEqual(english, chinese);
        Assert.DoesNotContain("Toolkit.", chinese);
        Assert.Equal(numeral, 123.45m.ToChineseNumber());
        Assert.Equal(money, NumberExtension.ToChineseMoney(123.45m));
        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("fr-FR");
        Assert.Equal(numeral, 123.45m.ToChineseNumber());
        Assert.Equal(money, NumberExtension.ToChineseMoney(123.45m));
    }

    private static JsonLangPlugin EmptyPlugin() => new() { ScanResourceFolder = false };
    private static JsonLocalizer Localizer() => new(typeof(LocalizationTests).Assembly);

    public void Dispose()
    {
        I18nManager.Instance.Register(EmptyPlugin(), _uiCulture);
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _uiCulture;
        CultureInfo.DefaultThreadCurrentCulture = _defaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
    }

    public sealed class CustomLanguage : LocalizationLanguage;
    private sealed class FailingPlugin : JsonLangPlugin, ILangPlugin
    {
        void ILangPlugin.Load(CultureInfo cultureInfo) => throw new InvalidOperationException("Test load failure");
    }
}
