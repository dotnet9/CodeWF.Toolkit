using System.Globalization;
using System.Xml.Linq;
using CodeWF.Toolkit.Files.Helpers;

namespace CodeWF.Toolkit.Tests.Files;

public sealed class AppConfigHelperTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "CodeWF.AppConfig", Guid.NewGuid().ToString("N"));
    private string ConfigPath => Path.Combine(_directory, "settings.config");

    [Fact]
    public void SnapshotReadsTypedValuesWithoutReopeningFile()
    {
        AppConfigHelper.SetMany(ConfigPath, new Dictionary<string, object?>
        {
            ["enabled"] = true,
            ["width"] = 123.5,
            ["count"] = 7,
            ["mode"] = DayOfWeek.Friday,
            ["text"] = "A & B",
            ["empty"] = null
        });
        var snapshot = AppConfigHelper.ReadAll(ConfigPath);
        File.Delete(ConfigPath);

        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.True(snapshot.TryGet<bool?>("enabled", out var enabled));
            Assert.True(enabled);
            Assert.True(snapshot.TryGet<double?>("width", out var width));
            Assert.Equal(123.5, width);
            Assert.True(snapshot.TryGet<int>("count", out var count));
            Assert.Equal(7, count);
            Assert.True(snapshot.TryGet<DayOfWeek?>("mode", out var mode));
            Assert.Equal(DayOfWeek.Friday, mode);
            Assert.True(snapshot.TryGet<string>("text", out var text));
            Assert.Equal("A & B", text);
            Assert.True(snapshot.TryGet<string>("empty", out var empty));
            Assert.Equal(string.Empty, empty);
            Assert.False(snapshot.TryGet<int>("text", out _));
            Assert.False(snapshot.TryGet<string>("missing", out _));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void BatchUpdatePreservesUnrelatedKeysSectionsAndComments()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(ConfigPath, """
            <configuration>
              <!-- Keep this section -->
              <appSettings><add key="mode" value="system" /><add key="external" value="keep" /></appSettings>
              <custom enabled="true"><child value="keep" /></custom>
            </configuration>
            """);

        AppConfigHelper.SetMany(ConfigPath, new Dictionary<string, object?>
        {
            ["mode"] = "dark",
            ["width"] = 123.5
        });
        AppConfigHelper.Set(ConfigPath, "external", "changed externally");
        AppConfigHelper.SetMany(ConfigPath, new Dictionary<string, object?> { ["width"] = 456.75 });

        var snapshot = AppConfigHelper.ReadAll(ConfigPath);
        Assert.Equal("dark", snapshot["mode"]);
        Assert.Equal("changed externally", snapshot["external"]);
        Assert.Equal("456.75", snapshot["width"]);
        var document = XDocument.Load(ConfigPath);
        Assert.Equal("keep", document.Root!.Element("custom")!.Element("child")!.Attribute("value")!.Value);
        Assert.Contains(document.DescendantNodes().OfType<XComment>(), comment => comment.Value.Contains("Keep this section", StringComparison.Ordinal));
        Assert.Single(document.Root.Element("appSettings")!.Elements("add"), item => item.Attribute("key")?.Value == "mode");
    }

    [Fact]
    public void SnapshotMatchesSingleReadForDuplicateAndMissingValues()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(ConfigPath, """
            <configuration><appSettings>
              <add key="mode" value="first" /><add key="mode" value="second" />
              <add key="missing" /><add key="missing" value="ignored" />
              <add key="Mode" value="case sensitive" />
            </appSettings></configuration>
            """);
        var snapshot = AppConfigHelper.ReadAll(ConfigPath);
        Assert.True(snapshot.TryGet<string>("mode", out var mode));
        Assert.Equal("first", mode);
        Assert.False(snapshot.TryGet<string>("missing", out _));
        Assert.Equal("case sensitive", snapshot["Mode"]);
    }

    [Fact]
    public void MissingOrMalformedFileReturnsEmptySnapshotAndEmptyBatchDoesNotCreateFile()
    {
        Assert.Empty(AppConfigHelper.ReadAll(ConfigPath));
        AppConfigHelper.SetMany(ConfigPath, new Dictionary<string, object?>());
        Assert.False(File.Exists(ConfigPath));
        Directory.CreateDirectory(_directory);
        File.WriteAllText(ConfigPath, "<configuration>");
        Assert.Empty(AppConfigHelper.ReadAll(ConfigPath));
        Assert.Equal("<configuration>", File.ReadAllText(ConfigPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
