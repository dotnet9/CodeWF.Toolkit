# CodeWF.Toolkit.Files

文件操作集合：多格式读写、7z 压缩、App.config。

## 安装

```bash
dotnet add package CodeWF.Toolkit.Files
```

## 能力一览

| 分类 | API | 说明 |
|---|---|---|
| Json | JsonExtensions（CodeWF.Toolkit.Files.Extensions） | ToJson/FromJson 扩展，基于框架内置 System.Text.Json |
| Yaml | YamlExtensions | ToYaml/FromYaml（YamlDotNet） |
| Csv / Excel | DataTableExtensions（CodeWF.Toolkit.Files.Exports） | 基于 CsvHelper / MiniExcel 的导出导入 |
| 7z 压缩 | SevenZipCompressor / ISevenZipCompressor | 基于 SharpCompress 的压缩/解压 |
| App.config | AppConfigHelper（CodeWF.Toolkit.Files.Helpers） | 传统 App.config 的 appSettings 读写（System.Configuration.ConfigurationManager） |
| 文件读写 | FileExtension（CodeWF.Toolkit.Files.FileExtensions） | 带重试的安全读写字符串/字节 |

## 示例

```csharp
using CodeWF.Toolkit.Files.Extensions;

var yaml = student.ToYaml(out var error);
var back = yaml.FromYaml<Student>(out var obj, out error);
```

## 注意事项

- 目标框架 net10.0，Json 能力使用框架内置实现，无 System.Text.Json 包依赖。
- 7z 能力面向常见压缩格式；分卷与加密压缩请评估 SharpCompress 能力边界。
