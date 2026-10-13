using System.Globalization;
using System.Windows.Input;
using CodeWF.Avalonia.Lang;
using ReactiveUI;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

public sealed record HelperExample(string Key, string Input, string Code, Func<string, Task<string>> Execute, bool Smoke = true);

/// <summary>Demo-only interaction state; production APIs remain in their own Toolkit packages.</summary>
public sealed class HelperDemoViewModel : ReactiveObject
{
    private string _input;
    private string _output = I18nManager.Instance.GetResource("Demo.Catalog.Ready");
    private bool _busy;

    public HelperDemoViewModel(HelperExample example)
    {
        Example = example;
        _input = example.Input;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
    }

    public HelperExample Example { get; }
    public string Input { get => _input; set => this.RaiseAndSetIfChanged(ref _input, value); }
    public string Output { get => _output; private set => this.RaiseAndSetIfChanged(ref _output, value); }
    public bool IsBusy { get => _busy; private set => this.RaiseAndSetIfChanged(ref _busy, value); }
    public ICommand RunCommand { get; }
    public bool Succeeded { get; private set; }

    public async Task RunAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            Output = await Example.Execute(Input);
            Succeeded = true;
        }
        catch (Exception ex)
        {
            Succeeded = false;
            Output = string.Format(CultureInfo.CurrentCulture, I18nManager.Instance.GetResource("Demo.Catalog.Failed"), ex.Message);
        }
        finally { IsBusy = false; }
    }
}
