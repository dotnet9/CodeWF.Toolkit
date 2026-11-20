# CodeWF.Toolkit.Logging

轻量级日志核心库：控制台输出、文件轮转日志与 UI 日志事件，适合控制台、桌面等 .NET 应用统一接入日志能力。Lightweight logging core with console output, rotating file logs and UI log events.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Logging?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Logging)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.Logging
```

仅依赖 Microsoft.Extensions.Logging.Abstractions 与本仓 `CodeWF.Toolkit.Lang`（随包自动安装）；目标框架 net10.0。需要把 Microsoft.Extensions.Logging 生态日志接入本核心时，再叠加 [CodeWF.Toolkit.Logging.Microsoft](https://www.nuget.org/packages/CodeWF.Toolkit.Logging.Microsoft)。

## 快速使用

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
    // LineTemplate 可自定义输出模板（LineTemplateController.DefaultTemplate）
});

Logger.Info("服务已启动");
Logger.Warn("磁盘空间不足");
Logger.Error("处理失败", ex);

await Logger.ShutdownAsync();   // 退出前刷盘
```

要点：

- `LoggerOptions` 为 record：MinimumLevel、EnableConsole、File（轮转/保留策略/批量写入）、LineTemplate、LocalizedLevelFormatter。
- `LogEventFeed` 暴露 UI 日志事件快照，供日志查看控件订阅（配合 `CodeWF.Avalonia.Log`）。
- `LoggerSelfDiagnostics` 输出日志系统自身的诊断信息。
- 运行时文案使用嵌入的中英文资源，应用可用同名键覆盖。

## 注意事项

- UI 事件流回调在后台线程触发，界面订阅方需自行调度到 UI 线程。
- 文件写入为批量异步，崩溃前请调用 `ShutdownAsync()`。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Logging%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
