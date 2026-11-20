# CodeWF.Toolkit.NetWrapper

TCP/UDP 通信封装与命令分发：客户端/服务端、心跳、断线重连，内置远程文件系统与断点续传能力。TCP/UDP helpers and command dispatching with built-in remote file system and resumable file transfer.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.NetWrapper?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.NetWrapper)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.NetWrapper
```

依赖本仓 `CodeWF.Toolkit.NetWeaver`、`CodeWF.Toolkit.EventBus`、`CodeWF.Toolkit.Logging`、`CodeWF.Toolkit.Lang`（随包自动安装）；目标框架 net10.0。

## 快速使用

```csharp
using CodeWF.Toolkit.NetWrapper.Helpers;

var server = new TcpSocketServer();
await server.StartAsync("demo-server", "127.0.0.1", 39561);

var client = new TcpSocketClient();
await client.ConnectAsync("demo-server", "127.0.0.1", 39561);
await client.SendCommandAsync(packet);
```

| 组件 | 说明 |
|---|---|
| `TcpSocketServer` / `TcpSocketClient` | TCP 服务端与客户端 |
| `UdpSocketServer` / `UdpSocketClient` | UDP 收发 |
| `SocketCommand` 系 | 命令分发：客户端上线/断开、错误等，配合 EventBus 订阅 |
| `Heartbeat` | 心跳保活模型 |
| FileSystem 目录 | 远程文件系统：浏览/建目录/删除/上传/下载/分块 ACK/断点续传 |

## 注意事项

- 文件传输涉及大端序分块与校验，跨版本互联请保持两端包版本一致。
- 数据包模型与序列化见 [CodeWF.Toolkit.NetWeaver](https://www.nuget.org/packages/CodeWF.Toolkit.NetWeaver)。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Net%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
