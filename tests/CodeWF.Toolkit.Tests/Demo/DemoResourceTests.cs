using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace CodeWF.Toolkit.Tests.Demo;

public sealed class DemoResourceTests
{
    private static string DemoDirectory
    {
        get
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "CodeWF.Toolkit.slnx")))
                    return Path.Combine(directory.FullName, "demo", "CodeWF.Toolkit.Demo.Avalonia");
            throw new DirectoryNotFoundException("Repository root not found");
        }
    }

    [Fact]
    public void ChineseAndEnglishHaveTheSameKeysAndAllReferencedResources()
    {
        var folder = DemoDirectory;
        var chinese = Read(Path.Combine(folder, "I18n", "CodeWF.Toolkit.Demo.zh-CN.json"));
        var english = Read(Path.Combine(folder, "I18n", "CodeWF.Toolkit.Demo.en-US.json"));
        Assert.Equal(chinese.Keys.Order().ToArray(), english.Keys.Order().ToArray());
        Assert.DoesNotContain(chinese.Keys, key => key.StartsWith("Demo.Text.", StringComparison.Ordinal));
        foreach (var resource in english.Where(resource => resource.Key != "Demo.Shell.LanguageChinese"))
            Assert.False(Regex.IsMatch(resource.Value, "[\\u4e00-\\u9fff]"), "Untranslated English resource: " + resource.Key);
        foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                     .Where(file => file.EndsWith(".cs") || file.EndsWith(".axaml")))
        {
            var content = File.ReadAllText(file);
            foreach (Match match in Regex.Matches(content, "GetResource\\(\\s*\"(Demo\\.[^\"]+)\"|:I18n\\s+(Demo\\.[^,}\\s]+)|\"@(Demo\\.[^\"]+)\""))
            {
                var key = match.Groups.Cast<Group>().Skip(1).First(group => group.Success).Value;
                Assert.True(chinese.ContainsKey(key), Path.GetFileName(file) + ": missing " + key);
            }
        }
    }


    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void ApiCatalogKeepsNativeAndPrototypeRoutesInSync(string lineEnding)
    {
        var repository = Directory.GetParent(Directory.GetParent(DemoDirectory)!.FullName)!.FullName;
        using var catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(repository, "design", "catalog.json")));
        var generated = File.ReadAllText(Path.Combine(repository, "scripts", "prototype", "assets", "catalog-data.js"))
            .ReplaceLineEndings(lineEnding);
        const string prefix = "window.CodeWFDemoCatalog = ";
        var assignment = generated.IndexOf(prefix, StringComparison.Ordinal);
        Assert.True(assignment >= 0, "Prototype catalog assignment is missing.");
        var start = assignment + prefix.Length;
        var end = generated.IndexOf("window.CodeWFCatalogText", start, StringComparison.Ordinal);
        Assert.True(end > start, "Prototype catalog boundary is missing.");
        using var prototype = JsonDocument.Parse(generated[start..end].TrimEnd().TrimEnd(';'));
        var groups = catalog.RootElement.EnumerateArray().ToArray();
        Assert.Equal("home", groups.First().GetProperty("id").GetString());
        Assert.Equal("examples", groups.Last().GetProperty("id").GetString());
        Assert.Equal(groups.Length, prototype.RootElement.GetArrayLength());
        var chinese = Read(Path.Combine(DemoDirectory, "I18n", "CodeWF.Toolkit.Demo.zh-CN.json"));
        var english = Read(Path.Combine(DemoDirectory, "I18n", "CodeWF.Toolkit.Demo.en-US.json"));
        var keys = new HashSet<string>();
        var routes = new HashSet<string>();
        var apis = new HashSet<string>();
        var factory = File.ReadAllText(Path.Combine(DemoDirectory, "ViewModels", "DemoCatalog.g.cs"));
        foreach (var group in groups)
        {
            var groupKey = group.GetProperty("titleKey").GetString()!;
            Assert.True(keys.Add(groupKey), "Duplicate group: " + groupKey);
            Assert.Equal(group.GetProperty("titleZh").GetString(), chinese[groupKey]);
            Assert.Equal(group.GetProperty("titleEn").GetString(), english[groupKey]);
            var prototypeGroup = prototype.RootElement.EnumerateArray().Single(g => g.GetProperty("id").GetString() == group.GetProperty("id").GetString());
            Assert.True(JsonElement.DeepEquals(group, prototypeGroup), "Prototype catalog is out of date.");
            foreach (var item in group.GetProperty("items").EnumerateArray())
            {
                if (item.TryGetProperty("sectionKey", out var section))
                {
                    Assert.True(chinese.ContainsKey(section.GetString()!), "Missing Chinese Ribbon section.");
                    Assert.True(english.ContainsKey(section.GetString()!), "Missing English Ribbon section.");
                }
                var key = item.GetProperty("titleKey").GetString()!;
                Assert.True(keys.Add(key), "Duplicate menu key: " + key);
                Assert.True(routes.Add(item.GetProperty("prototypeId").GetString()!), "Duplicate prototype route: " + key);
                Assert.Equal(item.GetProperty("titleZh").GetString(), chinese[key]);
                Assert.Equal(item.GetProperty("titleEn").GetString(), english[key]);
                Assert.Contains("\"" + key + "\"", factory);
                Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("icon").GetString()), key);
                Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("prototypeIcon").GetString()), key);
                Assert.StartsWith("SemiIcon", item.GetProperty("iconKey").GetString());
                if (group.GetProperty("id").GetString() is "home" or "examples") continue;
                var api = item.GetProperty("api").GetString()!;
                Assert.False(string.IsNullOrWhiteSpace(api), key);
                Assert.True(apis.Add(api), "Duplicate API menu: " + api);
                Assert.StartsWith("CodeWF.", item.GetProperty("package").GetString());
                Assert.True(chinese.ContainsKey("Demo.Api." + key.Split('.').Last() + ".Desc"), key);
            }
        }
    }


    [Fact]
    public void DemoTextFilesAreValidUtf8WithoutReplacementCharacters()
    {
        var utf8 = new System.Text.UTF8Encoding(false, true);
        foreach (var file in Directory.EnumerateFiles(DemoDirectory, "*", SearchOption.AllDirectories)
                     .Where(file => file.EndsWith(".cs") || file.EndsWith(".axaml") || file.EndsWith(".json")))
        {
            var content = utf8.GetString(File.ReadAllBytes(file));
            Assert.DoesNotContain("\uFFFD", content);
        }
    }

    private static Dictionary<string, string> Read(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var resources = new Dictionary<string, string>();
        void Walk(JsonElement element, string prefix)
        {
            if (element.ValueKind == JsonValueKind.Object)
                foreach (var property in element.EnumerateObject())
                    Walk(property.Value, prefix.Length == 0 ? property.Name : prefix + "." + property.Name);
            else if (element.ValueKind == JsonValueKind.String && prefix.StartsWith("Demo."))
                resources.Add(prefix, element.GetString()!);
        }
        Walk(document.RootElement, "");
        return resources;
    }
}
