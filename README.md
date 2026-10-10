# CodeWF.Toolkit

CodeWF.Toolkit 是一组无 UI 依赖的 .NET 基础设施库合集：工具扩展、文件与图片处理、日志、事件总线与网络通信，均以 `CodeWF.Toolkit.*` 前缀发布。

CodeWF.Toolkit is a collection of UI-independent .NET infrastructure libraries: tool extensions, file/image processing, logging, event bus and network communication, published with the `CodeWF.Toolkit.*` prefix.

## 包一览 / Packages

| 包 | 说明 |
|---|---|
| [CodeWF.Toolkit.Core](https://www.nuget.org/packages/CodeWF.Toolkit.Core) | .NET 原生功能扩展：字符串/日期/枚举/正则等 + 应用数据目录 + GitHub Release 更新检查，零第三方依赖 |
| [CodeWF.Toolkit.Files](https://www.nuget.org/packages/CodeWF.Toolkit.Files) | 文件操作集合：Json/Yaml/Csv/Excel 读写、7z 压缩、App.config |
| [CodeWF.Toolkit.Image](https://www.nuget.org/packages/CodeWF.Toolkit.Image) | 图片处理集合：Magick.NET 缩放/水印/格式转换 + ZXing 条码二维码 |
| [CodeWF.Toolkit.Logging](https://www.nuget.org/packages/CodeWF.Toolkit.Logging) | 轻量日志核心：控制台输出、文件轮转、UI 日志事件，零第三方依赖 |
| [CodeWF.Toolkit.Logging.Microsoft](https://www.nuget.org/packages/CodeWF.Toolkit.Logging.Microsoft) | Microsoft.Extensions.Logging Provider |
| [CodeWF.Toolkit.EventBus](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus) | 轻量进程内事件总线：Command/Query、`[EventHandler]` 特性、IOC 扫描注册，零第三方依赖 |
| [CodeWF.Toolkit.EventBus.Prism](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Prism) | Prism 容器集成 |
| [CodeWF.Toolkit.EventBus.AspNetCore](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.AspNetCore) | ASP.NET Core 集成 |
| [CodeWF.Toolkit.EventBus.Socket](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Socket) | 基于 TCP 的分布式事件总线：Pub/Sub、Query/Response、心跳重连、离线补发，不依赖第三方 MQ |
| [CodeWF.Toolkit.NetWeaver](https://www.nuget.org/packages/CodeWF.Toolkit.NetWeaver) | AOT 友好的 TCP/UDP 数据包二进制序列化核心，零第三方依赖 |
| [CodeWF.Toolkit.NetWrapper](https://www.nuget.org/packages/CodeWF.Toolkit.NetWrapper) | TCP/UDP 通信封装与命令分发，含远程文件系统与断点续传 |

## 安装 / Install

```bash
dotnet add package CodeWF.Toolkit.Core
dotnet add package CodeWF.Toolkit.EventBus
```

## 许可 / License

[MIT](LICENSE)
