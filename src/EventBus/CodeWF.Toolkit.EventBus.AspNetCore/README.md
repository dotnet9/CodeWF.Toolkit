# CodeWF.Toolkit.EventBus.AspNetCore

事件总线的 ASP.NET Core 集成：依赖注入注册、请求管线启动与处理器自动发现。ASP.NET Core integration for the event bus.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.EventBus.AspNetCore?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.AspNetCore)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.EventBus.AspNetCore
```

依赖 FrameworkReference `Microsoft.AspNetCore.App` 与本仓 `CodeWF.Toolkit.EventBus`、`CodeWF.Toolkit.Lang`（随包自动安装）；目标框架 net10.0。

## 用法

```csharp
var builder = WebApplication.CreateBuilder(args);

// 注册事件总线并扫描处理器程序集
builder.Services.AddEventBus(typeof(SomeHandler).Assembly);

var app = builder.Build();

// 请求管线启动事件总线
app.UseEventBus(typeof(SomeHandler).Assembly);
```

处理器自动发现，适合 WebAPI 后台事件；进程内总线的订阅、Command/Query 用法与核心包一致，见 [CodeWF.Toolkit.EventBus](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus)。

## 文档

- 完整说明文档（含全部集成形态）：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.EventBus%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
