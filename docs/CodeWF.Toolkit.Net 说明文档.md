# CodeWF.Toolkit.NetWeaver / CodeWF.Toolkit.NetWrapper

网络通信双包：

- **NetWeaver**：AOT 友好的 TCP/UDP 数据包二进制序列化核心，零第三方依赖。
- **NetWrapper**：TCP/UDP 客户端与服务端封装、命令分发，内置远程文件系统与断点续传（原独立扩展包并入）。

## 安装

```bash
dotnet add package CodeWF.Toolkit.NetWeaver   # 只要序列化
dotnet add package CodeWF.Toolkit.NetWrapper  # 要完整通信能力
```

## 序列化（CodeWF.Toolkit.NetWeaver）

用特性声明包头与字段顺序，属性即协议：

```csharp
[NetHead(0x01, 1)]
public sealed class SamplePacket : INetObject
{
    [NetFieldOrder(0)] public int Id { get; set; }
    [NetFieldOrder(1)] public float Score { get; set; }
    [NetFieldOrder(2)] public string Title { get; set; } = string.Empty;
}

var buffer = packet.Serialize(systemId: 0);
var restored = buffer.Deserialize<SamplePacket>();
```

- `[NetIgnoreMember]` 跳过成员，位域场景用 `NetFieldOffsetAttribute` 精确布局。
- 反射式序列化在裁剪/AOT 下通过 DynamicDependency 保留声明保证可用（见 SerializeHelper 内说明）。

## TCP/UDP 通信（CodeWF.Toolkit.NetWrapper）

| 组件 | 说明 |
|---|---|
| TcpSocketServer / TcpSocketClient | TCP 服务端与客户端：StartAsync / ConnectAsync / SendCommandAsync(INetObject) |
| UdpSocketServer / UdpSocketClient | UDP 收发 |
| SocketCommand 系 | 命令分发：客户端上线/断开（SocketClientChangedCommand）、错误（TcpClientErrorCommand）等，配合 EventBus 订阅 |
| Heartbeat | 心跳保活模型 |
| FileSystem 目录 | 远程文件系统：浏览/建目录/删除/上传/下载/分块 ACK/断点续传（TcpSocketClientFileSystemExtensions 等扩展接入） |

## 注意事项

- NetWrapper 依赖同仓 EventBus 与 Logging（进程内事件与日志），ProjectReference 直连。
- 文件传输涉及大端序分块与校验，跨版本互联请保持两端包版本一致。
