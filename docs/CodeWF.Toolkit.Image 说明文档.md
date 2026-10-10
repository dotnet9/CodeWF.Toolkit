# CodeWF.Toolkit.Image

图片处理集合：Magick.NET 缩放/水印/格式转换 + ZXing 条码与二维码。

## 安装

```bash
dotnet add package CodeWF.Toolkit.Image
```

## 能力一览（CodeWF.Toolkit.Image 命名空间）

| API | 说明 |
|---|---|
| ImageHelper | 基于 Magick.NET-Q16-AnyCPU 的图片处理：格式转换、缩放、水印合成、图标生成（MergeGenerateIcon / SeparateGenerateIcon，多尺寸 .ico 拆分与合成） |
| QrCodeGenerator | 基于 ZXing.Net.Bindings.Magick 的二维码生成 |

## 示例

```csharp
// 由 PNG 生成包含多尺寸的 Windows 图标
await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [24, 32, 48]);

// 从 ICO 拆回各尺寸 PNG
await ImageHelper.SeparateGenerateIcon("logo.ico", outputFolder, [24, 32]);
```

## 注意事项

- Magick.NET 携带原生二进制（Q16-AnyCPU），包体积较大，请独立引用而非并入聚合包。
- 条码/二维码渲染输出 MagickImage，可与 ImageHelper 无缝衔接。
