# CodeWF.Toolkit.Files

文件操作类集合：Json/Yaml/Csv/Excel 读写、7z 压缩解压、App.config 读写、HTML 转 Markdown。File operations: Json/Yaml/Csv/Excel reading & writing, 7z compression, App.config helpers and HTML to Markdown conversion.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Files?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Files)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.Files
```

依赖 SharpCompress、YamlDotNet、CsvHelper、MiniExcel、System.Configuration.ConfigurationManager（随包自动安装）；目标框架 net10.0。

## 能力一览

| 分类 | API | 说明 |
|---|---|---|
| Json | `JsonExtensions` | ToJson/FromJson 扩展，基于框架内置 System.Text.Json |
| Yaml | `YamlExtensions` | ToYaml/FromYaml（YamlDotNet） |
| Csv / Excel | `DataTableExtensions` | 基于 CsvHelper / MiniExcel 的导出导入 |
| 7z 压缩 | `SevenZipCompressor` | 基于 SharpCompress 的压缩/解压 |
| App.config | `AppConfigHelper` | appSettings 单项读写、批量快照和批量保存 |
| 文件读写 | `FileExtension` | 带重试的安全读写字符串/字节 |

## 示例

```csharp
using CodeWF.Toolkit.Files.Extensions;

var yaml = student.ToYaml(out var error);

var compressor = new SevenZipCompressor(httpClient);
compressor.Zip(sourceDirectory, "out.zip");
compressor.Decompress("out.zip", destinationDirectory);
```

`AppConfigHelper.ReadAll` 只解析一次文件、后续走内存快照；`SetMany` 只更新传入键、保留注释，只保存一次文件；文件路径重载支持 NativeAOT。

## 注意事项

- AOT/裁剪：`JsonExtensions` 反射重载标记 `RequiresUnreferencedCode`，请改用 `JsonTypeInfo<T>` 重载；`AppConfigHelper` 的 `Configuration` 对象重载同标记，请改用文件路径或快照重载。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Files%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
