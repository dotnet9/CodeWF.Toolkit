using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CodeWF.Toolkit.EventBus;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWeaver.Base;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>演示用数据模型（文件格式页的往返对象）。</summary>
public sealed class Student
{
    public string Name { get; set; } = L.Instance.GetResource("Demo.Content.ToolkitModels.Name.Alice");

    public int Age { get; set; } = 20;

    public string City { get; set; } = L.Instance.GetResource("Demo.Content.ToolkitModels.City.Chengdu");
}

/// <summary>[NetHead] / [NetFieldOrder] 特性驱动的二进制协议包。</summary>
[NetHead(0x01, 1)]
public sealed class SamplePacket : INetObject
{
    [NetFieldOrder(0)]
    public int Id { get; set; } = 42;

    [NetFieldOrder(1)]
    public float Score { get; set; } = 1.0f;

    [NetFieldOrder(2)]
    public string Title { get; set; } = "hello";
}

/// <summary>总线演示命令。</summary>
public sealed class GreetCommand(string name) : Command
{
    public string Name { get; } = name;
}

/// <summary>总线演示查询。</summary>
public sealed class TimeQuery : Query<DateTime>
{
    public override DateTime Result { get; set; }
}

/// <summary>[EventHandler] 扫描式处理器（随程序集扫描注册）。</summary>
[Event]
public sealed class TimeQueryHandler
{
    [EventHandler]
    private void ReceiveTimeQuery(TimeQuery query)
    {
        query.Result = DateTime.Now;
    }
}

/// <summary>
/// 日志面板：订阅 Toolkit 的 <see cref="LogEventFeed"/>（UI 事件流），按级别过滤后追加到可绑定集合。
/// 事件回调来自后台线程，这里仅做集合追加（Avalonia 对集合追加是线程安全的）。
/// </summary>
public sealed class LogStream : IDisposable
{
    private readonly List<CodeWFLogEvent> _all = [];
    private readonly IDisposable? _subscription;
    private bool _disposed;

    public LogStream(int capacity = 300)
    {
        Capacity = capacity;
        _subscription = Logger.Events.Subscribe(OnEvent);
    }

    public int Capacity { get; }

    public ObservableCollection<string> Lines { get; } = [];

    /// <summary>最低显示级别（Trace 为全部）。</summary>
    public Microsoft.Extensions.Logging.LogLevel MinimumLevel { get; set; } =
        Microsoft.Extensions.Logging.LogLevel.Trace;

    public void Clear()
    {
        lock (_all)
        {
            _all.Clear();
        }

        Lines.Clear();
    }

    public void Dispose()
    {
        _disposed = true;
        _subscription?.Dispose();
    }

    private void OnEvent(CodeWFLogEvent logEvent)
    {
        // ObservableCollection notifications must be raised on the UI thread.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnEvent(logEvent));
            return;
        }
        if (_disposed) return;
        lock (_all)
        {
            _all.Add(logEvent);
            if (_all.Count > Capacity)
            {
                _all.RemoveRange(0, _all.Count - Capacity);
            }
        }

        if (logEvent.Level < MinimumLevel)
        {
            return;
        }

        Lines.Add(Format(logEvent));
        while (Lines.Count > Capacity) Lines.RemoveAt(0);
    }

    private static string Format(CodeWFLogEvent logEvent)
    {
        var level = logEvent.LocalizedLevel ?? logEvent.Level.ToString();
        return $"{logEvent.Timestamp:HH:mm:ss.fff} [{level}] {logEvent.Message}";
    }
}
