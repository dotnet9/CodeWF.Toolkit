using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Globalization;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Lang.Json;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Infrastructure;

/// <summary>
/// 演示台启动引导：注册 Lang 资源（Json 后端，输出目录 I18n\）并初始化 Toolkit 日志核心。
/// 必须在 MainWindow 的 XAML 解析之前调用。
/// </summary>
public static class DemoBootstrapper
{
    private static bool _initialized;

    public const string DefaultCultureName = "zh-CN";

    /// <summary>语言资源目录（Json 后端按目录递归扫描）。</summary>
    public static string I18nFolder { get; } = Path.Combine(AppContext.BaseDirectory, "I18n");

    /// <summary>日志目录。</summary>
    public static string LogFolder { get; } = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "logs");

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        I18nManager.Instance.Register(
            new JsonLangPlugin { ResourceFolder = I18nFolder },
            new CultureInfo(DefaultCultureName));

        Logger.TryInitialize(new LoggerOptions
        {
            File = new FileLogOptions { DirectoryPath = LogFolder },
        });

        Logger.Info(L.Instance.GetResource("Demo.Content.DemoBootstrapper.Initialize.CodeWFToolkitStartedLogDirectory") + LogFolder);
    }
}
