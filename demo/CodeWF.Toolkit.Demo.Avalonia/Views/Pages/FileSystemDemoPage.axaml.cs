using Avalonia.Controls;
using CodeWF.Avalonia.Lang.MarkupExtensions;
using CodeWF.Avalonia.TreeDataGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

public sealed partial class FileSystemDemoPage : DemoPageBase
{
    private readonly bool _server;
    public FileSystemDemoViewModel ViewModel { get; } = new();
    public FileSystemDemoPage() : this(false) { }
    public FileSystemDemoPage(bool server)
    {
        _server = server;
        InitializeComponent();
        DataContext = ViewModel;
        PreviewGrid.EnableDefaults(ViewModel.Source);
        SessionHint.Bind(TextBlock.TextProperty, new I18nExtension(server ? "Demo.Catalog.FileServerHint" : "Demo.Catalog.FileClientHint").CreateBinding());
    }
    public override string TitleKey => "Demo.Item.TcpSocket" + (_server ? "Server" : "Client") + "FileSystemFeature";
    public override string DescriptionKey => "Demo.Api.TcpSocket" + (_server ? "Server" : "Client") + "FileSystemFeature.Desc";
    public override string XamlCode => "";
    public override string CSharpCode => _server
        ? "using var feature = new TcpSocketServerFileSystemFeature(server) { RootDirectory = isolatedRoot };\nawait server.StartAsync(mark, \"127.0.0.1\", 0);\n// The local client drives real browse, create, delete and file-transfer requests.\nawait server.StopAsync();"
        : "using var feature = new TcpSocketClientFileSystemFeature(client);\nfeature.FileTransferProgress += OnProgress;\nfeature.FileTransferOutcome += OnOutcome;\nawait feature.BrowseFileSystemAsync(root);\nawait feature.CreateDirectoryAsync(path);\nawait feature.UploadFileAsync(localFile, remoteFile, cancellationToken);\nawait feature.DownloadFileAsync(remoteFile, localDirectory, cancellationToken);\nawait feature.DeletePathAsync(path, isDirectory: true);\n// Await response/outcome events; sending a request does not mean completion.";
    public override Task OnDocumentClosedAsync() => ViewModel.StopAsync();
}
