# CodeWF.Toolkit.Core

.NET 原生功能扩展 + 应用基础设施（应用数据目录、GitHub Release 更新检查），零第三方依赖。

## 安装

```bash
dotnet add package CodeWF.Toolkit.Core
```

## 扩展方法（CodeWF.Toolkit.Core.Extensions 等命名空间）

| 分类 | 代表 API |
|---|---|
| 字符串 | StringExtensions：正则快捷判断、截断、编码探测辅助 |
| 日期时间 | DateTimeExtension / DateTimeConverter / TimeSpanExtensions |
| 数值 | NumberExtension / DoubleExtensions / ByteUnit |
| 枚举 | EnumExtension + EnumDescriptionAttribute（枚举描述特性） |
| 集合/对象 | LinqExtension / ObjectExtension / IConvertibleExtensions |
| 校验/随机 | ValidationExtension / RandomExtension |
| 其他 | IpHelper（IP 工具）、ActionHelper、AssemblyExtensions、NativeLibraryHelper、FileHelper（编码检测 GetFileEncodeType、安全读写重试、删除重试、打开文件夹/选中文件） |

## 应用数据目录（CodeWF.Toolkit.Core.ApplicationData）

`AppDataMigrator`：管理 %LOCALAPPDATA% 下的应用数据目录，并支持从旧位置向新位置迁移（换包名/重组目录后首次启动搬运数据）。

## 更新检查（CodeWF.Toolkit.Core.UpdateChecking）

基于 GitHub Releases 的版本检查：

```csharp
var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
var result = await checker.CheckAsync(new Version(1, 0, 0, 0));

if (!result.Succeeded) { /* result.Error */ }
else if (result.Update is null) { /* 已是最新 */ }
else { /* result.Update.Version / Tag / Title，配合 Release 页跳转 */ }
```

- 支持 ETag 增量检查（UpdateCheckState 缓存 Etag，命中 304 不消耗 API 配额）。
- 检查失败与"无更新"通过 `UpdateCheckResult` 显式区分，不误报。

## 注意事项

- 保持零第三方依赖：CI 中不监控此项，但新增依赖请评审。
- GB2312 等遗留编码检测在 Windows 上依赖系统编码提供程序。
