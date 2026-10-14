using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWrapper.FileSystem;
using CodeWF.Toolkit.NetWrapper.Helpers;
using CodeWF.Toolkit.NetWrapper.Models;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>
/// NetWrapper 页：真实启动 TCP 服务端 + 客户端（本机回环、临时端口），
/// 演示命令下发与远程文件系统下载（分块 + 断点续传）。
/// </summary>
public sealed class NetSocketPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.NetWrapper" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Content.NetSocket.Source.CSharp");

    private readonly TextBox _clientCount;
    private readonly TextBox _sampleSize;
    private readonly TextBlock _serverState;
    private readonly TextBlock _clientState;
    private readonly SelectableTextBlock _feed;
    private readonly SelectableTextBlock _transferOutput;
    private readonly Button _startButton;
    private readonly Button _stopButton;
    private readonly Button _downloadButton;

    private TcpSocketServer? _server;
    private TcpSocketClient? _client;
    private TcpSocketClientFileSystemFeature? _clientFileSystem;
    private string? _sampleFilePath;
    private int _port;

    public NetSocketPage()
    {
        _clientCount = Ui.Input("0", 80);
        _sampleSize = Ui.Input("2", 80);
        _serverState = Ui.Key("@Demo.Content.NetSocket.Content.NotStarted");
        _clientState = Ui.Key("@Demo.Content.NetSocket.Content.Disconnected");
        _feed = Ui.Code("@Demo.Content.NetSocket.Content.AwaitingStartup", output: true);
        _transferOutput = Ui.Code("@Demo.Content.NetSocket.Content.AwaitingDownload", output: true);

        _startButton = Ui.Button("@Demo.Content.NetSocket.Content.StartServerAndClient", async () => await StartAsync(), primary: true);
        _stopButton = Ui.Button("@Demo.Content.NetSocket.Content.Stop", async () => await StopAsync());
        _downloadButton = Ui.Button("@Demo.Content.NetSocket.Content.DownloadSampleChunksAndResume", async () => await DownloadAsync(), primary: true);

        var server = Ui.Card("@Demo.Content.NetSocket.server.ServerClient",
            Ui.Field(L.Instance.GetResource("Demo.Content.NetSocket.server.ListenAddress"), Ui.Key("@Demo.Content.NetSocket.server.127001Loopback")),
            Ui.Field(L.Instance.GetResource("Demo.Content.NetSocket.server.Port"), Ui.Key("@Demo.Content.NetSocket.server.AvailableTemporaryPortIpHelperGetAvailableTcpPort")),
            Ui.Row(_startButton, _stopButton),
            Ui.Field(L.Instance.GetResource("Demo.Content.NetSocket.server.Server"), _serverState),
            Ui.Field(L.Instance.GetResource("Demo.Content.NetSocket.server.Client"), _clientState),
            Ui.Field(L.Instance.GetResource("Demo.Content.NetSocket.server.ClientCount"), _clientCount),
            Ui.Hint("@Demo.Content.NetSocket.server.ClientConnectionsDisconnectionsAndErrorsAreBroadcastThrough"));

        var messages = Ui.Card("@Demo.Content.NetSocket.messages.MessagesAndSession",
            _feed,
            Ui.Row(Ui.Button("@Demo.Content.NetSocket.messages.ClearSession", () => _feed.Text = string.Empty),
                Ui.Button("@Demo.Content.NetSocket.messages.SendHeartbeat", async () => await SendHeartbeatAsync())),
            Ui.Code("@Demo.Content.NetSocket.messages.TcpSocketServerStartAsyncServerMarkIpPortTimeoutTcpSocketClientConnectAsync", output: true),
            Ui.Hint("@Demo.Content.NetSocket.messages.TheServerRemovesClientsWithNoActivityFor"));

        var transfer = Ui.Card("@Demo.Content.NetSocket.transfer.RemoteFileSystemRealDownload",
            Ui.Field(L.Instance.GetResource("Demo.Content.NetSocket.transfer.SampleSizeMB"), _sampleSize),
            _downloadButton,
            _transferOutput,
            Ui.Hint("@Demo.Content.NetSocket.transfer.DownloadsA2MBSampleByDefaultThroughChunk"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.NetSocket.Content.NetWrapperTCPUDPAndRemoteFiles"),
            Ui.Sub("@Demo.Content.NetSocket.Content.TcpSocketServerTcpSocketClientProvideCommandDispatchAndAResumable"),
            Ui.TwoColumns(server, messages),
            transfer);
    }

    public override string TitleKey => DemoKeys.NetSocketExampleTitle;

    public override string DescriptionKey => DemoKeys.DescNetSocket;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("ns.conn", L.Instance.GetResource("Demo.Content.NetSocket.Properties.ConnectionOptions"), L.Instance.GetResource("Demo.Content.NetSocket.Properties.LocalLoopbackConnectionOptions"),
            DemoProperty.ReadOnly("ns.host", L.Instance.GetResource("Demo.Content.NetSocket.Properties.Host"), "127.0.0.1"),
            DemoProperty.ReadOnly("ns.port", L.Instance.GetResource("Demo.Content.NetSocket.Properties.Port"), L.Instance.GetResource("Demo.Content.NetSocket.Properties.AvailableTemporaryPort")),
            DemoProperty.Number("ns.timeout", L.Instance.GetResource("Demo.Content.NetSocket.Properties.ClientTimeoutSeconds"), 30, 5, 300, 1, "s", L.Instance.GetResource("Demo.Content.NetSocket.Properties.ServerHeartbeatTimeout")),
            DemoProperty.Number("ns.chunk", L.Instance.GetResource("Demo.Content.NetSocket.Properties.ChunkSizeKB"), 64, 4, 1024, 4, "KB", "FileTransferBlockSize。")),
        DemoProperty.Category("ns.file", L.Instance.GetResource("Demo.Content.NetSocket.Properties.FileTransfer"), L.Instance.GetResource("Demo.Content.NetSocket.Properties.OptionsForTheRemoteFileDemo"),
            DemoProperty.Bool("ns.resume", L.Instance.GetResource("Demo.Content.NetSocket.Properties.ResumeTransfer"), true, L.Instance.GetResource("Demo.Content.NetSocket.Properties.ResumeFromTheByteCountInTheTemporary")),
            DemoProperty.ReadOnly("ns.dir", L.Instance.GetResource("Demo.Content.NetSocket.Properties.DownloadDirectory"), Path.Combine("%TEMP%", "CodeWFToolkitDemo", "net"))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "ns.timeout" && _server is not null)
        {
            Append(L.Instance.GetResource("Demo.Content.NetSocket.OnPropertyCommitted.SetTheTimeoutBeforeStartupTimeOutIsRead"));
        }
    }

    private async Task StartAsync()
    {
        if (_server is not null)
        {
            return;
        }

        _startButton.IsEnabled = false;
        try
        {
            _port = CodeWF.Toolkit.Core.Helpers.IpHelper.GetAvailableTcpPort();
            _server = new TcpSocketServer();
            var (started, startError) = await _server.StartAsync(L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.DemoFileService"), "127.0.0.1", _port);
            if (!started)
            {
                Append(L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.ServerStartupFailed") + startError);
                await StopAsync();
                return;
            }

            _server.UseFileSystem(Directory.GetCurrentDirectory());
            _serverState.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.Running127001Format"), _port);
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.ServerStartAsyncDemoFileService12700Format"), _port));
            Append($"server.UseFileSystem(\"{Directory.GetCurrentDirectory()}\")");

            _client = new TcpSocketClient();
            var (connected, connectError) = await _client.ConnectAsync(L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.DemoFileService"), "127.0.0.1", _port);
            if (!connected)
            {
                Append(L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.ClientConnectionFailed") + connectError);
                await StopAsync();
                return;
            }

            _clientState.Text = L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.Connected");
            _clientCount.Text = _server.Clients.Count.ToString();
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.ClientConnectAsyncConnectedLocalEndpointFormat"), _client.LocalEndPoint));

            _clientFileSystem = _client.UseFileSystem();
            _clientFileSystem.FileTransferProgress += OnTransferProgress;
            _clientFileSystem.FileTransferOutcome += OnTransferOutcome;

            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.NetWrapperDemoTCPConnectedAt12700Format"), _port));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.StartupError") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Content.NetSocket.StartAsync.NetWrapperDemoStartupFailed"), ex);
            await StopAsync();
        }
        finally
        {
            _startButton.IsEnabled = true;
        }
    }

    private async Task StopAsync()
    {
        if (_clientFileSystem is not null)
        {
            _clientFileSystem.FileTransferProgress -= OnTransferProgress;
            _clientFileSystem.FileTransferOutcome -= OnTransferOutcome;
            _clientFileSystem = null;
        }

        if (_client is not null)
        {
            await _client.StopAsync();
            _client = null;
        }

        if (_server is not null)
        {
            await _server.StopAsync();
            _server = null;
        }

        _serverState.Text = L.Instance.GetResource("Demo.Content.NetSocket.StopAsync.NotStarted");
        _clientState.Text = L.Instance.GetResource("Demo.Content.NetSocket.StopAsync.Disconnected");
        _clientCount.Text = "0";
        Append("client.Stop() / await server.StopAsync()");
        Logger.Warn(L.Instance.GetResource("Demo.Content.NetSocket.StopAsync.NetWrapperDemoStopped"));
    }

    private async Task SendHeartbeatAsync()
    {
        if (_client is null || _server is null)
        {
            Append(L.Instance.GetResource("Demo.Content.NetSocket.SendHeartbeatAsync.StartTheServerAndClientFirst"));
            return;
        }

        try
        {
            await _client.SendCommandAsync(new Heartbeat { TaskId = Random.Shared.Next(1, 10_000) });
            Append(L.Instance.GetResource("Demo.Content.NetSocket.SendHeartbeatAsync.ClientSendCommandAsyncNewHeartbeatSent"));
            Logger.Info(L.Instance.GetResource("Demo.Content.NetSocket.SendHeartbeatAsync.NetWrapperDemoHeartbeatSent"));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Content.NetSocket.SendHeartbeatAsync.SendFailed") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Content.NetSocket.SendHeartbeatAsync.NetWrapperDemoHeartbeatFailed"), ex);
        }
    }

    private async Task DownloadAsync()
    {
        if (_client is null)
        {
            Append(L.Instance.GetResource("Demo.Content.NetSocket.DownloadAsync.StartTheServerAndClientFirst"));
            return;
        }

        try
        {
            var downloadDirectory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "net");
            Directory.CreateDirectory(downloadDirectory);

            var sizeMb = double.TryParse(_sampleSize.Text, out var parsed) ? Math.Clamp(parsed, 1, 8) : 2;
            _sampleFilePath = await PrepareSampleFileAsync(downloadDirectory, sizeMb);
            var fileName = Path.GetFileName(_sampleFilePath);

            _downloadButton.IsEnabled = false;
            _transferOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.DownloadAsync.SamplePreparedBytesFormat"), fileName, new FileInfo(_sampleFilePath).Length)
                                   + L.Instance.GetResource("Demo.Content.NetSocket.DownloadAsync.ClientDownloadFileAsyncAwaitingServerChunks");
            Append($"client.DownloadFileAsync(\"{_sampleFilePath}\", \"{downloadDirectory}\")");

            await _client.DownloadFileAsync(_sampleFilePath, downloadDirectory);
        }
        catch (Exception ex)
        {
            _transferOutput.Text = L.Instance.GetResource("Demo.Content.NetSocket.DownloadAsync.DownloadFailed") + ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Content.NetSocket.DownloadAsync.NetWrapperDemoDownloadFailed"), ex);
            _downloadButton.IsEnabled = true;
        }
    }

    private static async Task<string> PrepareSampleFileAsync(string directory, double sizeMb)
    {
        var path = Path.Combine(directory, "demo-download.bin");
        var target = (long)(sizeMb * 1024 * 1024);
        if (File.Exists(path) && new FileInfo(path).Length == target)
        {
            return path;
        }

        var buffer = new byte[64 * 1024];
        Random.Shared.NextBytes(buffer);
        await using var stream = File.Create(path);
        long written = 0;
        while (written < target)
        {
            var count = (int)Math.Min(buffer.Length, target - written);
            await stream.WriteAsync(buffer.AsMemory(0, count));
            written += count;
        }

        return path;
    }

    private void OnTransferProgress(object? sender, FileTransferProgressEventArgs e)
    {
        _transferOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferProgress.DownloadFormat"), e.FileName)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferProgress.ProgressBytesFormat"), e.Progress, e.TransferredBytes, e.TotalBytes)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferProgress.ChunkSizeKBTaskFormat"), TcpSocketClientFileSystemFeature.FileTransferBlockSize / 1024, e.TaskId);
    }

    private void OnTransferOutcome(object? sender, FileTransferOutcomeEventArgs e)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "net", e.FileName);
        _transferOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.DownloadFormat"), (e.Success ? L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.Completed") : L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.Failed")), localPath)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.DirectionTaskCancelledFormat"), (e.IsUpload ? L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.Upload") : L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.Download")), e.TaskId, e.IsCancelled)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.ResultFormat"), e.Message);
        Append($"FileTransferOutcome → {e.FileName}（{(e.Success ? L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.Success") : L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.Failed"))}）");
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.NetSocket.OnTransferOutcome.NetWrapperDemoDownloadEndedSuccessFormat"), e.FileName, e.Success));
        _downloadButton.IsEnabled = true;
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource("Demo.Content.NetSocket.Append.AwaitingStartup"), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
