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

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.NetWrapper.Helpers;
        using CodeWF.Toolkit.NetWrapper.Models;

        // 服务端：开启远程文件系统（默认根目录 = 进程当前目录）
        var server = new TcpSocketServer();
        var (ok, error) = await server.StartAsync("文件服务", "127.0.0.1", 39561);
        var fileSystem = server.UseFileSystem();
        fileSystem.FileTransferProgress += (_, e) =>
            Console.WriteLine($"{e.FileName} {e.Progress:F1}% ({e.BytesPerSecond / 1024 / 1024:F1} MB/s)");

        // 客户端：连接后下载（自动分块 64KB、校验、断点续传）
        var client = new TcpSocketClient();
        await client.ConnectAsync("文件服务", "127.0.0.1", 39561);

        var feature = client.UseFileSystem();
        feature.FileTransferProgress += (_, e) => Console.WriteLine($"下载 {e.FileName} {e.Progress:F1}%");
        feature.FileTransferOutcome += (_, e) => Console.WriteLine($"完成：{e.LocalFilePath}");

        await client.DownloadFileAsync("demo-download.bin", localSaveDirectory);

        // 命令下发：任意 INetObject 都会被序列化后发给对端
        await client.SendCommandAsync(new Heartbeat { TaskId = 1001 });

        client.Stop();
        await server.StopAsync();
        """;

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
        _serverState = Ui.Key("● 未启动");
        _clientState = Ui.Key("● 未连接");
        _feed = Ui.Code("等待启动…", output: true);
        _transferOutput = Ui.Code("等待下载", output: true);

        _startButton = Ui.Button("启动服务端 + 客户端", async () => await StartAsync(), primary: true);
        _stopButton = Ui.Button("停止", async () => await StopAsync());
        _downloadButton = Ui.Button("下载样本文件（分块 + 断点续传）", async () => await DownloadAsync(), primary: true);

        var server = Ui.Card("服务端 / 客户端",
            Ui.Field("监听地址", Ui.Key("127.0.0.1（回环）")),
            Ui.Field("端口", Ui.Key("临时可用端口（IpHelper.GetAvailableTcpPort）")),
            Ui.Row(_startButton, _stopButton),
            Ui.Field("服务端", _serverState),
            Ui.Field("客户端", _clientState),
            Ui.Field("客户端数", _clientCount),
            Ui.Hint("客户端上线 / 断开、错误通过 SocketClientChangedCommand / TcpClientErrorCommand 经 EventBus 广播。"));

        var messages = Ui.Card("消息与会话",
            _feed,
            Ui.Row(Ui.Button("清空会话", () => _feed.Text = string.Empty),
                Ui.Button("发送 Heartbeat", async () => await SendHeartbeatAsync())),
            Ui.Code("""
                TcpSocketServer.StartAsync(serverMark, ip, port, timeout)
                TcpSocketClient.ConnectAsync(serverMark, ip, port)
                client.SendCommandAsync(new Heartbeat { SystemId = ... })
                server.SendCommandAsync(command)     // 广播给全部客户端
                UdpSocketClient.SendAsync(command, time)
                """, output: true),
            Ui.Hint("服务端内置心跳检测：30 秒未活动的客户端由后台任务清理。"));

        var transfer = Ui.Card("远程文件系统（真实下载）",
            Ui.Field("样本大小(MB)", _sampleSize),
            _downloadButton,
            _transferOutput,
            Ui.Hint("下载即生成一个 2MB 样本文件（默认），走 FileDownloadRequest → FileChunkData → FileChunkAck → FileTransferComplete，64KB 分块、哈希校验、断点续传。"));

        Content = Ui.Page(
            Ui.Heading("NetWrapper：TCP/UDP 通信与远程文件"),
            Ui.Sub("TcpSocketServer / TcpSocketClient 封装、命令分发，内置远程文件系统与断点续传；下方在回环地址上真实互通。"),
            Ui.TwoColumns(server, messages),
            transfer);
    }

    public override string TitleKey => DemoKeys.ItemNetSocket;

    public override string DescriptionKey => DemoKeys.DescNetSocket;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("ns.conn", "连接参数", "演示用回环连接参数。",
            DemoProperty.ReadOnly("ns.host", "主机", "127.0.0.1"),
            DemoProperty.ReadOnly("ns.port", "端口", "临时可用端口"),
            DemoProperty.Number("ns.timeout", "客户端超时(秒)", 30, 5, 300, 1, "s", "服务端心跳检测超时。"),
            DemoProperty.Number("ns.chunk", "分块大小(KB)", 64, 4, 1024, 4, "KB", "FileTransferBlockSize。")),
        DemoProperty.Category("ns.file", "文件传输", "远程文件系统演示参数。",
            DemoProperty.Bool("ns.resume", "断点续传", true, "已传输字节由 .part 临时文件推断并续传。"),
            DemoProperty.ReadOnly("ns.dir", "下载目录", Path.Combine("%TEMP%", "CodeWFToolkitDemo", "net"))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "ns.timeout" && _server is not null)
        {
            Append("注意：超时时间需在启动前设置（TimeOut 属性只读）。");
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
            var (started, startError) = await _server.StartAsync("演示文件服务", "127.0.0.1", _port);
            if (!started)
            {
                Append("服务端启动失败：" + startError);
                await StopAsync();
                return;
            }

            _server.UseFileSystem(Directory.GetCurrentDirectory());
            _serverState.Text = $"● 运行中（127.0.0.1:{_port}）";
            Append($"server.StartAsync(\"演示文件服务\", \"127.0.0.1\", {_port}) → 成功");
            Append($"server.UseFileSystem(\"{Directory.GetCurrentDirectory()}\")");

            _client = new TcpSocketClient();
            var (connected, connectError) = await _client.ConnectAsync("演示文件服务", "127.0.0.1", _port);
            if (!connected)
            {
                Append("客户端连接失败：" + connectError);
                await StopAsync();
                return;
            }

            _clientState.Text = "● 已连接";
            _clientCount.Text = _server.Clients.Count.ToString();
            Append($"client.ConnectAsync → 已连接（本地端点 {_client.LocalEndPoint}）");

            _clientFileSystem = _client.UseFileSystem();
            _clientFileSystem.FileTransferProgress += OnTransferProgress;
            _clientFileSystem.FileTransferOutcome += OnTransferOutcome;

            Logger.Info($"NetWrapper 演示：TCP 已互通 127.0.0.1:{_port}");
        }
        catch (Exception ex)
        {
            Append("启动异常：" + ex.Message);
            Logger.Error("NetWrapper 演示启动失败", ex);
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

        _serverState.Text = "● 未启动";
        _clientState.Text = "● 未连接";
        _clientCount.Text = "0";
        Append("client.Stop() / await server.StopAsync()");
        Logger.Warn("NetWrapper 演示已停止");
    }

    private async Task SendHeartbeatAsync()
    {
        if (_client is null || _server is null)
        {
            Append("请先启动服务端与客户端。");
            return;
        }

        try
        {
            await _client.SendCommandAsync(new Heartbeat { TaskId = Random.Shared.Next(1, 10_000) });
            Append("client.SendCommandAsync(new Heartbeat { ... }) → 已发送");
            Logger.Info("NetWrapper 演示：心跳已发送");
        }
        catch (Exception ex)
        {
            Append("发送失败：" + ex.Message);
            Logger.Error("NetWrapper 演示心跳发送失败", ex);
        }
    }

    private async Task DownloadAsync()
    {
        if (_client is null)
        {
            Append("请先启动服务端与客户端。");
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
            _transferOutput.Text = $"准备样本：{fileName}（{new FileInfo(_sampleFilePath).Length:N0} 字节）\n"
                                   + "client.DownloadFileAsync(...) → 等待服务端分块…";
            Append($"client.DownloadFileAsync(\"{_sampleFilePath}\", \"{downloadDirectory}\")");

            await _client.DownloadFileAsync(_sampleFilePath, downloadDirectory);
        }
        catch (Exception ex)
        {
            _transferOutput.Text = "下载失败：" + ex.Message;
            Logger.Error("NetWrapper 演示下载失败", ex);
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
        _transferOutput.Text = $"下载 {e.FileName}\n"
                               + $"进度 {e.Progress:F1}% · {e.TransferredBytes:N0} / {e.TotalBytes:N0} 字节\n"
                               + $"分块大小 {TcpSocketClientFileSystemFeature.FileTransferBlockSize / 1024} KB · 任务号 {e.TaskId}";
    }

    private void OnTransferOutcome(object? sender, FileTransferOutcomeEventArgs e)
    {
        var localPath = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "net", e.FileName);
        _transferOutput.Text = $"下载{(e.Success ? "完成" : "失败")}：{localPath}\n"
                               + $"方向 {(e.IsUpload ? "上传" : "下载")} · 任务号 {e.TaskId} · 取消 {e.IsCancelled}\n"
                               + $"结果：{e.Message}";
        Append($"FileTransferOutcome → {e.FileName}（{(e.Success ? "成功" : "失败")}）");
        Logger.Info($"NetWrapper 演示下载结束：{e.FileName}，成功={e.Success}");
        _downloadButton.IsEnabled = true;
    }

    private void Append(string line)
    {
        var current = _feed.Text;
        _feed.Text = string.IsNullOrEmpty(current) || current.StartsWith("等待启动", StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }
}
