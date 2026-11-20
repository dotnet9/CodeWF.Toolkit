# CodeWF.Toolkit.Core

.NET 原生功能扩展合集：字符串/日期/枚举/正则/数值等扩展方法、应用数据目录管理、GitHub Release 更新检查，零第三方库依赖。.NET native extensions, application data directory management and GitHub Release update checking with zero third-party dependencies.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Core?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Core)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.Core
```

依赖本仓 `CodeWF.Toolkit.Lang`（经传递引用自动安装）；零第三方运行时依赖。

## 扩展方法

| 分类 | 代表 API |
|---|---|
| 字符串 | `StringExtensions`：正则快捷判断、截断、编码探测辅助 |
| 日期时间 | `DateTimeExtension` / `DateTimeConverter` / `TimeSpanExtensions` |
| 数值 | `NumberExtension` / `DoubleExtensions` / `ByteUnit` |
| 枚举 | `EnumExtension` + `EnumDescriptionAttribute` |
| 集合/对象 | `LinqExtension` / `ObjectExtension` / `IConvertibleExtensions` |
| 校验/随机 | `ValidationExtension` / `RandomExtension` |
| 其他 | `IpHelper`、`FileHelper`（编码检测、安全读写重试）、`AssemblyExtensions.CompileTime()` |

## 应用数据目录

`AppDataMigrator` 管理 `%LOCALAPPDATA%` 下的应用数据目录，支持从旧位置向新位置迁移（换包名/重组目录后首次启动搬运数据）。

## 更新检查（GitHub Releases）

```csharp
using CodeWF.Toolkit.Core.UpdateChecking;

var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
```

- 支持 ETag 增量检查（命中 304 不消耗 API 配额）；检查失败与"无更新"通过 `UpdateCheckResult` 显式区分。
- 完整的检查/下载/SHA-256 校验/安装分发流程见 `IUpdateService` / `UpdateService`，安装资源（中英文 JSON）已嵌入程序集，应用可用同名键覆盖。

## 注意事项

- AOT/裁剪：`EnumExtension`、`ValidationExtension`、`IConvertibleExtensions` 中读取特性的方法标记 `RequiresUnreferencedCode`，请使用显式映射或手写转换。
- GB2312 等遗留编码检测在 Windows 上依赖系统编码提供程序。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Core%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
