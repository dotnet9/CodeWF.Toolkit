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
| App.config | AppConfigHelper（CodeWF.Toolkit.Files.Helpers） | appSettings 单项读写、批量快照和批量保存；文件路径重载使用 XML，支持 NativeAOT |
| 文件读写 | FileExtension（CodeWF.Toolkit.Files.FileExtensions） | 带重试的安全读写字符串/字节 |

## 示例

```csharp
using CodeWF.Toolkit.Files.Extensions;

var yaml = student.ToYaml(out var error);
var back = yaml.FromYaml<Student>(out var obj, out error);
```

### 批量配置读写

```csharp
using CodeWF.Toolkit.Files.Helpers;

var path = Path.Combine(AppContext.BaseDirectory, "MyApp.config");
AppConfigHelper.SetMany(path, new Dictionary<string, object?>
{
    ["Theme"] = "dark",
    ["PanelWidth"] = 320.5,
    ["ShowDetails"] = true
});

var settings = AppConfigHelper.ReadAll(path);
settings.TryGet<double>("PanelWidth", out var width);
settings.TryGet<bool>("ShowDetails", out var showDetails);
```

`ReadAll` 只读取、解析一次文件，后续从快照读取不访问磁盘；数值转换使用固定文化，不受界面语言影响。文件不存在或无法读取时返回空快照。需要反映磁盘上的后续修改时重新读取。

`SetMany` 每次重新加载并只更新传入的键，保留其他键、配置节和注释，只保存一次文件；空集合不创建文件。省略路径的重载沿用 `GetDefaultConfigPath()`。既有 `Configuration` 对象重载仍依赖 `System.Configuration`，NativeAOT 使用文件路径或快照重载。

## 注意事项

- 目标框架 net10.0，Json 能力使用框架内置实现，无 System.Text.Json 包依赖。
- 7z 能力面向常见压缩格式；分卷与加密压缩请评估 SharpCompress 能力边界。
