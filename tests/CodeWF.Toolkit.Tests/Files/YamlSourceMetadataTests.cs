using System.Text.Json.Serialization;
using CodeWF.Toolkit.Files.Extensions;
using Xunit;

namespace CodeWF.Toolkit.Tests.Files;

public sealed class YamlSourceMetadataTests
{
    [Fact]
    public void GeneratedMetadataPreservesScalarTypesAndCollections()
    {
        var expected = new YamlPayload
        {
            Name = "0001",
            Values = ["false", "null", "2026-10-13", "7:30", "a: b"],
            Count = -42,
            Enabled = true,
        };
        Assert.True(expected.ToYaml(YamlTestContext.Default.YamlPayload, out var yaml, out var error), error);
        Assert.True(yaml.FromYaml(YamlTestContext.Default.YamlPayload, out var actual, out error), error);
        Assert.NotNull(actual);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Values, actual.Values);
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected.Enabled, actual.Enabled);
        Assert.Null(actual.Optional);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Name: [unterminated")]
    public void InvalidDocumentReturnsAnError(string yaml)
    {
        Assert.False(yaml.FromYaml(YamlTestContext.Default.YamlPayload, out var value, out var error));
        Assert.Null(value);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}

public sealed class YamlPayload
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = [];
    public int Count { get; set; }
    public bool Enabled { get; set; }
    public string? Optional { get; set; }
}

[JsonSerializable(typeof(YamlPayload))]
internal partial class YamlTestContext : JsonSerializerContext;
