using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>页面属性面板的 PropertyItem 构造与取值助手。</summary>
public static class DemoProperty
{
    public static PropertyItem ReadOnly(string id, string name, object? value, string? description = null) => new()
    {
        Id = id,
        Name = name,
        Kind = PropertyItemKind.ReadOnly,
        Value = value,
        DefaultValue = value,
        Description = description,
    };

    public static PropertyItem Bool(string id, string name, bool value, string? description = null) => new()
    {
        Id = id,
        Name = name,
        Kind = PropertyItemKind.Boolean,
        Value = value,
        DefaultValue = value,
        Description = description,
    };

    public static PropertyItem Number(string id, string name, double value, double min = 0, double max = 1_000_000,
        double increment = 1, string? unit = null, string? description = null) => new()
    {
        Id = id,
        Name = name,
        Kind = PropertyItemKind.Number,
        Value = (decimal)value,
        DefaultValue = (decimal)value,
        Minimum = min,
        Maximum = max,
        Increment = increment,
        Unit = unit,
        Description = description,
    };

    public static PropertyItem Choice(string id, string name, string value, IReadOnlyList<string> options,
        string? description = null) => new()
    {
        Id = id,
        Name = name,
        Kind = PropertyItemKind.Choice,
        Value = value,
        DefaultValue = value,
        Options = options,
        Description = description,
    };

    public static PropertyItem Category(string id, string name, string? description, params PropertyItem[] children) => new()
    {
        Id = id,
        Name = name,
        Kind = PropertyItemKind.Category,
        Description = description,
        Children = children,
    };

    /// <summary>刷新摘要与修改标记；页面 <c>BuildProperties</c> 末尾调用。</summary>
    public static IReadOnlyList<PropertyItem> Finish(params PropertyItem[] roots)
    {
        foreach (var root in roots)
        {
            foreach (var node in root.SelfAndDescendants())
            {
                node.RefreshSummary();
            }

            root.RecalculateModifiedDeep();
        }

        return roots;
    }

    public static string? Text(PropertyValueChange change) => change.NewValue?.ToString();

    public static bool Flag(PropertyValueChange change) =>
        change.NewValue is true || string.Equals(change.NewValue?.ToString(), "True", StringComparison.OrdinalIgnoreCase);

    public static double Num(PropertyValueChange change, double fallback = 0) =>
        double.TryParse(change.NewValue?.ToString(), out var value) ? value : fallback;
}
