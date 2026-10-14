using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;
using Ui = CodeWF.Avalonia.Controls.Presentation.StudioUi;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>Uses existing controls to display one helper's input, action and real result.</summary>
public sealed class HelperDemoPage : DemoPageBase
{
    public HelperDemoViewModel ViewModel { get; }

    public HelperDemoPage(HelperExample example)
    {
        ViewModel = new(example);
        DataContext = ViewModel;
        var input = new TextBox { AcceptsReturn = true, MinHeight = 72, MaxHeight = 150 };
        input.Bind(TextBox.TextProperty, new Binding(nameof(ViewModel.Input)) { Mode = BindingMode.TwoWay });
        var button = new Button { Classes = { "demo-action" }, Command = ViewModel.RunCommand, HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left };
        button.Bind(Button.IsEnabledProperty, new Binding(nameof(ViewModel.IsBusy)) { Converter = BoolConverters.Not });
        button.Bind(ContentControl.ContentProperty, new CodeWF.Avalonia.Lang.MarkupExtensions.I18nExtension("Demo.Catalog.Run").CreateBinding());
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
}
