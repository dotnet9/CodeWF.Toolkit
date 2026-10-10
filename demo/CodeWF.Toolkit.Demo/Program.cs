// CodeWF.Toolkit 控制台演示。
// 直接运行进入菜单交互；附加 --smoke 参数时按顺序执行全部演示后退出（供 CI / AOT 冒烟使用）。
using System.Text;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.EventBus;
using CodeWF.Toolkit.EventBus.Socket;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWeaver;
using CodeWF.Toolkit.NetWeaver.Base;

namespace CodeWF.Toolkit.Demo;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var smoke = args.Any(a => string.Equals(a, "--smoke", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine("=== CodeWF.Toolkit 演示 ===");
        try
        {
            RunLoggingDemo();
            RunEventBusDemo();
            await RunSocketBusDemoAsync();
            RunSerializationDemo();

            if (smoke)
            {
                Console.WriteLine("=== 冒烟演示全部通过 ===");
                return 0;
            }

            return await RunMenuAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[异常] {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunMenuAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1) 重新运行日志演示");
            Console.WriteLine("2) 重新运行进程内事件总线演示");
            Console.WriteLine("3) 重新运行分布式事件总线演示");
            Console.WriteLine("4) 重新运行序列化演示");
            Console.WriteLine("5) 检查 GitHub 最新版本");
            Console.WriteLine("0) 退出");
            Console.Write("请选择: ");
            var input = Console.ReadLine();
            switch (input?.Trim())
            {
                case "1": RunLoggingDemo(); break;
                case "2": RunEventBusDemo(); break;
                case "3": await RunSocketBusDemoAsync(); break;
                case "4": RunSerializationDemo(); break;
                case "5": await RunUpdateCheckingDemoAsync(); break;
                case "0" or null or "": return 0;
                default: Console.WriteLine("无效选择。"); break;
            }
        }
    }

    private static void RunLoggingDemo()
    {
        Console.WriteLine();
        Console.WriteLine("--- 日志（控制台 + 文件轮转）---");
        var logDirectory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "logs");
        Logger.TryInitialize(new LoggerOptions
        {
            File = new FileLogOptions
            {
                DirectoryPath = logDirectory,
            },
        });
        Logger.Info("演示：一条普通信息日志");
        Logger.Warn("演示：一条警告日志");
        Logger.Error("演示：一条错误日志", new InvalidOperationException("示例异常"));
        Console.WriteLine($"文件日志目录：{logDirectory}");
    }

    private static void RunEventBusDemo()
    {
        Console.WriteLine();
        Console.WriteLine("--- 进程内事件总线（发布订阅 + 命令 + 查询）---");
        var bus = new CodeWF.Toolkit.EventBus.EventBus();
        bus.Subscribe<GreetCommand>(cmd => Console.WriteLine($"[订阅者] 收到命令：{cmd.Name}"));
        bus.Publish(new GreetCommand("CodeWF"));
        bus.PublishAsync(new GreetCommand("Toolkit")).GetAwaiter().GetResult();

        bus.Subscribe(new TimeQueryHandler());
        var query = new TimeQuery();
        bus.Query(query);
        Console.WriteLine($"[查询] 服务器时间：{query.Result:HH:mm:ss.fff}");
    }

    private static async Task RunSocketBusDemoAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- 分布式事件总线（TCP 本机互通）---");
        const int port = 39561;
        using var server = new EventServer();
        server.Start("127.0.0.1", port);

        using var client = new EventClient();
        var connected = await client.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(5));
        Console.WriteLine($"客户端连接：{(connected ? "成功" : "失败")}");
        if (!connected)
        {
            server.Stop();
            return;
        }

        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Subscribe<string>("demo.greeting", msg => received.TrySetResult(msg));
        // 给订阅注册留出同步时间
        await Task.Delay(200);
        client.Publish("demo.greeting", "hello from client", out _);

        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Console.WriteLine($"[服务端转发] 收到：{message}");

        client.Disconnect();
        server.Stop();
        Console.WriteLine("演示结束，连接已关闭。");
    }

    private static void RunSerializationDemo()
    {
        Console.WriteLine();
        Console.WriteLine("--- 二进制序列化（TCP/UDP 包）---");
        var packet = new SamplePacket
        {
            Id = 9527,
            Score = 99.5f,
            Title = "序列化往返",
        };
        var buffer = packet.Serialize(0);
        var restored = buffer.Deserialize<SamplePacket>();
        Console.WriteLine($"原始：Id={packet.Id}, Score={packet.Score}, Title={packet.Title}");
        Console.WriteLine($"还原：Id={restored.Id}, Score={restored.Score}, Title={restored.Title}");
        Console.WriteLine(restored.Id == packet.Id && Math.Abs(restored.Score - packet.Score) < 0.001 &&
                          restored.Title == packet.Title
            ? "往返校验：一致"
            : "往返校验：不一致！");
    }

    private static async Task RunUpdateCheckingDemoAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- GitHub Release 更新检查 ---");
        var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
        var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
        if (!result.Succeeded)
        {
            Console.WriteLine($"检查失败：{result.Error}");
        }
        else if (result.Update is null)
        {
            Console.WriteLine("已是最新版本。");
        }
        else
        {
            Console.WriteLine($"发现新版本：{result.Update.Version}（{result.Update.Tag}）");
        }
    }
}

public sealed class GreetCommand(string name) : Command
{
    public string Name { get; } = name;
}

public sealed class TimeQuery : Query<DateTime>
{
    public override DateTime Result { get; set; }
}

public sealed class TimeQueryHandler
{
    [EventHandler]
    private void ReceiveTimeQueryAsync(TimeQuery query)
    {
        query.Result = DateTime.Now;
    }
}

[NetHead(0x01, 1)]
public sealed class SamplePacket : INetObject
{
    [NetFieldOrder(0)]
    public int Id { get; set; }

    [NetFieldOrder(1)]
    public float Score { get; set; }

    [NetFieldOrder(2)]
    public string Title { get; set; } = string.Empty;
}
