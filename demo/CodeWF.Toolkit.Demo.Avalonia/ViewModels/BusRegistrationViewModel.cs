using System.Windows.Input;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.EventBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prism.DryIoc;
using ReactiveUI;
using AspNetBus = CodeWF.Toolkit.EventBus.AspNetCore.EventBusExtensions;
using PrismBus = CodeWF.Toolkit.EventBus.Prism.EventBusExtensions;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>Runs the actual hosting extensions without starting an HTTP listener.</summary>
public sealed class BusRegistrationViewModel : ReactiveObject
{
    private string _prismOutput = string.Empty;
    private string _aspNetOutput = string.Empty;

    public BusRegistrationViewModel()
    {
        PrismCommand = ReactiveCommand.Create(() => RunPrism());
        AspNetCommand = ReactiveCommand.Create(() => RunAspNetCore());
    }

    public ICommand PrismCommand { get; }
    public ICommand AspNetCommand { get; }
    public string PrismOutput { get => _prismOutput; private set => this.RaiseAndSetIfChanged(ref _prismOutput, value); }
    public string AspNetOutput { get => _aspNetOutput; private set => this.RaiseAndSetIfChanged(ref _aspNetOutput, value); }

    public bool RunPrism()
    {
        try
        {
            using var container = new DryIoc.Container();
            var registry = new DryIocContainerExtension(container);
            PrismBus.AddEventBus(registry, typeof(TimeQueryHandler).Assembly);
            PrismBus.UseEventBus(container, typeof(TimeQueryHandler).Assembly);
            var query = new TimeQuery();
            DryIoc.Resolver.Resolve<IEventBus>(container).Query(query);
            if (query.Result == default) throw new InvalidOperationException("Scanned query handler did not run.");
            PrismOutput = $"✓ AddEventBus → UseEventBus → TimeQuery\n{query.Result:O}";
            return true;
        }
        catch (Exception ex)
        {
            PrismOutput = "✕ " + ex.Message;
            return false;
        }
    }

    public bool RunAspNetCore()
    {
        try
        {
            var services = new ServiceCollection();
            AspNetBus.AddEventBus(services, typeof(TimeQueryHandler).Assembly);
            using var provider = services.BuildServiceProvider();
            AspNetBus.UseEventBus(new ApplicationBuilder(provider), typeof(TimeQueryHandler).Assembly);
            var query = new TimeQuery();
            provider.GetRequiredService<IEventBus>().Query(query);
            if (query.Result == default) throw new InvalidOperationException("Scanned query handler did not run.");
            AspNetOutput = $"✓ AddEventBus → UseEventBus → TimeQuery\n{query.Result:O}";
            return true;
        }
        catch (Exception ex)
        {
            AspNetOutput = "✕ " + ex.Message;
            return false;
        }
    }
}
