# CodeWF.Toolkit

[![CI](https://github.com/dotnet9/CodeWF.Toolkit/actions/workflows/ci.yml/badge.svg)](https://github.com/dotnet9/CodeWF.Toolkit/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Core?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Core)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![Target](https://img.shields.io/badge/target-net10.0-512bd4)](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)

一组无 UI 依赖的 .NET 基础设施库合集：工具扩展、文件与图片处理、日志、事件总线与网络通信，均以 `CodeWF.Toolkit.*` 前缀发布。

A collection of UI-independent .NET infrastructure libraries: tool extensions, file/image processing, logging, event bus and network communication, published with the `CodeWF.Toolkit.*` prefix. Runtime messages use embedded English and Simplified Chinese JSON resources from CodeWF.Toolkit.Lang.

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
| [CodeWF.Toolkit.Lang](https://www.nuget.org/packages/CodeWF.Toolkit.Lang) | JSON/XML/RESX 国际化、运行时切换、父文化回退与库级嵌入资源 | 仅 .NET BCL，无 UI 或第三方运行时依赖 |
| [CodeWF.Toolkit.Core](https://www.nuget.org/packages/CodeWF.Toolkit.Core) | .NET 原生功能扩展：字符串/日期/枚举/正则等 + 应用数据目录 + GitHub Release 更新检查 | 本仓 Lang；零第三方运行时依赖 |
| [CodeWF.Toolkit.Files](https://www.nuget.org/packages/CodeWF.Toolkit.Files) | 文件操作集合：Json/Yaml/Csv/Excel 读写、7z 压缩、App.config | SharpCompress、YamlDotNet、CsvHelper、MiniExcel、System.Configuration.ConfigurationManager |
| [CodeWF.Toolkit.Image](https://www.nuget.org/packages/CodeWF.Toolkit.Image) | 图片处理集合：Magick.NET 缩放/水印/格式转换 + ZXing 条码二维码 | Magick.NET-Q16-AnyCPU、ZXing.Net.Bindings.Magick |
| [CodeWF.Toolkit.Logging](https://www.nuget.org/packages/CodeWF.Toolkit.Logging) | 轻量日志核心：控制台输出、文件轮转、UI 日志事件 | Microsoft.Extensions.Logging.Abstractions + 本仓 Lang |
| [CodeWF.Toolkit.Logging.Microsoft](https://www.nuget.org/packages/CodeWF.Toolkit.Logging.Microsoft) | Microsoft.Extensions.Logging 提供程序，把 MEL 日志接入轻量核心 | Microsoft.Extensions.{DependencyInjection.Abstractions, Hosting.Abstractions, Logging, Logging.Configuration, Options} + 本仓 Logging、Lang |
| [CodeWF.Toolkit.EventBus](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus) | 轻量进程内事件总线：Command/Query、`[EventHandler]` 特性、IOC 扫描注册 | 零第三方依赖 |
| [CodeWF.Toolkit.EventBus.Prism](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Prism) | Prism 容器集成 | Prism.Core、DryIoc.dll + 本仓 EventBus、Lang |
| [CodeWF.Toolkit.EventBus.AspNetCore](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.AspNetCore) | ASP.NET Core 集成 | FrameworkReference `Microsoft.AspNetCore.App` + 本仓 EventBus、Lang |
| [CodeWF.Toolkit.EventBus.Socket](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Socket) | 基于 TCP 的分布式事件总线：Pub/Sub、Query/Response、心跳重连、离线补发，不依赖第三方 MQ | 本仓 EventBus、NetWrapper、Lang |
| [CodeWF.Toolkit.NetWeaver](https://www.nuget.org/packages/CodeWF.Toolkit.NetWeaver) | AOT 友好的 TCP/UDP 数据包二进制序列化核心 | 零第三方依赖 |
| [CodeWF.Toolkit.NetWrapper](https://www.nuget.org/packages/CodeWF.Toolkit.NetWrapper) | TCP/UDP 通信封装与命令分发，含远程文件系统与断点续传 | 本仓 NetWeaver、EventBus、Logging、Lang |

## 快速开始 / Quick start

### 国际化（CodeWF.Toolkit.Lang）

控制台、服务和基础库可直接使用 `CodeWF.Toolkit.Lang`，无需引入 Avalonia。JSON 保持现有格式：`language`、`description`、`cultureName` 为元数据，其余对象路径组成稳定资源键。NuGet 库使用 `EmbeddedResource`；Demo 和桌面应用将 JSON 复制到输出和发布目录的 `I18n/`，保持宿主资源可替换。

```csharp
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;

var plugin = new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") };
I18nManager.Instance.Register(plugin, new System.Globalization.CultureInfo("en-US"));
I18nManager.Instance.Culture = new System.Globalization.CultureInfo("zh-CN");
var text = I18nManager.Instance.GetResource("App.Title");
```

库自身使用 `new JsonLocalizer(typeof(MyLibrary).Assembly)` 读取嵌入 JSON，无需调用方先注册。它跟随全局语言或当前 UI 文化，按请求文化、父文化、默认 `en-US` 回退；应用可用同名键覆盖库文案。空翻译保留，缺失键返回键本身。`JsonLangPlugin<TLanguage>`、`XmlLangPlugin<TLanguage>`、`ResxLangPlugin<TLanguage>` 支持已有语言元数据子类，解析逻辑只维护一份。裁剪或 NativeAOT 应用使用显式程序集或 `ResourceManager` 注册，避免 RESX 约定扫描。

全进程只有 `CodeWF.Toolkit.Lang.I18nManager.Instance` 一个管理器；Avalonia 的国际化绑定直接订阅该实例。即使没有注册业务资源，也可设置 `Culture` 来切换库内嵌文案。覆盖优先级为 **业务输出资源 > 外层库内嵌资源 > 内层依赖库内嵌资源**。显式合并多个库时，`AddResource(innerAssembly, outerAssembly)` 按内层到外层注册；后注册的库覆盖同键，业务输出资源始终优先，追加库、重复注册和重新加载均不改变该规则。宿主无需为每个库提供 resolver 或复制库资源。

### 应用更新（CodeWF.Toolkit.Core）

`IUpdateService` 统一检查 GitHub Release、匹配当前 RID、下载、SHA-256 校验与系统安装分发。应用只维护界面状态，程序集 `AssemblyInformationalVersion` 同时用于显示和版本比较。

```csharp
using CodeWF.Toolkit.Core.UpdateChecking;

using var updates = new UpdateService(new UpdateServiceOptions
{
    Owner = "dotnet9",
    Repository = "MyApp",
    ProductName = "MyApp"
});
var result = await updates.CheckAsync(cancellationToken: cancellationToken);
if (result is { Succeeded: true, Update: { AssetUrl: not null, ChecksumUrl: not null } update })
{
    var download = await updates.DownloadAsync(update, progress, cancellationToken);
    // 用户点击安装后调用；Windows 成功启动安装器后按 ShouldExitApplication 退出。
    var install = await updates.InstallAsync(download, cancellationToken);
}
```

无匹配资产或校验文件时，界面提供 `UpdateInfo.PageUrl` 作为发布页入口。检查失败通过 `UpdateCheckResult` 返回；下载失败和取消通过异常通知调用方，需由界面状态机处理。安装分发为 Windows EXE、Linux DEB、macOS DMG/PKG；便携 ZIP 只打开目录，启动失败返回手动处理。错误和日志从库的嵌入 JSON 读取，无需应用复制资源。

**UpdateChecking 的资源由 Toolkit 维护。** 安装 Core 包后，中英文 JSON 已嵌入程序集；无需提供 resolver、创建 JSON 或注册语言插件。未注册应用国际化时跟随 `CultureInfo.CurrentUICulture`；注册后跟随 `I18nManager.Instance.Culture`，不支持的语言回退到库内英文。

需要定制时，应用可在自己的国际化资源中只定义要替换的稳定键，使用既有 `JsonLangPlugin` 扫描输出目录。例如应用输出的 `I18n/en-US.json` 可写：

```json
{
  "language": "English",
  "description": "MyApp overrides",
  "cultureName": "en-US",
  "Toolkit.Core.Update.AssetMissing": "No package is available for this application."
}
```

只有应用提供的键被覆盖，其余文案仍从 Core 内嵌资源读取。完整键表在 [Core 中英文资源](src/Core/CodeWF.Toolkit.Core/I18n/)；覆盖带占位符的文案时保留 `{0}`、`{1}` 等格式参数。

库和 Demo 的 C# 资源键由 `I18n/Language.tt` 生成到 `Language.g.cs`，引用 `I18n.ResourceKeys` 强类型常量。修改 JSON 后执行 `pwsh scripts/sync_localization_keys.ps1`；CI 以 `-Check` 检查生成文件。普通构建直接使用已提交的生成代码，无需安装 T4。XAML 仍使用原有 `I18n` 绑定。

`AssemblyExtensions.CompileTime()` 优先读取 Core 包的 `buildTransitive` 构建目标写入的 UTC `CodeWF.BuildTimestamp` 元数据，支持 NativeAOT；旧程序集回退到 PE 时间或文件修改时间，并排除确定性构建的哈希时间戳。需要可重复构建时设置固定的 `CodeWFBuildTimestamp`（ISO 8601），或用 `CodeWFGenerateBuildTimestamp=false` 关闭自动生成；默认使用本次构建时间，会影响程序集的字节级可重复性。

各库的运行时错误、日志、枚举描述使用 `Toolkit.<库名>.<语义>` 键。中英文资源随各自 NuGet 包嵌入，中文数字转换所需字符表作为固定算法数据读取，不随界面语言改变。编译期 AOT/裁剪诊断保留英文常量，以符合 .NET 特性参数约束。

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

Demo 保留 Ribbon + Dock，按核心扩展、系统与文件、数据与图像、事件与集成、协议与网络、日志与观测分类；每个主菜单对应一个帮助类，52 个页面按类名查找；CSV / Excel 提供真实表格回读，网络与文件系统提供可保持的会话和传输操作，组合示例集中到“综合示例”。详见 [原生 Demo 文档](docs/NativeDemo.md)。

核心扩展再按文本、数值、日期时间、类型与查询分组，密集命令使用双行排列，保留完整帮助类名。UI 从公开 NuGet 安装 CodeWF.Avalonia 包，单独克隆本仓即可构建；`global.json` 选择稳定版 .NET 10 SDK。

标题栏提供浅色、深色、跟随系统三态，搭配 .NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏、夜空八套配色。颜色与模式独立选择，通过 NuGet 中的 StudioThemeController 跟随系统并同步文档属性面板。

当前 `1.0.0.22` 的 Dock 文档分屏、浮动与重新停靠效果（真实 Windows NativeAOT + Skia 离屏渲染）：

![文档与属性面板同步停靠](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-dock-redocked.png)

![浮动文档保留属性面板](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-dock-floating.png)

快捷入口采用 18×18 图标与 30×30 点击热区，Ribbon 主命令使用 32×32 图标。每个文档内的演示和属性面板通过 GridSplitter 调整宽度，属性默认行高 31px、名称列宽 118px；下列历史页面截图来自 `1.0.0.18` 的 Windows x64 NativeAOT 程序。

![Ribbon 帮助类分组与原生 Dock 工作区](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-ribbon.png)

![.NET 紫色原生演示台](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-dotnet.png)

![AI 极光浅色原生演示台](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-aurora-light.png)

![JsonExtensions 独立演示](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-json.png)

![文件流水线与真实产物](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-pipeline.png)

![CSV 与 Excel 导出回读](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-table.png)

![文件系统会话与目录请求](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-files.png)

![主题与实验截图巡览](https://raw.githubusercontent.com/dotnet9/CodeWF.Toolkit/main/docs/images/studio-interactions.gif)

The Ribbon and Dock demo groups APIs by function, with one helper class per primary menu entry. Composite workflows are in the final category. Its UI libraries come from NuGet; cloning CodeWF.Toolkit alone is sufficient to build the GUI and console demos.

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

图形演示台的安装包由 [GitHub Releases](https://github.com/dotnet9/CodeWF.Toolkit/releases) 提供（随 `v*` tag 构建）：Windows x64 `setup.exe`、Linux x64 / ARM64 `deb`、macOS x64 / ARM64 `dmg`，均附 `.sha256`，不分发便携压缩包。控制台演示以源码运行。

## 许可与贡献 / License and contributing

[MIT](LICENSE)。Issue 与 PR 都欢迎，提交前请确保 `dotnet build CodeWF.Toolkit.slnx -c Release` 与 `dotnet test` 通过。
