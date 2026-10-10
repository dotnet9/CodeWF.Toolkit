# CodeWF.Toolkit.EventBus 系列

轻量事件总线家族：进程内总线 + Prism / ASP.NET Core / TCP 分布式三种集成形态。

| 包 | 说明 |
|---|---|
| CodeWF.Toolkit.EventBus | 进程内总线核心：发布订阅、Command/Query（轻量 CQRS）、[EventHandler] 特性与程序集扫描、通用 IOC 委托注册 |
| CodeWF.Toolkit.EventBus.Prism | Prism/DryIoc 容器集成 |
| CodeWF.Toolkit.EventBus.AspNetCore | ASP.NET Core 集成 |
| CodeWF.Toolkit.EventBus.Socket | 基于 TCP 的分布式事件总线（心跳、断线重连、离线补发），不依赖第三方 MQ |

## 安装

```bash
dotnet add package CodeWF.Toolkit.EventBus
```

## 进程内总线

```csharp
var bus = new CodeWF.Toolkit.EventBus.EventBus();

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
- 通用 IOC 场景（无 MEDI/Prism）：`EventBusRegistrationExtensions.AddEventBus(addSingleton, addScoped, assemblies)` + `UseEventBus(resolve, assemblies)`。

## Prism 集成（CodeWF.Toolkit.EventBus.Prism）

Prism 容器内完成注册、扫描与启动（Prism.Ioc 扩展），适合 Prism.DryIoc.Avalonia 桌面应用。

## ASP.NET Core 集成（CodeWF.Toolkit.EventBus.AspNetCore）

依赖注入注册 + 请求管线启动，处理器自动发现，适合 WebAPI 后台事件。

## 分布式总线（CodeWF.Toolkit.EventBus.Socket）

```csharp
var server = new EventServer();
server.Start("127.0.0.1", 39561);

var client = new EventClient();
await client.ConnectAsync("127.0.0.1", 39561);
client.Subscribe<string>("demo.greeting", msg => Console.WriteLine(msg));
client.Publish("demo.greeting", "hello", out _);
var (result, error) = await client.QueryAsync<Ping, Pong>(ping);
```

- Pub/Sub 与 Query/Response 语义，底层传输由 NetWrapper 承担（同仓 ProjectReference）。
- 心跳保活、断线重连、离线消息补发；`EventBusOptions` 配置端口与行为。

## 注意事项

- 核心包零第三方依赖；Prism 形态引入 Prism.Core/DryIoc；分布式形态引入 NetWrapper。
- 处理器异常不会默默吞掉：发布端异步方法会抛出，注意 UI 侧捕获。
