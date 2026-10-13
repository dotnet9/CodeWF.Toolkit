using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
public sealed class AboutPage : DemoPageBase
{
    public AboutPage() => Content = Ui.Page(
        Ui.Heading("@Demo.Catalog.About"), Ui.Sub("@Demo.Catalog.AboutDesc"),
        Ui.Card("@Demo.Catalog.Project", Ui.Code("dotnet add package CodeWF.Toolkit.Core"), Ui.Key("https://github.com/dotnet9/CodeWF.Toolkit")),
        Ui.Hint("@Demo.Catalog.License"));
    public override string TitleKey => "Demo.Item.About";
    public override string DescriptionKey => "Demo.Catalog.AboutDesc";
    public override string XamlCode => "";
}
