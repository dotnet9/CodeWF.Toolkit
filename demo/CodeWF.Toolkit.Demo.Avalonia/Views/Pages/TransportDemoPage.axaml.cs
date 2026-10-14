using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

public sealed partial class TransportDemoPage : DemoPageBase
{
    public TransportDemoViewModel ViewModel { get; }
    public TransportDemoPage() : this("TcpSocketClient") { }
    public TransportDemoPage(string api)
    {
        ViewModel = new(api);
        InitializeComponent();
        DataContext = ViewModel;
    }
    public override string TitleKey => "Demo.Item." + ViewModel.Api;
    public override string DescriptionKey => "Demo.Api." + ViewModel.Api + ".Desc";
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();
    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
    public override string XamlCode => "";
    public override string CSharpCode => HelperDemoCatalog.Create(ViewModel.Api).Code;
    public override Task OnDocumentClosedAsync() => ViewModel.StopAsync();
}
