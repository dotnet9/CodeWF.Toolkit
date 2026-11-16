using CodeWF.Toolkit.Core.Git;
using Xunit;

namespace CodeWF.Toolkit.Tests;

/// <summary>托管行级 diff（Myers 简化）：上下文/删除/新增分类与尾部保持。</summary>
public sealed class TextDiffBuilderTests
{
    [Fact]
    public void Build_marks_additions_and_deletions_with_context()
    {
        var diff = TextDiffBuilder.Build("a.md", ["L1", "old", "L3"], ["L1", "new", "L3"]);
        var kinds = diff.Hunks.SelectMany(h => h.Lines).Select(l => l.Kind).ToArray();
        Assert.Equal([' ', '-', '+', ' ', ' '], kinds);
        Assert.Contains(diff.Hunks.SelectMany(h => h.Lines), l => l.Kind == '+' && l.Text == "new");
        Assert.Contains(diff.Hunks.SelectMany(h => h.Lines), l => l.Kind == '-' && l.Text == "old");
    }

    [Fact]
    public void CountRows_reports_added_and_deleted()
    {
        var (added, deleted) = TextDiffBuilder.CountRows(["a", "b", "c"], ["a", "c", "d", "e"]);
        Assert.Equal(2, added);
        Assert.Equal(1, deleted);
    }

    [Fact]
    public void Empty_old_lines_means_all_added()
    {
        var (added, deleted) = TextDiffBuilder.CountRows([], ["only"]);
        Assert.Equal(1, added);
        Assert.Equal(0, deleted);
    }
}
