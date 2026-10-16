# 当前原生截图来源

2026-10-16，版本 `1.0.0.25`。README 的两张 Dock PNG 从本轮 Windows x64 NativeAOT UI 验证器的原始 Skia 捕获逐字节复制，展示同一个文档内的真实演示控件、属性面板及停靠状态。它们不是浏览器原型或操作系统桌面截图。

| 仓库图片 | 原生捕获文件 | SHA-256（复制前后相同） |
| --- | --- | --- |
| `docs/images/studio-dock-redocked.png` | `dock-captures/redocked.png` | `53f7945a841f88dc3a980e94df8f185d9d4c6301953c7813f422faec34ac5e3c` |
| `docs/images/studio-dock-floating.png` | `dock-captures/floating.png` | `509cea977bb1a3d667bb144cf5a83b4d07a5a62b9198f0bdfcb3d56e5ae0bb09` |

其余普通页面 PNG 和主题巡览 GIF 保留 `1.0.0.24` 的历史真实控件媒体；本轮没有重生成全套 GIF，也没有将旧图片标记为新版本。

构建使用本轮 main 工作区的 `1.0.0.25` 产品源。验收归档保存了 318 个产品与验证输入文件的 SHA-256，以及实际消费的五个 `12.1.4.25` Avalonia 包的来源和 SHA-256。临时 NuGet 源配置保存在仓库之外，不影响其他人单独克隆和构建。

正式应用经仓库发布脚本生成，实际 EXE 的 `--verify-runtime` 检查版本、两语资源和 52 条内嵌目录。独立 NativeAOT UI 验证器通过 1664 组页面／语言／配色渲染、34 次实际 API 调用及动态切语、真实语言菜单、属性隐藏分支、Dock 分屏／浮动／回停回归。原生配色审计通过 16 种模式、736 次文字对比度测量，失败数为 0。

界面验证通过 Headless + Skia 离屏运行，布局、日志与配置写入独立验收目录，不操作用户桌面和日常 Demo 配置。人工跨屏、多 DPI、输入法与安装流程尚未验证；渲染覆盖和局部布局、对比度检查不代表全页像素还原率。
