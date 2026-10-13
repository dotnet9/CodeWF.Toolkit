# 更新日志

## 1.0.0.4 (2026-10-13)

- 🔨[优化]-升级 CodeWF.Avalonia 依赖到 12.1.4.5：该版本修掉主题令牌在渲染期解析失败的缺陷（Light / Dark 变体字典以 `StaticResource` 别名引用 `SemiColor*`、`WindowDefaultBackground` 时看不到应用级 Semi 令牌，高对比色板又缺这组令牌），导引、文档流程等页面不再在测量排布时抛异常。
- 🧪[验证]-演示台冒烟升级为逐页面 × 逐色板渲染：16 页 × 7 变体 = 112 次，失败 0；CI 的 Avalonia 演示台 AOT 冒烟不再 `continue-on-error`。

## 1.0.0.3 (2026-10-13)

- 🚀[发布]-全平台安装包：推送 v* 标签即由 GitHub Actions 产出 5 个 RID 的 NativeAOT 产物，Windows 出 Inno Setup 安装向导与便携 zip，Linux 出 deb，macOS 出 dmg，均附 sha256；本地同款脚本在 scripts/ 下（publish_toolkit_demo.ps1、package_linux_deb.ps1、package_macos.sh、package_demo_artifacts.ps1、CodeWF.Toolkit.Demo.iss）。
- 🔨[优化]-CI 的 Avalonia 演示台冒烟改用发布脚本，产物路径与发布流水线保持一致。

- ✨[新功能]-新增桌面演示台 demo/CodeWF.Toolkit.Demo.Avalonia：与 design 原型同款外壳（CodeWFWindow 标题栏 + Ribbon 页签 + Dock 工作区 + PropertyGrid + LogView），12 个演示页覆盖 Core/EventBus/Files/Image/Logging/Net 六条产品线，全部调用仓库真实 API。

## 1.0.0.2 (2026-10-11)

- 🛡[加固]-EventBus：`Publish` 在校验通过后若任务未同步完成（发布期间并发订阅了异步处理器），不再阻塞等待以防 UI 线程死锁，改为与异步处理器一致的快速失败。

## 1.0.0.0-preview.1 (2026-10-10)

- 🚀[发布]-首个预览版：11 个包覆盖 Core、Files、Image、Logging、EventBus、Net 六条产品线，目标框架 net10.0。
- ✨[新功能]-Core 提供原生扩展、应用数据目录与 GitHub Release 更新检查；Files 提供多格式读写与压缩；Image 提供图片处理与条码生成。
- ✨[新功能]-EventBus 提供进程内事件总线，附 Prism、ASP.NET Core 与 TCP 分布式三种集成形态；Net 提供二进制序列化与 TCP/UDP 封装（含远程文件系统）。
- 🔨[优化]-全仓中央包版本管理，包内文档随仓库根 README 生成，仓库内依赖全部使用 ProjectReference。
- 🧪[验证]-单元测试 226 个（逻辑 152 + 网络 74）全部通过；CI 增加 Demo 的 AOT 发布与冒烟运行作业。
