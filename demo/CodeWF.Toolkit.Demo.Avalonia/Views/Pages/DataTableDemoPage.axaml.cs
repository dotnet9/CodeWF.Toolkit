using Avalonia.Controls;
using CodeWF.Avalonia.Lang.MarkupExtensions;
using CodeWF.Avalonia.TreeDataGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

public sealed partial class DataTableDemoPage : DemoPageBase
{
    public DataTableDemoViewModel ViewModel { get; } = new();
    public DataTableDemoPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        PreviewGrid.EnableDefaults(ViewModel.Source);
    }
    public override string TitleKey => "Demo.Item.DataTableExtensions";
    public override string DescriptionKey => "Demo.Api.DataTableExtensions.Desc";
    public override string XamlCode => "";
    public override string CSharpCode => "table.ExportToCsv(csvPath, Encoding.UTF8, out var error);\ncsvPath.ImportFromCsv(out error, out var csvTable);\ntable.ExportToXlsx(excelPath, out error);\nexcelPath.ImportFromExcel(out error, out var excelTable);\n// Dispose the imported DataTables after use.";
}
