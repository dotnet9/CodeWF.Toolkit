using System;
using System.Collections.Generic;

namespace CodeWF.Toolkit.Core.Git;

/// <summary>文件状态枚举：对应原型描边标志（M 黄点 / A 绿加号 / D 红减号 / ? 未跟踪）。</summary>
public enum GitChangeKind
{
    Modified,
    Added,
    Deleted,
    Renamed,
    Untracked,
}

/// <summary>工作区改动条目：路径 + 状态 + 行级增删统计（-1 = 未知，二进制或统计失败，UI 不显示计数）。</summary>
public sealed record ChangeEntry(string Path, GitChangeKind Kind, int LinesAdded = -1, int LinesDeleted = -1)
{
    public string KindLabel => Kind switch
    {
        GitChangeKind.Added => "A",
        GitChangeKind.Deleted => "D",
        GitChangeKind.Modified => "M",
        GitChangeKind.Renamed => "R",
        _ => "?",
    };

    public bool HasLineStats => LinesAdded >= 0 && LinesDeleted >= 0;
}

public sealed record CommitInfo(string Sha, string Message, string Author, DateTimeOffset When, string? Tag = null);

/// <summary>历史某次提交里的文件改动（状态描边 + 路径）。</summary>
public sealed record CommitFileChange(string Path, GitChangeKind Kind);

/// <summary>单行 diff：Kind 为 ' '（上下文）/ '+'（新增）/ '-'（删除）。</summary>
public sealed record DiffLine(char Kind, string Text);

public sealed record DiffHunk(string Header, IReadOnlyList<DiffLine> Lines);

/// <summary>一个文件的文本 diff。</summary>
public sealed record TextDiff(string OldPath, IReadOnlyList<DiffHunk> Hunks);
