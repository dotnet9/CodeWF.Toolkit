# CodeWF.Toolkit

[![CI](https://github.com/dotnet9/CodeWF.Toolkit/actions/workflows/ci.yml/badge.svg)](https://github.com/dotnet9/CodeWF.Toolkit/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Core?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Core)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![Target](https://img.shields.io/badge/target-net10.0-512bd4)](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)

一组无 UI 依赖的 .NET 基础设施库合集：工具扩展、文件与图片处理、日志、事件总线与网络通信，均以 `CodeWF.Toolkit.*` 前缀发布。

A collection of UI-independent .NET infrastructure libraries: tool extensions, file/image processing, logging, event bus and network communication, published with the `CodeWF.Toolkit.*` prefix.

## 适用场景 / Typical scenarios

| 场景 | 用到的包 |
|---|---|
| 进程内解耦：发布订阅、Command/Query、`[EventHandler]` 扫描注册 | `CodeWF.Toolkit.EventBus`（可选 Prism / ASP.NET Core 集成） |
| 跨进程消息：TCP 分布式总线，心跳保活、断线重连、离线消息补发 | `CodeWF.Toolkit.EventBus.Socket` |
| 文件批处理：Json/Yaml/Csv/Excel 读写、7z 压缩解压、App.config | `CodeWF.Toolkit.Files` |
| 图片批处理：缩放/水印/格式转换、多尺寸 ico 合成拆分、条码二维码 | `CodeWF.Toolkit.Image` |
| 统一日志：控制台 + 文件轮转 + UI 事件流，可桥接 Microsoft.Extensions.Logging | `CodeWF.Toolkit.Logging`、`CodeWF.Toolkit.Logging.Microsoft` |
| 二进制协议：特性声明字段布局的 AOT 友好序列化 + TCP/UDP 封装（含远程文件系统与断点续传） | `CodeWF.Toolkit.NetWeaver`、`CodeWF.Toolkit.NetWrapper` |
| 桌面应用基础设施：应用数据目录迁移、GitHub Release 更新检查、常用扩展方法 | `CodeWF.Toolkit.Core` |

## 包一览 / Packages

所有包均面向 `net10.0`。「依赖」列中的**本仓包**由包依赖自动带入，无需单独安装。

| 包 | 说明 | 依赖 |
|---|---|---|
| [CodeWF.Toolkit.Core](https://www.nuget.org/packages/CodeWF.Toolkit.Core) | .NET 原生功能扩展：字符串/日期/枚举/正则等 + 应用数据目录 + GitHub Release 更新检查 | 零第三方依赖 |
| [CodeWF.Toolkit.Files](https://www.nuget.org/packages/CodeWF.Toolkit.Files) | 文件操作集合：Json/Yaml/Csv/Excel 读写、7z 压缩、App.config | SharpCompress、YamlDotNet、CsvHelper、MiniExcel、System.Configuration.ConfigurationManager |
| [CodeWF.Toolkit.Image](https://www.nuget.org/packages/CodeWF.Toolkit.Image) | 图片处理集合：Magick.NET 缩放/水印/格式转换 + ZXing 条码二维码 | Magick.NET-Q16-AnyCPU、ZXing.Net.Bindings.Magick |
| [CodeWF.Toolkit.Logging](https://www.nuget.org/packages/CodeWF.Toolkit.Logging) | 轻量日志核心：控制台输出、文件轮转、UI 日志事件 | Microsoft.Extensions.Logging.Abstractions（**唯一依赖**，无其它第三方库） |
| [CodeWF.Toolkit.Logging.Microsoft](https://www.nuget.org/packages/CodeWF.Toolkit.Logging.Microsoft) | Microsoft.Extensions.Logging 提供程序，把 MEL 日志接入轻量核心 | Microsoft.Extensions.{DependencyInjection.Abstractions, Hosting.Abstractions, Logging, Logging.Configuration, Options} + 本仓 Logging |
| [CodeWF.Toolkit.EventBus](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus) | 轻量进程内事件总线：Command/Query、`[EventHandler]` 特性、IOC 扫描注册 | 零第三方依赖 |
| [CodeWF.Toolkit.EventBus.Prism](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Prism) | Prism 容器集成 | Prism.Core、DryIoc.dll + 本仓 EventBus |
| [CodeWF.Toolkit.EventBus.AspNetCore](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.AspNetCore) | ASP.NET Core 集成 | FrameworkReference `Microsoft.AspNetCore.App` + 本仓 EventBus |
| [CodeWF.Toolkit.EventBus.Socket](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Socket) | 基于 TCP 的分布式事件总线：Pub/Sub、Query/Response、心跳重连、离线补发，不依赖第三方 MQ | 本仓 EventBus、NetWrapper |
| [CodeWF.Toolkit.NetWeaver](https://www.nuget.org/packages/CodeWF.Toolkit.NetWeaver) | AOT 友好的 TCP/UDP 数据包二进制序列化核心 | 零第三方依赖 |
| [CodeWF.Toolkit.NetWrapper](https://www.nuget.org/packages/CodeWF.Toolkit.NetWrapper) | TCP/UDP 通信封装与命令分发，含远程文件系统与断点续传 | 本仓 NetWeaver、EventBus、Logging |

## 快速开始 / Quick start

```bash
dotnet add package CodeWF.Toolkit.Core
dotnet add package CodeWF.Toolkit.EventBus
dotnet add package CodeWF.Toolkit.Logging
```

### 日志（CodeWF.Toolkit.Logging）

```csharp
using CodeWF.Toolkit.Logging;

Logger.TryInitialize(new LoggerOptions
{
    MinimumLevel = LogLevel.Information,
    EnableConsole = true,
    File = new FileLogOptions
    {
        DirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs"),
        MaxFileSizeBytes = 32 * 1024 * 1024,
        RetentionDays = 30,
    },
});

Logger.Info("服务已启动");
Logger.Error("处理失败", ex);
await Logger.ShutdownAsync();   // 退出前刷盘
```

需要把 MEL 生态的日志也收进来时，加装 `CodeWF.Toolkit.Logging.Microsoft`：`builder.Logging.AddCodeWFLogger(options => { /* 同 LoggerOptions */ });`

### 事件总线（CodeWF.Toolkit.EventBus）

```csharp
using CodeWF.Toolkit.EventBus;

var bus = new EventBus();
bus.Subscribe<GreetCommand>(cmd => Console.WriteLine(cmd.Name));
await bus.PublishAsync(new GreetCommand("hi"));

var query = new TimeQuery();
bus.Subscribe(new TimeQueryHandler());   // 含 [EventHandler] 方法的处理器
bus.Query(query);
```

### 序列化往返（CodeWF.Toolkit.NetWeaver）

```csharp
[NetHead(0x01, 1)]
public sealed class SamplePacket : INetObject
{
    [NetFieldOrder(0)] public int Id { get; set; }
    [NetFieldOrder(1)] public string Title { get; set; } = string.Empty;
}

var buffer = packet.Serialize(systemId: 0);
var restored = buffer.Deserialize<SamplePacket>();
```

### TCP 收发（CodeWF.Toolkit.NetWrapper）

```csharp
using CodeWF.Toolkit.NetWrapper.Helpers;

var server = new TcpSocketServer();
await server.StartAsync("demo-server", "127.0.0.1", 39561);

var client = new TcpSocketClient();
await client.ConnectAsync("demo-server", "127.0.0.1", 39561);
await client.SendCommandAsync(packet);
```

远程文件系统（浏览/上传/下载/断点续传）见 `CodeWF.Toolkit.NetWrapper.FileSystem` 的 `TcpSocketClientFileSystemFeature`。

### 文件与图片（CodeWF.Toolkit.Files / CodeWF.Toolkit.Image）

```csharp
using CodeWF.Toolkit.Core.FileExtensions;   // SevenZipCompressor
using CodeWF.Toolkit.Files.Extensions;      // ToJson / ToYaml
using CodeWF.Toolkit.Image;                 // ImageHelper / QrCodeGenerator

var yaml = student.ToYaml(out var error);

var compressor = new SevenZipCompressor(httpClient);
compressor.Zip(sourceDirectory, "out.zip");
compressor.Decompress("out.zip", destinationDirectory);

await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [24, 32, 48]);
QrCodeGenerator.GenerateQrCode("CodeWF", "https://codewf.com", "qr.png");
```

### 更新检查（CodeWF.Toolkit.Core）

```csharp
using CodeWF.Toolkit.Core.UpdateChecking;

var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
```

## 与 CodeWF.Avalonia 的关系 / Relationship with CodeWF.Avalonia

- `CodeWF.Avalonia.Log`（日志查看控件与桌面通知）配套本仓的 `CodeWF.Toolkit.Logging`：控件订阅核心的 UI 日志事件流，日志回调在后台线程，界面侧自行调度到 UI 线程。
- 两个仓库各自带演示程序：本仓 `demo/` 下是控制台演示与 Avalonia 图形演示台，`CodeWF.Avalonia` 仓的演示台展示的是控件与主题。
- UI 相关能力一律留在 `CodeWF.Avalonia.*`；本仓保持无 UI 依赖。

## 依赖与目标框架 / Dependencies and AOT

全部库项目面向 `net10.0`，`Directory.Packages.props` 集中管理版本。Native AOT/裁剪注意点：

| 包 | 注意点 |
|---|---|
| NetWeaver | 反射式 DTO 序列化：`SerializeHelper` 通过 `DynamicallyAccessedMembers` / `UnconditionalSuppressMessage` 声明保留要求，裁剪时需保留被序列化 DTO 的公共属性。 |
| Files | `JsonExtensions` 的反射重载标记 `RequiresUnreferencedCode`，AOT 请改用 `JsonTypeInfo<T>` 重载；`AppConfigHelper` 基于 `System.Configuration.ConfigurationManager`，同标记，AOT 请改用文件路径重载。 |
| Core | `EnumExtension`、`ValidationExtension`、`IConvertibleExtensions` 中读取特性的方法标记 `RequiresUnreferencedCode`，AOT 下请使用显式映射或手写转换。 |
| Image | Magick.NET 携带原生二进制（Q16-AnyCPU），包体积较大，请按需独立引用而非并入聚合包。 |
| Logging | 文件写入为批量异步，崩溃前请调用 `Logger.ShutdownAsync()`；UI 事件回调在后台线程触发。 |

CI 的 `aot-smoke` 作业会对控制台演示与 Avalonia 演示台做 `PublishAot=true` 发布并运行 `--smoke`。

## 包 → 源码 → 文档索引 / Source & docs index

| 包 | 源码 | 文档 |
|---|---|---|
| Core | [src/Core](src/Core/CodeWF.Toolkit.Core) | [说明文档](docs/CodeWF.Toolkit.Core%20说明文档.md) |
| Files | [src/Files](src/Files/CodeWF.Toolkit.Files) | [说明文档](docs/CodeWF.Toolkit.Files%20说明文档.md) |
| Image | [src/Image](src/Image/CodeWF.Toolkit.Image) | [说明文档](docs/CodeWF.Toolkit.Image%20说明文档.md) |
| Logging / Logging.Microsoft | [src/Logging](src/Logging) | [说明文档](docs/CodeWF.Toolkit.Logging%20说明文档.md) |
| EventBus 系列（含 Prism / AspNetCore / Socket） | [src/EventBus](src/EventBus) | [说明文档](docs/CodeWF.Toolkit.EventBus%20说明文档.md) |
| NetWeaver / NetWrapper | [src/Net](src/Net) | [说明文档](docs/CodeWF.Toolkit.Net%20说明文档.md) |

## 演示 / Demos

新版原生首页提供真实事件流、Int32 字节编码和 JSON → YAML → ZIP → QR 流水线，默认使用 .NET 紫，并支持 AI 极光与暖陶配色。图形 Demo 通过 NuGet 引用 UI 库，**只克隆本仓即可构建**；功能映射、双语资源与 Windows x64 NativeAOT 验收见 [原生演示台文档](docs/NativeDemo.md)。

![.NET 紫色原生演示台](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-dotnet.png)

![AI 极光浅色原生演示台](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-aurora-light.png)

![文件流水线与真实产物](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-pipeline.png)

![主题与实验切换](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-interactions.gif)

The native overview runs real event, serialization and file experiments. Building the GUI currently requires the sibling CodeWF.Avalonia source checkout; the console Demo and production libraries remain independently buildable.

```bash
# 控制台演示：依次跑日志、进程内总线、分布式总线、序列化，然后进入交互菜单
dotnet run --project demo/CodeWF.Toolkit.Demo
# 冒烟模式：跑完全部演示直接退出，退出码 0 表示通过（CI / AOT 冒烟使用）
dotnet run --project demo/CodeWF.Toolkit.Demo -- --smoke

# Avalonia 图形演示台：同款能力以界面方式逐页演示
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia
# 冒烟模式：逐页构建后逐色板渲染一遍再退出，退出码 0 表示全部通过（CI / AOT 冒烟使用）
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia -- --smoke
```

图形演示台的安装包由 [GitHub Releases](https://github.com/dotnet9/CodeWF.Toolkit/releases) 提供（随 `v*` tag 构建）：Windows `setup.exe` / 便携 `zip`、Linux `deb`、macOS `dmg`，均附 `.sha256` 校验文件。控制台演示以源码运行，不作为安装包分发。

## 许可与贡献 / License and contributing

[MIT](LICENSE)。Issue 与 PR 都欢迎，提交前请确保 `dotnet build CodeWF.Toolkit.slnx -c Release` 与 `dotnet test` 通过。
