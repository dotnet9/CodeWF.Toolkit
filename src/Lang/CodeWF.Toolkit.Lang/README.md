# CodeWF.Toolkit.Lang

UI 无关的国际化引擎：JSON / XML / RESX 资源、运行时语言切换、父文化回退与库级嵌入资源。UI-independent localization with JSON, XML and RESX resources, culture fallback and runtime language switching.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Lang?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Lang)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.Lang
```

仅依赖 .NET BCL，无 UI 或第三方运行时依赖；目标框架 net10.0。

## 初始化（应用只做一次）

```csharp
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;

var plugin = new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") };
I18nManager.Instance.Register(plugin, new CultureInfo("en-US"));
I18nManager.Instance.Culture = new CultureInfo("zh-CN");
var text = I18nManager.Instance.GetResource("App.Title");
```

JSON 保持现有格式：`language`、`description`、`cultureName` 为元数据，其余对象路径组成稳定资源键。`JsonLangPlugin<TLanguage>`、`XmlLangPlugin<TLanguage>`、`ResxLangPlugin<TLanguage>` 支持已有语言元数据子类。

## 库内嵌资源

NuGet 库用 `new JsonLocalizer(typeof(MyLibrary).Assembly)` 读取 `EmbeddedResource` 嵌入的 JSON，无需调用方先注册：跟随全局语言或当前 UI 文化，按 请求文化 → 父文化 → 默认 `en-US` 回退；空翻译保留，缺失键返回键本身。

同键优先级：**业务输出资源 > 外层库内嵌资源 > 内层依赖库内嵌资源**。显式合并多个库时，`AddResource(innerAssembly, outerAssembly)` 按内层到外层注册；业务输出资源始终优先，宿主无需为每个库提供 resolver 或复制库资源。

## 注意事项

- 裁剪或 NativeAOT 应用使用显式程序集或 `ResourceManager` 注册，避免 RESX 约定扫描。
- 全进程只有 `I18nManager.Instance` 一个管理器；Avalonia 绑定（`CodeWF.Avalonia.Lang`）直接订阅该实例。

## 文档

- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
