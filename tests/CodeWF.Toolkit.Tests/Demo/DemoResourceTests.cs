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
        foreach (var resource in english.Where(resource => resource.Key != "Demo.Text.MainViewModel.Content001"))
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
