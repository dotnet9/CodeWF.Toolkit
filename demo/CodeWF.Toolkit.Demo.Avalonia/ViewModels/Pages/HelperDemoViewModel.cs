using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using System.Globalization;
using System.Windows.Input;
using CodeWF.Avalonia.Lang;
using ReactiveUI;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

public sealed record HelperExample(string Key, string Input, string Code, Func<string, Task<string>> Execute, bool Smoke = true);

/// <summary>单个帮助类的输入、执行与结果状态，实际功能调用对应 Toolkit 包。</summary>
public sealed class HelperDemoViewModel : ReactiveObject
{
    private string _input;
    private string _output = I18nManager.Instance.GetResource(DemoResources.Catalog.Ready);
    private bool _busy;
    private PropertyItem? _inputProperty;

    public HelperDemoViewModel(HelperExample example)
    {
        Example = example;
        _input = example.Input;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
    }

    public HelperExample Example { get; }
    public string Input
    {
        get => _input;
        set
        {
            this.RaiseAndSetIfChanged(ref _input, value);
            if (_inputProperty is not null) _inputProperty.Value = value;
        }
    }
    public string Output { get => _output; private set => this.RaiseAndSetIfChanged(ref _output, value); }
    public bool IsBusy { get => _busy; private set => this.RaiseAndSetIfChanged(ref _busy, value); }
    public ICommand RunCommand { get; }
    public bool Succeeded { get; private set; }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        _inputProperty = new PropertyItem
        {
            Id = nameof(Input), Name = I18nManager.Instance.GetResource(DemoResources.Catalog.Input),
            Kind = PropertyItemKind.MultilineText, Value = Input, DefaultValue = Example.Input
        };
        return [_inputProperty];
    }

    public void Commit(PropertyValueChange change)
    {
        if (change.Item.Id == nameof(Input)) Input = change.NewValue?.ToString() ?? "";
    }

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
            Output = string.Format(CultureInfo.CurrentCulture, I18nManager.Instance.GetResource(DemoResources.Catalog.Failed), ex.Message);
        }
        finally { IsBusy = false; }
    }
}
