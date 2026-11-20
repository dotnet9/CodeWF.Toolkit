# CodeWF.Toolkit.EventBus.Prism

事件总线的 Prism/DryIoc 容器集成：在 Prism 应用中完成注册、处理器扫描与启动。Prism/DryIoc container integration for the event bus.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.EventBus.Prism?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus.Prism)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.EventBus.Prism
```

依赖 Prism.Core、DryIoc.dll 与本仓 `CodeWF.Toolkit.EventBus`、`CodeWF.Toolkit.Lang`（随包自动安装）；目标框架 net10.0。适合 Prism.DryIoc.Avalonia 桌面应用。

## 用法

通过 Prism.Ioc 容器扩展在应用启动流程中完成事件总线注册、`[EventHandler]` 处理器扫描与启动，处理器实例从 Prism 容器解析，构造依赖由容器注入：

```csharp
public override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.AddEventBus(typeof(SomeHandler).Assembly);
}

public override void OnInitialized()
{
    // 启动总线并扫描处理器
    Container.UseEventBus(typeof(SomeHandler).Assembly);
}
```

进程内总线的订阅、Command/Query 用法与核心包一致，见 [CodeWF.Toolkit.EventBus](https://www.nuget.org/packages/CodeWF.Toolkit.EventBus)。

## 文档

- 完整说明文档（含全部集成形态）：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.EventBus%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
