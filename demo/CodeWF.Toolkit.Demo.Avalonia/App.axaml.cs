using Avalonia.Controls;
using CodeWF.Toolkit.Demo.Avalonia.Infrastructure;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace CodeWF.Toolkit.Demo.Avalonia;

public class App : PrismApplication
{
    protected override Window CreateShell()
    {
        // Lang 资源必须先于 Shell XAML 解析注册（I18n 标记扩展在解析期求值）。
        DemoBootstrapper.Initialize();
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
