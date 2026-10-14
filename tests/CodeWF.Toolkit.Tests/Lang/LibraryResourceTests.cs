using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeWF.Toolkit.Tests.Lang;

public sealed class LibraryResourceTests
{
    [Fact]
    public void LibraryResourcesHaveStableKeysMatchingTranslationsAndFormatArguments()
    {
        var root = FindRepository();
        var known = new HashSet<string>(StringComparer.Ordinal);
        var folders = Directory.GetDirectories(Path.Combine(root, "src"), "I18n", SearchOption.AllDirectories);
        Assert.NotEmpty(folders);
        foreach (var folder in folders)
        {
            using var english = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "en-US.json"), new UTF8Encoding(false, true)));
            using var chinese = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "zh-CN.json"), new UTF8Encoding(false, true)));
            var en = english.RootElement.EnumerateObject().Where(p => p.Name.StartsWith("Toolkit.", StringComparison.Ordinal)).ToDictionary(p => p.Name, p => p.Value.GetString()!);
            var zh = chinese.RootElement.EnumerateObject().Where(p => p.Name.StartsWith("Toolkit.", StringComparison.Ordinal)).ToDictionary(p => p.Name, p => p.Value.GetString()!);
            Assert.Equal(en.Keys.Order(), zh.Keys.Order());
            foreach (var (key, value) in en)
            {
                Assert.True(known.Add(key), $"Duplicate resource key: {key}");
                Assert.DoesNotMatch(@"\d{3}$", key);
                Assert.DoesNotContain('\ufffd', value);
                Assert.Equal(CompositeFormat.Parse(value).MinimumArgumentCount, CompositeFormat.Parse(zh[key]).MinimumArgumentCount);
            }
        }
        foreach (var file in SourceFiles(root))
        {
            var source = File.ReadAllText(file, new UTF8Encoding(false, true));
            foreach (Match match in Regex.Matches(source, "\"(Toolkit\\.[A-Za-z0-9.]+)\""))
                Assert.Contains(match.Groups[1].Value, known);
        }
    }

    [Fact]
    public void LibraryCodeContainsNoChineseStringLiteralsOrBrokenUtf8()
    {
        foreach (var file in SourceFiles(FindRepository()))
        {
            var source = File.ReadAllText(file, new UTF8Encoding(false, true));
            Assert.DoesNotContain('\ufffd', source);
            foreach (var token in CSharpSyntaxTree.ParseText(source).GetRoot().DescendantTokens())
            {
                if (token.Kind() is SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringTextToken
                    or SyntaxKind.Utf8StringLiteralToken or SyntaxKind.SingleLineRawStringLiteralToken or SyntaxKind.MultiLineRawStringLiteralToken)
                    Assert.False(Regex.IsMatch(token.ValueText, "[\\u4e00-\\u9fff]"), $"Hardcoded text in {file}:{token.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
            }
        }
    }

    private static IEnumerable<string> SourceFiles(string root) => Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
        .Where(file => !file.Split(Path.DirectorySeparatorChar).Any(part => part is "obj" or "bin"));

    private static string FindRepository()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "CodeWF.Toolkit.slnx"))) return directory.FullName;
        throw new DirectoryNotFoundException("CodeWF.Toolkit.slnx was not found.");
    }
}
