# CodeWF.Toolkit.Logging / CodeWF.Toolkit.Logging.Microsoft

轻量日志双包：

- **Logging**（核心）：静态 Logger、控制台输出、文件轮转、UI 日志事件流，零第三方依赖（仅抽象包）。
- **Logging.Microsoft**：Microsoft.Extensions.Logging 提供程序，把 MEL 生态日志统一接入核心。

## 安装

```bash
dotnet add package CodeWF.Toolkit.Logging
# ASP.NET Core / 通用主机应用再叠加：
dotnet add package CodeWF.Toolkit.Logging.Microsoft
```

## 核心（CodeWF.Toolkit.Logging）

```csharp
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
- `LogEventFeed` 暴露 UI 日志事件快照，供日志查看控件订阅（配合 CodeWF.Avalonia.Log）。
- `LoggerSelfDiagnostics` 输出日志系统自身的诊断信息。

## MEL 提供程序（CodeWF.Toolkit.Logging.Microsoft）

```csharp
builder.Logging.AddCodeWFLogger(options => { /* 同 LoggerOptions */ });
```

`CodeWFLoggerProvider` + `CodeWFLoggerOptionsSetup` 支持配置绑定；`MessageTemplateParser` 解析结构化模板；`CodeWFUserLogState` 支持按用户标记日志。

## 注意事项

- UI 事件流回调在后台线程触发，界面订阅方需自行调度到 UI 线程。
- 文件写入为批量异步，崩溃前请调用 ShutdownAsync。
