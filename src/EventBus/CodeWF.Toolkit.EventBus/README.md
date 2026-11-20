# CodeWF.Toolkit.EventBus

轻量进程内事件总线：发布订阅、Command/Query 读写分离、`[EventHandler]` 特性注册与程序集扫描，适用于桌面与应用服务间模块解耦，零第三方依赖。Lightweight in-process event bus with pub/sub, CQRS-style commands and queries, zero third-party dependencies.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.EventBus?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.EventBus
```

零第三方依赖（仅本仓 `CodeWF.Toolkit.Lang` 随包带入）；目标框架 net10.0。容器/分布式形态见 [Prism 集成](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Prism)、[ASP.NET Core 集成](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.AspNetCore)、[TCP 分布式总线](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Socket)。

## 快速使用

```csharp
using CodeWF.Toolkit.EventBus;

var bus = new EventBus();

// lambda 订阅
bus.Subscribe<GreetCommand>(cmd => Console.WriteLine(cmd.Name));
bus.Publish(new GreetCommand("hi"));
await bus.PublishAsync(new GreetCommand("hi"));

// 查询：处理器设置 query.Result
bus.Subscribe(new TimeQueryHandler());
var query = new TimeQuery();
bus.Query(query);
```

```csharp
public sealed class TimeQueryHandler
{
    [EventHandler]
    private void ReceiveTimeQueryAsync(TimeQuery query) => query.Result = DateTime.Now;
}
```

- `Command` / `Query<TResponse>` 基类区分写命令与读查询。
- `[EventHandler(Order=n)]` 控制同命令多处理器顺序；`Subscribe(Assembly[])` 扫描实例处理器，`RegisterServiceHandlerAction` 提供构造依赖。
- 通用 IOC 场景（无 MEDI/Prism）：`EventBusRegistrationExtensions.AddEventBus(...)` + `UseEventBus(...)`。

## 注意事项

- 处理器异常不会默默吞掉：发布端异步方法会抛出，注意 UI 侧捕获。

## 文档

- 完整说明文档（含全部集成形态）：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.EventBus%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
