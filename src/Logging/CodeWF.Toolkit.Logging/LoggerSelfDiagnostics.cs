using LibraryResources = global::CodeWF.Toolkit.Logging.I18n.ResourceKeys;
using System.Diagnostics;

namespace CodeWF.Toolkit.Logging;

internal static class LoggerSelfDiagnostics
{
    private static readonly TimeSpan ConsoleReportInterval = TimeSpan.FromSeconds(30);
    private static long _lastConsoleReportTicks;

    public static void Report(string message, Exception exception)
    {
        Debug.WriteLine($"{message}{Environment.NewLine}{exception}");

        var nowTicks = DateTimeOffset.UtcNow.UtcTicks;
        var lastTicks = Interlocked.Read(ref _lastConsoleReportTicks);
        if (nowTicks - lastTicks < ConsoleReportInterval.Ticks ||
            Interlocked.CompareExchange(ref _lastConsoleReportTicks, nowTicks, lastTicks) != lastTicks)
            return;

        try
        {
            Console.Error.WriteLine(global::CodeWF.Toolkit.Logging.LibraryText.Format(LibraryResources.Diagnostics.Failure, message));
        }
        catch
        {
            // 日志组件最后的兜底出口不可再向外抛出异常。
        }
    }
}
