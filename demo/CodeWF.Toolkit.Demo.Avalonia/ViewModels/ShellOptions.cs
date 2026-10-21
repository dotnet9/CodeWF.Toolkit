using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Toolkit.Lang;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>界面配色选项（规则 §13 注册表驱动：内置九套 + 用户配色）。</summary>
public sealed class AppearanceOption(string key, string title, string note, IBrush swatch) : global::ReactiveUI.ReactiveObject
{
    public string Key { get; } = key;
    public string Title { get; } = title;
    public string Note { get; } = note;
    public IBrush Swatch { get; } = swatch;
    public ThemeVariant ResolveVariant(bool dark) => StudioTheme.GetVariant(Key, dark);
    public override string ToString() => Title;

    public static System.Collections.Generic.IReadOnlyList<AppearanceOption> CreateAll(StudioThemeController controller)
    {
        var list = new System.Collections.Generic.List<AppearanceOption>();
        foreach (var entry in controller.Registry.List())
            list.Add(new AppearanceOption(entry.Key, entry.Name, entry.Note, controller.GetSwatch(entry.Key)));
        return list;
    }
}

/// <summary>保留对象身份的主题模式选项，切换语言只刷新显示标题。</summary>
public sealed class ThemeModeOption(string titleKey) : global::ReactiveUI.ReactiveObject
{
    public string Title => I18nManager.Instance.GetResource(titleKey);
    public void RefreshTitle() => global::ReactiveUI.IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Title));
    public override string ToString() => Title;
}

/// <summary>可选语言。</summary>
public sealed class CultureOption
{
    public CultureOption(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }

    /// <summary>语言名以其自身语言展示（不随界面语言切换）。</summary>
    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
