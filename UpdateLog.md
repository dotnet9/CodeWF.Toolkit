# 更新日志

## 未发布

## 1.0.0.0-preview.1 (2026-10-10)

- 🚀[发布]-首个预览版：11 个包覆盖 Core、Files、Image、Logging、EventBus、Net 六条产品线，目标框架 net10.0。
- ✨[新功能]-Core 提供原生扩展、应用数据目录与 GitHub Release 更新检查；Files 提供多格式读写与压缩；Image 提供图片处理与条码生成。
- ✨[新功能]-EventBus 提供进程内事件总线，附 Prism、ASP.NET Core 与 TCP 分布式三种集成形态；Net 提供二进制序列化与 TCP/UDP 封装（含远程文件系统）。
- 🔨[优化]-全仓中央包版本管理，包内文档随仓库根 README 生成，仓库内依赖全部使用 ProjectReference。
- 🧪[验证]-单元测试 226 个（逻辑 152 + 网络 74）全部通过；CI 增加 Demo 的 AOT 发布与冒烟运行作业。
