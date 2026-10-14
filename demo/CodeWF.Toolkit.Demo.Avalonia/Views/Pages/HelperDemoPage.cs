using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;
using Ui = CodeWF.Avalonia.Controls.Presentation.StudioUi;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>展示单个帮助类的输入、操作与真实结果，复用现有控件。</summary>
public sealed class HelperDemoPage : DemoPageBase
{
    public HelperDemoViewModel ViewModel { get; }

    public HelperDemoPage(HelperExample example)
    {
        ViewModel = new(example);
        DataContext = ViewModel;
        var input = new TextBox { AcceptsReturn = true, FontSize = 13, FontFamily = new global::Avalonia.Media.FontFamily("Consolas, Microsoft YaHei UI"), MinHeight = 72, MaxHeight = 150 };
        input.Bind(TextBox.TextProperty, new Binding(nameof(ViewModel.Input)) { Mode = BindingMode.TwoWay });
        var button = new Button { Classes = { "demo-action" }, Command = ViewModel.RunCommand, HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left };
        button.Bind(Button.IsEnabledProperty, new Binding(nameof(ViewModel.IsBusy)) { Converter = BoolConverters.Not });
        button.Bind(ContentControl.ContentProperty, new CodeWF.Avalonia.Lang.MarkupExtensions.I18nExtension(DemoResources.Catalog.Run).CreateBinding());
        var output = Ui.Code("", output: true);
        output.Bind(TextBlock.TextProperty, new Binding(nameof(ViewModel.Output)));
        var progress = new ProgressBar { IsIndeterminate = true, Height = 3 };
        progress.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(ViewModel.IsBusy)));
        Content = Ui.Page(Ui.Card("@Demo.Catalog.Input", input, button, progress), Ui.Card("@Demo.Catalog.Output", output));
    }

    public override string TitleKey => "Demo.Item." + ViewModel.Example.Key;
    public override string DescriptionKey => "Demo.Api." + ViewModel.Example.Key + ".Desc";
    public override string XamlCode => string.Empty;
    public override string CSharpCode => ViewModel.Example.Code;
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();
    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
}
