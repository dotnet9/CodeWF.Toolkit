using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>演示页 UI 建造助手：统一卡片 / 标题 / 代码块 / 行的外观（样式见 Views/DemoStyles.axaml）。</summary>
public static class Ui
{
    /// <summary>演示页根：滚动容器 + 限宽堆叠面板。</summary>
    public static ScrollViewer Page(params Control[] children)
    {
        var panel = new StackPanel { Spacing = 12, Margin = new Thickness(16, 14) };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return new ScrollViewer { Content = panel };
    }

    public static TextBlock Heading(string text) => new() { Text = text, Classes = { "h" } };

    public static TextBlock Sub(string text) => new() { Text = text, Classes = { "sub" } };

    public static TextBlock Hint(string text) => new() { Text = text, Classes = { "hint" } };

    public static TextBlock Key(string text) => new() { Text = text, Classes = { "k" } };

    /// <summary>代码/输出块（等宽、可选中复制）。</summary>
    public static SelectableTextBlock Code(string text, bool output = false) => new()
    {
        Text = text,
        Classes = { output ? "output" : "code" },
    };

    /// <summary>卡片：可选小标题 + 内容。</summary>
    public static Border Card(string? header, params Control[] content)
    {
        var panel = new StackPanel { Spacing = 8 };
        if (!string.IsNullOrEmpty(header))
        {
            panel.Children.Add(new TextBlock { Text = header, FontWeight = FontWeight.SemiBold, FontSize = 12.5 });
        }

        foreach (var child in content)
        {
            panel.Children.Add(child);
        }

        var border = new Border { Classes = { "card" }, Child = panel };
        return border;
    }

    /// <summary>两列等宽网格（左 / 右）。</summary>
    public static Grid TwoColumns(Control left, Control right)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 12 };
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 1);
        grid.Children.Add(left);
        grid.Children.Add(right);
        return grid;
    }

    public static StackPanel Row(params Control[] children)
    {
        var panel = new StackPanel { Classes = { "row" } };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    public static StackPanel RowLoose(double spacing, params Control[] children)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = spacing, VerticalAlignment = VerticalAlignment.Center };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    /// <summary>小按钮（样式类 b；primary=true 用主色填充）。</summary>
    public static Button Button(string text, Action onClick, bool primary = false)
    {
        var button = new Button { Content = text, Classes = { "b" } };
        if (!primary)
        {
            button.Background = Brushes.Transparent;
        }
        else
        {
            button.Classes.Add("accent");
        }

        button.Click += (_, _) => onClick();
        return button;
    }

    /// <summary>标签 + 控件的一行。</summary>
    public static StackPanel Field(string label, Control control, double labelWidth = 92)
    {
        var text = Key(label);
        text.Width = labelWidth;
        control.VerticalAlignment = VerticalAlignment.Center;
        return RowLoose(8, text, control);
    }

    public static TextBox Input(string text, double width = double.NaN, bool multiline = false)
    {
        var box = new TextBox
        {
            Text = text,
            FontSize = 12,
            MinHeight = multiline ? 64 : 26,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
        };

        if (!double.IsNaN(width))
        {
            box.Width = width;
        }
        else
        {
            box.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        return box;
    }

    public static TextBox Number(double value, double width = 84)
    {
        var box = new TextBox { Text = value.ToString("0.##"), Width = width, FontSize = 12, MinHeight = 26 };
        return box;
    }

    public static ToggleSwitch Switch(bool value) => new() { IsChecked = value, OnContent = null, OffContent = null };

    public static ComboBox Select(IEnumerable<string> options, int selectedIndex = 0, double minWidth = 120)
    {
        var combo = new ComboBox { ItemsSource = options.ToList(), SelectedIndex = selectedIndex, MinWidth = minWidth, FontSize = 12 };
        return combo;
    }
}
