using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using System.Collections.ObjectModel;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.NetWrapper.FileSystem;
using CodeWF.Toolkit.NetWrapper.Helpers;
using CodeWF.Toolkit.NetWrapper.Models;
using CodeWF.Toolkit.NetWrapper.Response;
using ReactiveUI;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

/// <summary>真实本地文件会话，所有操作仅作用于新建的演示目录。</summary>
public sealed class FileSystemDemoViewModel : ReactiveObject
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private TcpSocketServer? _server;
    private TcpSocketClient? _client;
    private TcpSocketServerFileSystemFeature? _serverFeature;
    private TcpSocketClientFileSystemFeature? _clientFeature;
    private IDisposable? _registration;
    private CancellationTokenSource? _transferCancellation;
    private TaskCompletionSource<BrowseFileSystemResponse>? _browse;
    private TaskCompletionSource<CreateDirectoryResponse>? _create;
    private TaskCompletionSource<DeletePathResponse>? _delete;
    private TaskCompletionSource<FileTransferOutcomeEventArgs>? _transfer;
    private string _directory = "";
    private string _output = "";
    private bool _running, _busy;
    private double _progress;
    private bool _cancelAfterFirstBlock;

    public FileSystemDemoViewModel()
    {
        Source = new FlatTreeDataGridSource<TablePreviewRow>(Rows)
        {
            Columns = { new TextColumn<TablePreviewRow, string>("Name", r => r.Name), new TextColumn<TablePreviewRow, string>("Path", r => r.Value) }
        };
        StartCommand = ReactiveCommand.CreateFromTask(StartAsync);
        StopCommand = ReactiveCommand.CreateFromTask(StopAsync);
        BrowseCommand = ReactiveCommand.CreateFromTask(BrowseAsync);
        CreateCommand = ReactiveCommand.CreateFromTask(CreateAsync);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync);
        UploadCommand = ReactiveCommand.CreateFromTask(() => TransferAsync(true));
        DownloadCommand = ReactiveCommand.CreateFromTask(() => TransferAsync(false));
        CancelCommand = ReactiveCommand.Create(CancelTransfer);
    }

    public bool IsRunning { get => _running; private set { this.RaiseAndSetIfChanged(ref _running, value); this.RaisePropertyChanged(nameof(CanOperate)); this.RaisePropertyChanged(nameof(CanStart)); } }
    public bool IsBusy { get => _busy; private set { this.RaiseAndSetIfChanged(ref _busy, value); this.RaisePropertyChanged(nameof(CanOperate)); this.RaisePropertyChanged(nameof(CanStart)); } }
    public bool CanOperate => IsRunning && !IsBusy;
    public bool CanStart => !IsRunning && !IsBusy;
    public bool Succeeded { get; private set; }
    public bool LastTransferWasCancelled { get; private set; }
    public string Output { get => _output; private set => this.RaiseAndSetIfChanged(ref _output, value); }
    public double Progress { get => _progress; private set => this.RaiseAndSetIfChanged(ref _progress, value); }
    public int TransferSize { get; private set; } = 4 * 1024 * 1024;
    public IReadOnlyList<PropertyItem> BuildProperties() => [new PropertyItem
    {
        Id = nameof(TransferSize), Name = nameof(TransferSize), Kind = PropertyItemKind.Number,
        Value = (decimal)TransferSize, DefaultValue = 4m * 1024 * 1024, Minimum = 1, Maximum = 16 * 1024 * 1024, Increment = 1024
    }];
    public void Commit(PropertyValueChange change)
    {
        if (change.Item.Id == nameof(TransferSize)) TransferSize = Math.Clamp(Convert.ToInt32(change.NewValue), 1, 16 * 1024 * 1024);
    }
    public string RootDirectory => Path.Combine(_directory, "server");
    public ObservableCollection<TablePreviewRow> Rows { get; } = [];
    public FlatTreeDataGridSource<TablePreviewRow> Source { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand BrowseCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand UploadCommand { get; }
    public ICommand DownloadCommand { get; }
    public ICommand CancelCommand { get; }

    public Task StartAsync() => ExecuteAsync(async () =>
    {
        if (IsRunning) return;
        _directory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "file-session", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(RootDirectory);
        await File.WriteAllTextAsync(Path.Combine(RootDirectory, "sample.txt"), "CodeWF remote file system");
        _server = new(); _client = new();
        _serverFeature = new(_server) { RootDirectory = RootDirectory };
        _clientFeature = new(_client);
        _clientFeature.FileTransferProgress += OnProgress;
        _clientFeature.FileTransferOutcome += (_, e) => _transfer?.TrySetResult(e);
        _registration = _client.RegisterCommandHandler(command =>
        {
            if (command.IsCommand<BrowseFileSystemResponse>()) _browse?.TrySetResult(command.GetCommand<BrowseFileSystemResponse>());
            else if (command.IsCommand<CreateDirectoryResponse>()) _create?.TrySetResult(command.GetCommand<CreateDirectoryResponse>());
            else if (command.IsCommand<DeletePathResponse>()) _delete?.TrySetResult(command.GetCommand<DeletePathResponse>());
            else return Task.FromResult(false);
            return Task.FromResult(true);
        });
        try
        {
            var started = await _server.StartAsync("file-demo", "127.0.0.1", 0);
            if (!started.IsSuccess) throw new InvalidOperationException(started.ErrorMessage);
            var port = ((IPEndPoint)_server.Server!.LocalEndPoint!).Port;
            var connected = await _client.ConnectAsync("file-demo", "127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
            if (!connected.IsSuccess) throw new InvalidOperationException(connected.ErrorMessage);
            IsRunning = true;
            Output = $"127.0.0.1:{port}\nRootDirectory = {RootDirectory}";
            await BrowseCoreAsync();
        }
        catch { await StopCoreAsync(); throw; }
    });

    public Task BrowseAsync() => ExecuteAsync(BrowseCoreAsync);
    private async Task BrowseCoreAsync()
    {
        RequireSession();
        _browse = NewCompletion<BrowseFileSystemResponse>();
        await _clientFeature!.BrowseFileSystemAsync(RootDirectory);
        var response = await _browse.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Rows.Clear();
        foreach (var entry in response.Entries ?? []) Rows.Add(new(entry.Name, Path.Combine(RootDirectory, entry.Name)));
        Output += $"\nBrowseFileSystemAsync: {response.TotalCount}";
    }
    public Task CreateAsync() => ExecuteAsync(async () =>
    {
        RequireSession(); _create = NewCompletion<CreateDirectoryResponse>();
        await _clientFeature!.CreateDirectoryAsync(Path.Combine(RootDirectory, "demo-folder"));
        var response = await _create.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (!response.Success) throw new IOException(response.Message);
        Output += "\nCreateDirectoryAsync: demo-folder";
        await BrowseCoreAsync();
    });
    public Task DeleteAsync() => ExecuteAsync(async () =>
    {
        RequireSession(); _delete = NewCompletion<DeletePathResponse>();
        await _clientFeature!.DeletePathAsync(Path.Combine(RootDirectory, "demo-folder"), true);
        var response = await _delete.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (!response.Success) throw new IOException(response.Message);
        Output += "\nDeletePathAsync: demo-folder";
        await BrowseCoreAsync();
    });

    public Task TransferAsync(bool upload, int? sampleSizeBytes = null, bool cancelAfterFirstBlock = false) => ExecuteAsync(async () =>
    {
        RequireSession();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        _transferCancellation = cancellation;
        _cancelAfterFirstBlock = cancelAfterFirstBlock;
        LastTransferWasCancelled = false;
        _transfer = NewCompletion<FileTransferOutcomeEventArgs>();
        Progress = 0;
        var local = Path.Combine(_directory, "client");
        Directory.CreateDirectory(local);
        var remotePath = Path.Combine(RootDirectory, "payload.bin");
        try
        {
            if (upload)
            {
                var path = Path.Combine(local, "payload.bin");
                if (!File.Exists(path) || new FileInfo(path).Length != (sampleSizeBytes ?? TransferSize))
                    await Task.Run(() => File.WriteAllBytes(path, RandomNumberGenerator.GetBytes(sampleSizeBytes ?? TransferSize)), cancellation.Token);
                await _clientFeature!.UploadFileAsync(path, remotePath, cancellation.Token);
            }
            else await _clientFeature!.DownloadFileAsync(remotePath, Path.Combine(local, "download"), cancellation.Token);
            var result = await _transfer.Task.WaitAsync(TimeSpan.FromSeconds(30));
            if (result.IsCancelled) { LastTransferWasCancelled = true; Output += "\n" + I18nManager.Instance.GetResource(DemoResources.Catalog.FileCancelled); return; }
            if (!result.Success) throw new IOException(result.Message);
            var source = Path.Combine(local, "payload.bin");
            var target = upload ? remotePath : Path.Combine(local, "download", "payload.bin");
            if (!File.Exists(source) || !File.Exists(target) || !await SameHashAsync(source, target))
                throw new InvalidDataException("Transferred file hash did not match.");
            Output += $"\n{(upload ? "UploadFileAsync" : "DownloadFileAsync")}: SHA256 = OK\n{target}";
            await BrowseCoreAsync();
        }
        finally { _transferCancellation = null; }
    });

    private static async Task<bool> SameHashAsync(string a, string b)
    {
        await using var left = File.OpenRead(a);
        await using var right = File.OpenRead(b);
        var leftHash = await SHA256.HashDataAsync(left);
        var rightHash = await SHA256.HashDataAsync(right);
        return leftHash.SequenceEqual(rightHash);
    }
    private void OnProgress(object? sender, FileTransferProgressEventArgs e)
    {
        if (_cancelAfterFirstBlock && e.TransferredBytes > 0)
        {
            _cancelAfterFirstBlock = false;
            CancelTransfer();
        }
        Dispatcher.UIThread.Post(() => Progress = e.Progress);
    }
    public void CancelTransfer() => _transferCancellation?.Cancel();
    public async Task StopAsync()
    {
        CancelTransfer();
        await ExecuteAsync(StopCoreAsync);
    }
    private async Task StopCoreAsync()
    {
        if (_client is not null) await _client.StopAsync();
        if (_server is not null) await _server.StopAsync();
        _registration?.Dispose(); _clientFeature?.Dispose(); _serverFeature?.Dispose();
        _registration = null; _clientFeature = null; _serverFeature = null; _client = null; _server = null;
        IsRunning = false;
        Output += "\n" + I18nManager.Instance.GetResource(DemoResources.Catalog.FileStopped);
    }
    private async Task ExecuteAsync(Func<Task> action)
    {
        await _gate.WaitAsync();
        IsBusy = true; Succeeded = false;
        try { await action(); Succeeded = true; }
        catch (OperationCanceledException) { LastTransferWasCancelled = true; Output += "\n" + I18nManager.Instance.GetResource(DemoResources.Catalog.FileCancelled); }
        catch (Exception ex) { Output += "\n" + string.Format(I18nManager.Instance.GetResource(DemoResources.Catalog.Failed), ex.Message); }
        finally { IsBusy = false; _gate.Release(); }
    }
    private void RequireSession() { if (!IsRunning) throw new InvalidOperationException(I18nManager.Instance.GetResource(DemoResources.Catalog.FileStartFirst)); }
    private static TaskCompletionSource<T> NewCompletion<T>() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
