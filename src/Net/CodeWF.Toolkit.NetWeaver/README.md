# CodeWF.Toolkit.NetWeaver

AOT 友好的 TCP/UDP 数据包二进制序列化核心：基于特性声明字段布局与顺序，零第三方依赖。AOT-friendly binary packet serialization core for TCP and UDP protocols with zero dependencies.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.NetWeaver?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.NetWeaver)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.NetWeaver
```

零第三方依赖；目标框架 net10.0。只要序列化能力选本包，需要完整 TCP/UDP 通信能力请用 [CodeWF.Toolkit.NetWrapper](https://www.nuget.org/packages/CodeWF.Toolkit.NetWrapper)。

## 快速使用

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

## 注意事项

- 反射式 DTO 序列化通过 `DynamicallyAccessedMembers` / `UnconditionalSuppressMessage` 声明保留要求；裁剪时需保留被序列化 DTO 的公共属性。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Net%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
