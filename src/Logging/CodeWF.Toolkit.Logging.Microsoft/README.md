# CodeWF.Toolkit.Logging.Microsoft

Microsoft.Extensions.Logging 提供程序，将应用日志统一接入轻量日志核心。Microsoft.Extensions.Logging provider bridging to the lightweight logging core.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Logging.Microsoft?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Logging.Microsoft)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.Logging
dotnet add package CodeWF.Toolkit.Logging.Microsoft
```

依赖 Microsoft.Extensions.{DependencyInjection.Abstractions, Hosting.Abstractions, Logging, Logging.Configuration, Options} 与本仓 `CodeWF.Toolkit.Logging`、`CodeWF.Toolkit.Lang`（随包自动安装）；目标框架 net10.0。

## 用法

```csharp
builder.Logging.AddCodeWFLogger(options =>
{
    // 与 CodeWF.Toolkit.Logging 的 LoggerOptions 相同
    options.MinimumLevel = LogLevel.Information;
    options.File = new FileLogOptions
    {
        DirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs"),
    };
});
```

`CodeWFLoggerProvider` + `CodeWFLoggerOptionsSetup` 支持配置绑定；`MessageTemplateParser` 解析结构化模板；`CodeWFUserLogState` 支持按用户标记日志。MEL 生态的日志由此统一进入轻量核心：控制台输出、文件轮转与 UI 日志事件流一次到位。

## 注意事项

- UI 事件流回调在后台线程触发，界面订阅方需自行调度到 UI 线程。
- 退出前请调用 `Logger.ShutdownAsync()` 刷盘（见核心包）。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Logging%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
