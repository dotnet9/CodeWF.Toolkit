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

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.NetSocketPage.CSharpSnippet001");

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
        _serverState = Ui.Key("@Demo.Text.NetSocketPage.Content002");
        _clientState = Ui.Key("@Demo.Text.NetSocketPage.Content003");
        _feed = Ui.Code("@Demo.Text.NetSocketPage.Content004", output: true);
        _transferOutput = Ui.Code("@Demo.Text.NetSocketPage.Content005", output: true);

        _startButton = Ui.Button("@Demo.Text.NetSocketPage.Content006", async () => await StartAsync(), primary: true);
        _stopButton = Ui.Button("@Demo.Text.NetSocketPage.Content007", async () => await StopAsync());
        _downloadButton = Ui.Button("@Demo.Text.NetSocketPage.Content008", async () => await DownloadAsync(), primary: true);

        var server = Ui.Card("@Demo.Text.NetSocketPage.server009",
            Ui.Field(L.Instance.GetResource("Demo.Text.NetSocketPage.server010"), Ui.Key("@Demo.Text.NetSocketPage.server011")),
            Ui.Field(L.Instance.GetResource("Demo.Text.NetSocketPage.server012"), Ui.Key("@Demo.Text.NetSocketPage.server013")),
            Ui.Row(_startButton, _stopButton),
            Ui.Field(L.Instance.GetResource("Demo.Text.NetSocketPage.server014"), _serverState),
            Ui.Field(L.Instance.GetResource("Demo.Text.NetSocketPage.server015"), _clientState),
            Ui.Field(L.Instance.GetResource("Demo.Text.NetSocketPage.server016"), _clientCount),
            Ui.Hint("@Demo.Text.NetSocketPage.server017"));

        var messages = Ui.Card("@Demo.Text.NetSocketPage.messages018",
            _feed,
            Ui.Row(Ui.Button("@Demo.Text.NetSocketPage.messages019", () => _feed.Text = string.Empty),
                Ui.Button("@Demo.Text.NetSocketPage.messages020", async () => await SendHeartbeatAsync())),
            Ui.Code("@Demo.Text.NetSocketPage.messages021", output: true),
            Ui.Hint("@Demo.Text.NetSocketPage.messages022"));

        var transfer = Ui.Card("@Demo.Text.NetSocketPage.transfer023",
            Ui.Field(L.Instance.GetResource("Demo.Text.NetSocketPage.transfer024"), _sampleSize),
            _downloadButton,
            _transferOutput,
            Ui.Hint("@Demo.Text.NetSocketPage.transfer025"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.NetSocketPage.Content026"),
            Ui.Sub("@Demo.Text.NetSocketPage.Content027"),
            Ui.TwoColumns(server, messages),
            transfer);
    }

    public override string TitleKey => DemoKeys.ItemNetSocket;

    public override string DescriptionKey => DemoKeys.DescNetSocket;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("ns.conn", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties028"), L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties029"),
            DemoProperty.ReadOnly("ns.host", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties030"), "127.0.0.1"),
            DemoProperty.ReadOnly("ns.port", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties031"), L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties032")),
            DemoProperty.Number("ns.timeout", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties033"), 30, 5, 300, 1, "s", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties034")),
            DemoProperty.Number("ns.chunk", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties035"), 64, 4, 1024, 4, "KB", "FileTransferBlockSize。")),
        DemoProperty.Category("ns.file", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties036"), L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties037"),
            DemoProperty.Bool("ns.resume", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties038"), true, L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties039")),
            DemoProperty.ReadOnly("ns.dir", L.Instance.GetResource("Demo.Text.NetSocketPage.BuildProperties040"), Path.Combine("%TEMP%", "CodeWFToolkitDemo", "net"))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "ns.timeout" && _server is not null)
        {
            Append(L.Instance.GetResource("Demo.Text.NetSocketPage.OnPropertyCommitted041"));
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
            var (started, startError) = await _server.StartAsync(L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync042"), "127.0.0.1", _port);
            if (!started)
            {
                Append(L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync043") + startError);
                await StopAsync();
                return;
            }

            _server.UseFileSystem(Directory.GetCurrentDirectory());
            _serverState.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync044"), _port);
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync045"), _port));
            Append($"server.UseFileSystem(\"{Directory.GetCurrentDirectory()}\")");

            _client = new TcpSocketClient();
            var (connected, connectError) = await _client.ConnectAsync(L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync046"), "127.0.0.1", _port);
            if (!connected)
            {
                Append(L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync047") + connectError);
                await StopAsync();
                return;
            }

            _clientState.Text = L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync048");
            _clientCount.Text = _server.Clients.Count.ToString();
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync049"), _client.LocalEndPoint));

            _clientFileSystem = _client.UseFileSystem();
            _clientFileSystem.FileTransferProgress += OnTransferProgress;
            _clientFileSystem.FileTransferOutcome += OnTransferOutcome;

            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync050"), _port));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync051") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Text.NetSocketPage.StartAsync052"), ex);
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

        _serverState.Text = L.Instance.GetResource("Demo.Text.NetSocketPage.StopAsync053");
        _clientState.Text = L.Instance.GetResource("Demo.Text.NetSocketPage.StopAsync054");
        _clientCount.Text = "0";
        Append("client.Stop() / await server.StopAsync()");
        Logger.Warn(L.Instance.GetResource("Demo.Text.NetSocketPage.StopAsync055"));
    }

    private async Task SendHeartbeatAsync()
    {
        if (_client is null || _server is null)
        {
            Append(L.Instance.GetResource("Demo.Text.NetSocketPage.SendHeartbeatAsync056"));
            return;
        }

        try
        {
            await _client.SendCommandAsync(new Heartbeat { TaskId = Random.Shared.Next(1, 10_000) });
            Append(L.Instance.GetResource("Demo.Text.NetSocketPage.SendHeartbeatAsync057"));
            Logger.Info(L.Instance.GetResource("Demo.Text.NetSocketPage.SendHeartbeatAsync058"));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Text.NetSocketPage.SendHeartbeatAsync059") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Text.NetSocketPage.SendHeartbeatAsync060"), ex);
        }
    }

    private async Task DownloadAsync()
    {
        if (_client is null)
        {
            Append(L.Instance.GetResource("Demo.Text.NetSocketPage.DownloadAsync061"));
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
            _transferOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.DownloadAsync062"), fileName, new FileInfo(_sampleFilePath).Length)
                                   + L.Instance.GetResource("Demo.Text.NetSocketPage.DownloadAsync063");
            Append($"client.DownloadFileAsync(\"{_sampleFilePath}\", \"{downloadDirectory}\")");

            await _client.DownloadFileAsync(_sampleFilePath, downloadDirectory);
        }
        catch (Exception ex)
        {
            _transferOutput.Text = L.Instance.GetResource("Demo.Text.NetSocketPage.DownloadAsync064") + ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Text.NetSocketPage.DownloadAsync065"), ex);
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
        _transferOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferProgress066"), e.FileName)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferProgress067"), e.Progress, e.TransferredBytes, e.TotalBytes)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferProgress068"), TcpSocketClientFileSystemFeature.FileTransferBlockSize / 1024, e.TaskId);
    }

    private void OnTransferOutcome(object? sender, FileTransferOutcomeEventArgs e)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "net", e.FileName);
        _transferOutput.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome071"), (e.Success ? L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome069") : L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome070")), localPath)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome074"), (e.IsUpload ? L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome072") : L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome073")), e.TaskId, e.IsCancelled)
                               + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome075"), e.Message);
        Append($"FileTransferOutcome → {e.FileName}（{(e.Success ? L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome076") : L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome077"))}）");
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetSocketPage.OnTransferOutcome078"), e.FileName, e.Success));
        _downloadButton.IsEnabled = true;
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource("Demo.Text.NetSocketPage.Append079"), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
