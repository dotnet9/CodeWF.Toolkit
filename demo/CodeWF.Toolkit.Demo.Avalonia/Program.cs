using Avalonia;
using ReactiveUI.Avalonia;

namespace CodeWF.Toolkit.Demo.Avalonia;

internal static class Program
{
    // 附加 --smoke 时：启动后自动跑一遍演示台冒烟并退出（CI / AOT 冒烟）。
    public static bool IsSmokeMode { get; private set; }

    public static string? CaptureDirectory { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        IsSmokeMode = args.Any(a => string.Equals(a, "--smoke", StringComparison.OrdinalIgnoreCase));
        CaptureDirectory = args.SkipWhile(a => a != "--capture").Skip(1).FirstOrDefault();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI(_ => { })
            .LogToTrace();
}
