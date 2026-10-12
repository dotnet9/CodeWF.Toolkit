using System.Windows.Input;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using ReactiveUI;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>ShowMeCode 当前段。</summary>
public enum DemoSegment
{
    Demo,
    Xaml,
    CSharp,
}

/// <summary>
/// 打开的演示文档：头部带 ShowMeCode 三段切换（演示 / XAML / C#），
/// 右侧 PropertyGrid 绑定 <see cref="Page"/> 的属性定义。
/// </summary>
public class DemoDocumentViewModel : ReactiveObject
{
    private DemoSegment _segment = DemoSegment.Demo;

    public DemoDocumentViewModel(DemoItemViewModel item, DemoPageBase page)
    {
        Item = item;
        Page = page;
        ValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(change => Page.OnPropertyCommitted(change));
        BrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(_ => { });

        I18nManager.Instance.CultureChanged += OnCultureChanged;
    }

    public DemoItemViewModel Item { get; }

    public DemoPageBase Page { get; }

    /// <summary>承载本文档的 Dock Document 包装（用于 Dock 工作区页签），由 Shell 回填。</summary>
    public global::Dock.Model.Core.IDockable? DockDocument { get; set; }

    public string TitleKey => Item.TitleKey;

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public string Description => I18nManager.Instance.GetResource(Page.DescriptionKey);

    /// <summary>ShowMeCode 当前段。</summary>
    public DemoSegment Segment
    {
        get => _segment;
        set
        {
            if (Equals(_segment, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _segment, value);
            this.RaisePropertyChanged(nameof(IsDemoSegment));
            this.RaisePropertyChanged(nameof(IsXamlSegment));
            this.RaisePropertyChanged(nameof(IsCSharpSegment));
        }
    }

    public bool IsDemoSegment
    {
        get => Segment == DemoSegment.Demo;
        set => Segment = DemoSegment.Demo;
    }

    public bool IsXamlSegment
    {
        get => Segment == DemoSegment.Xaml;
        set => Segment = DemoSegment.Xaml;
    }

    public bool IsCSharpSegment
    {
        get => Segment == DemoSegment.CSharp;
        set => Segment = DemoSegment.CSharp;
    }

    /// <summary>ShowMeCode 的 XAML 片段。</summary>
    public string XamlCode => Page.XamlCode;

    /// <summary>ShowMeCode 的 C# 片段；无内容时回退 XAML 片段，避免空段。</summary>
    public string CSharpCode => string.IsNullOrEmpty(Page.CSharpCode) ? Page.XamlCode : Page.CSharpCode;

    public ICommand ValueCommittedCommand { get; }

    public ICommand BrowseRequestedCommand { get; }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(Title));
        this.RaisePropertyChanged(nameof(Description));
    }
}
