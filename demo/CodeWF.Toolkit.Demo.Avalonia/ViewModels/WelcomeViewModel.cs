using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using System.Text.Json.Serialization;
using CodeWF.Toolkit.Core.FileExtensions;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Files.Extensions;
using CodeWF.Toolkit.Image;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWeaver;
using ReactiveUI;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>Live, local experiments; every output is produced by the Toolkit APIs.</summary>
public sealed class WelcomeViewModel : ReactiveObject
{
    private readonly EventBus.EventBus _bus = new();
    private readonly Action<GreetCommand> _receive;
    private bool _subscribed = true;
    private int _received;
    private bool _hasPublished;
    private int _stage = -1;
    private int _labIndex;
    private string _message = "Hello, CodeWF!";
    private string _busOutput = L.Instance.GetResource("Demo.Studio.BusReady");
    private string _integerText = "42";
    private string _byteOutput = string.Empty;
    private string _copyStatus = string.Empty;
    private int _pipelineStep;
    private string _pipelineOutput = string.Empty;
    private string? _outputDirectory;

    public WelcomeViewModel(Action<string> navigate, Action<string> togglePanel)
    {
        NavigateCommand = ReactiveCommand.Create<string>(navigate);
        CopyInstallCommand = ReactiveCommand.CreateFromTask(CopyInstallAsync);
        _receive = command =>
        {
            _received++;
            this.RaisePropertyChanged(nameof(ReceivedCount));
            BusOutput = string.Format(L.Instance.GetResource("Demo.Studio.Delivery"), command.Name, _received, DateTime.Now);
            Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.WelcomeViewModel.Content001"), command.Name));
        };
        _bus.Subscribe(_receive);
        PublishCommand = ReactiveCommand.CreateFromTask(PublishAsync);
        ToggleSubscriptionCommand = ReactiveCommand.Create(ToggleSubscription);
        EncodeCommand = ReactiveCommand.Create(Encode);
        PipelineCommand = ReactiveCommand.CreateFromTask(RunPipelineAsync);
        OpenOutputCommand = ReactiveCommand.Create(OpenOutput);
        Encode();
    }

    public ICommand NavigateCommand { get; }
    public ICommand CopyInstallCommand { get; }
    public Func<string, Task>? CopyToClipboard { private get; set; }

