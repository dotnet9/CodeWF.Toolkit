using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using Avalonia.Controls;

namespace CodeWF.Toolkit.Demo.Avalonia.Views;

public partial class DemoDocumentView : UserControl
{
    public DemoDocumentView()
    {
        InitializeComponent();
        // Grid 的列集合不支持绑定，仅在此连接视图模型持有的布局定义。
        DataContextChanged += (_, _) =>
        {
            if (DataContext is DemoDocumentViewModel document) DocumentWorkspace.ColumnDefinitions = document.ContentColumns;
        };
    }
}
