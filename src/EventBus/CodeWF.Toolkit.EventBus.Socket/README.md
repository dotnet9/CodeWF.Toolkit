# CodeWF.Toolkit.EventBus.Socket

基于 TCP 的分布式事件总线：Pub/Sub、Query/Response、心跳保活、断线重连与离线消息补发，不依赖第三方消息队列。Distributed event bus over TCP with heartbeat, reconnect and offline replay, without third-party MQ.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.EventBus.Socket?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Socket)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.EventBus.Socket
```

依赖本仓 `CodeWF.Toolkit.EventBus`、`CodeWF.Toolkit.NetWrapper`、`CodeWF.Toolkit.Lang`（随包自动安装）；目标框架 net10.0。

## 快速使用

```csharp
var server = new EventServer();
server.Start("127.0.0.1", 39561);

var client = new EventClient();
await client.ConnectAsync("127.0.0.1", 39561);

client.Subscribe<string>("demo.greeting", msg => Console.WriteLine(msg));
client.Publish("demo.greeting", "hello", out _);
var (result, error) = await client.QueryAsync<Ping, Pong>(ping);
```

- Pub/Sub 与 Query/Response 语义，底层传输由 NetWrapper 承担。
- 心跳保活、断线重连、离线消息补发；`EventBusOptions` 配置端口与行为。

## 文档

- 完整说明文档（含全部集成形态）：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.EventBus%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