    private async Task CopyInstallAsync()
    {
        try
        {
            if (CopyToClipboard is null) throw new InvalidOperationException("Clipboard unavailable");
            await CopyToClipboard(InstallCommand);
            CopyStatus = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Studio.Copied");
        }
        catch
        {
            CopyStatus = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Studio.CopyFailed");
        }
    }
    public ICommand PublishCommand { get; }
    public ICommand ToggleSubscriptionCommand { get; }
    public ICommand EncodeCommand { get; }
    public ICommand PipelineCommand { get; }
    public ICommand OpenOutputCommand { get; }
    public string InstallCommand => "dotnet add package CodeWF.Toolkit.EventBus";
    public string CopyStatus { get => _copyStatus; set => this.RaiseAndSetIfChanged(ref _copyStatus, value); }
    public int LabIndex { get => _labIndex; set => this.RaiseAndSetIfChanged(ref _labIndex, value); }
    public string Message { get => _message; set => this.RaiseAndSetIfChanged(ref _message, value); }
    public string BusOutput { get => _busOutput; private set => this.RaiseAndSetIfChanged(ref _busOutput, value); }
    public string IntegerText { get => _integerText; set => this.RaiseAndSetIfChanged(ref _integerText, value); }
    public string ByteOutput { get => _byteOutput; private set => this.RaiseAndSetIfChanged(ref _byteOutput, value); }
    public string SubscriptionStatus => L.Instance.GetResource(_subscribed ? "Demo.Studio.Subscribed" : "Demo.Studio.Unsubscribed");
    public bool IsSubscribed => _subscribed;
    public int ReceivedCount => _received;
    public bool IsIntegerValid => int.TryParse(IntegerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    public string? OutputDirectory => _outputDirectory;

    public void RefreshCulture()
    {
        this.RaisePropertyChanged(nameof(SubscriptionStatus));
        if (!_hasPublished) BusOutput = L.Instance.GetResource("Demo.Studio.BusReady");
    }
    public bool IsPublishing => _stage == 0;
    public bool IsRouting => _stage == 1;
    public bool IsReceiving => _stage == 2;
    public bool IsLogging => _stage == 3;
    public int PipelineStep { get => _pipelineStep; private set => this.RaiseAndSetIfChanged(ref _pipelineStep, value); }
    public string PipelineOutput { get => _pipelineOutput; private set => this.RaiseAndSetIfChanged(ref _pipelineOutput, value); }
    public bool HasOutput => _outputDirectory is not null;

    public async Task PublishAsync()
    {
        _hasPublished = true;
        var message = Message;
        try
        {
            SetStage(0);
            await Task.Delay(130);
            SetStage(1);
            var previousCount = _received;
            _bus.Publish(new GreetCommand(message));
            await Task.Delay(130);
            if (_received > previousCount)
            {
                SetStage(2);
                await Task.Delay(180);
                SetStage(3);
            }
            else
            {
                BusOutput = string.Format(L.Instance.GetResource("Demo.Studio.NoDelivery"), message);
            }
            await Task.Delay(180);
        }
        catch (Exception ex)
        {
            BusOutput = ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Text.WelcomeViewModel.PublishAsync002") + ex.Message);
        }
        finally { SetStage(-1); }
    }

    private void SetStage(int stage)
    {
        _stage = stage;
        this.RaisePropertyChanged(nameof(IsPublishing));
        this.RaisePropertyChanged(nameof(IsRouting));
        this.RaisePropertyChanged(nameof(IsReceiving));
        this.RaisePropertyChanged(nameof(IsLogging));
    }

    public void ToggleSubscription()
    {
        if (_subscribed) _bus.Unsubscribe(_receive);
        else _bus.Subscribe(_receive);
        _subscribed = !_subscribed;
        this.RaisePropertyChanged(nameof(IsSubscribed));
        this.RaisePropertyChanged(nameof(SubscriptionStatus));
    }

    public void Encode()
    {
        if (!int.TryParse(IntegerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            ByteOutput = L.Instance.GetResource("Demo.Studio.Int32Range");
            return;
        }
        // SerializeObject writes Int32 through BinaryWriter (little endian), without a packet header.
        var bytes = value.SerializeObject();
        var restored = (int)bytes.DeserializeObject(typeof(int))!;
        ByteOutput = string.Format(L.Instance.GetResource("Demo.Studio.ByteResult"), BitConverter.ToString(bytes).Replace('-', ' '), bytes.Length, value, restored);
    }

    public async Task RunPipelineAsync()
    {
        PipelineStep = 0;
        _outputDirectory = null;
        this.RaisePropertyChanged(nameof(HasOutput));
        // Each execution owns a new directory and cannot overwrite an earlier run or user files.
        var directory = Path.Combine(Path.GetTempPath(), "CodeWF.Toolkit.Demo", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(directory);
            var student = new Student { Name = "CodeWF", Age = 10, City = "Studio" };
            if (!student.ToJson(OverviewJsonContext.Default.Student, out var json, out var jsonError)) throw new InvalidOperationException(jsonError);
            await File.WriteAllTextAsync(Path.Combine(directory, "sample.json"), json);
            PipelineStep = 1;
            PipelineOutput = "✓ JSON · sample.json";
            if (!json.JsonToYaml(out var yaml, out var yamlError)) throw new InvalidOperationException(yamlError);
            await File.WriteAllTextAsync(Path.Combine(directory, "sample.yaml"), yaml);
            PipelineStep = 2;
            PipelineOutput += "\n✓ YAML · sample.yaml";
            await Task.Run(() =>
            {
                using var client = new HttpClient();
                new SevenZipCompressor(client).Zip(
                    new[] { Path.Combine(directory, "sample.json"), Path.Combine(directory, "sample.yaml") },
                    Path.Combine(directory, "bundle.zip"), "samples");
            });
            PipelineStep = 3;
            PipelineOutput += "\n✓ ZIP · bundle.zip";
            await Task.Run(() => QrCodeGenerator.GenerateQrCode("CodeWF Toolkit", "https://github.com/dotnet9/CodeWF.Toolkit", Path.Combine(directory, "qr.png")));
            PipelineStep = 4;
            _outputDirectory = directory;
            this.RaisePropertyChanged(nameof(HasOutput));
            PipelineOutput += $"\n✓ QR · qr.png\n{directory}";
            Logger.Info(L.Instance.GetResource("Demo.Text.WelcomeViewModel.RunPipelineAsync003") + directory);
        }
        catch (Exception ex)
        {
            PipelineOutput += "\n✕ " + ex.Message;
            Logger.Error(L.Instance.GetResource("Demo.Text.WelcomeViewModel.RunPipelineAsync004") + ex.Message);
        }
    }

    private void OpenOutput()
    {
        try
        {
            if (_outputDirectory is not null)
                Process.Start(new ProcessStartInfo(_outputDirectory) { UseShellExecute = true });
        }
        catch (Exception ex) { PipelineOutput += "\n" + ex.Message; }
    }
}

[JsonSerializable(typeof(Student))]
internal partial class OverviewJsonContext : JsonSerializerContext;
