# CodeWF.Toolkit.Image

图片处理集合：基于 Magick.NET 的缩放/水印/格式转换，以及 ZXing 条码与二维码生成。Image processing powered by Magick.NET with ZXing barcode and QR code generation.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Toolkit.Image?label=nuget)](https://www.nuget.org/packages/CodeWF.Toolkit.Image)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Toolkit/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Toolkit.Image
```

依赖 Magick.NET-Q16-AnyCPU 与 ZXing.Net.Bindings.Magick（随包自动安装）；目标框架 net10.0。

## 能力一览

| API | 说明 |
|---|---|
| `ImageHelper` | 格式转换、缩放、水印合成、图标生成：`MergeGenerateIcon` / `SeparateGenerateIcon` 多尺寸 .ico 合成与拆分 |
| `QrCodeGenerator` | 基于 ZXing 的条码与二维码生成 |

## 示例

```csharp
// 由 PNG 生成包含多尺寸的 Windows 图标
await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [24, 32, 48]);

// 从 ICO 拆回各尺寸 PNG
await ImageHelper.SeparateGenerateIcon("logo.ico", outputFolder, [24, 32]);

QrCodeGenerator.GenerateQrCode("CodeWF", "https://codewf.com", "qr.png");
```

## 注意事项

- Magick.NET 携带原生二进制（Q16-AnyCPU），包体积较大，请按需独立引用而非并入聚合包。
- 条码/二维码渲染输出 MagickImage，可与 `ImageHelper` 无缝衔接。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Toolkit/blob/main/docs/CodeWF.Toolkit.Image%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Toolkit>
