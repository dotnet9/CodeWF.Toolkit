# Toolkit 原生演示台

`design/` 原型冻结。图形 Demo 用真实 Toolkit API 展示基础设施如何组成工作流，复用 Semi、Dock 与 CodeWF.Avalonia 的现有 UI。

## 首页实验

- **事件流**：真正订阅、发布与退订 `GreetCommand`，显示接收计数和日志；动画按发布、路由、接收与日志阶段推进。
- **字节编码**：Int32 经 NetWeaver 序列化和反序列化，显示小端字节与往返结果，支持负数、边界值和无效输入。
- **文件流水线**：通过 Core 源生成 JSON、Files 转 YAML、SevenZipCompressor 打 ZIP、Image 生成二维码。每次在系统临时目录创建独立文件夹，可直接打开产物。
- 六张能力卡片进入完整 Core、EventBus、Files、Image、Logging、NetWeaver 演示。原有导航、属性编辑、源码、场景工作流及布局存档仍可使用。

默认 .NET 紫色暗色；AI 极光（紫 / 青渐变）和 AI 暖陶也支持深浅模式，原有六套 Semi 配色沿用原来的行为。动画使用 Avalonia 原生能力。首页状态与命令在 `WelcomeViewModel`，视图后置仅做初始化和平台接线。

## 源码构建依赖

新增共享 UI 与外观接口归 `CodeWF.Avalonia`，避免在 Toolkit 重复实现 UI 工具。当前尚未发布包含这些接口的 NuGet 版本，因此源码开发默认引用相邻的 Avalonia 仓库：

```text
workspace/
  CodeWF.Avalonia/
  CodeWF.Toolkit/
```

```powershell
git clone https://github.com/dotnet9/CodeWF.Avalonia.git
git clone https://github.com/dotnet9/CodeWF.Toolkit.git
cd CodeWF.Toolkit
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia
```

其他位置可通过 MSBuild 的 `CodeWFAvaloniaSource` 属性或同名环境变量指定：

```powershell
dotnet build demo/CodeWF.Toolkit.Demo.Avalonia -p:CodeWFAvaloniaSource=C:/source/CodeWF.Avalonia
```

只有图形 Demo 需要这项源码依赖；Toolkit 的 11 个生产库与控制台演示仍独立构建。CI 和现有发布构建通过 `.github/actions/checkout-studio` 检出固定 Avalonia 提交，并设置源码路径，确保不依赖执行机器的目录或主分支后续变更。

`UseLocalCodeWFAvalonia=false` 保留未来 NuGet 消费入口；**当前中央包版本不包含新增 Studio 接口，不能直接用这个开关替代源码**。待兼容包发布后再更新中央版本与依赖方式。本次改造不执行 NuGet 发布。

## 国际化与验证

中文显示文本与代码示例位于 `demo/CodeWF.Toolkit.Demo.Avalonia/I18n/`；AXAML 使用 I18n，C# 组合视图的固定文案用 `StudioUi` 的 `@资源键` 绑定。切换中英文会更新标题、配色名称、固定文案和属性面板，保留用户编辑及产物结果。

```powershell
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia -- --smoke
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia -- --capture ./artifacts/studio-qa
dotnet test tests/CodeWF.Toolkit.Tests --filter FullyQualifiedName~DemoResourceTests
```

首页冒烟检查真实总线接收 / 退订、Int32 极值往返、无效输入、ZIP 条目、二维码文件和六个导航入口；其余冒烟逐页 / 逐主题渲染并检查场景步骤。测试验证双语键集合、固定文案缺键和英文资源。截图模式覆盖三套新配色的深浅、三个实验页签、英文、能力卡片与最小窗口，结束后退出。

日志集合通过 UI 线程追加并保持容量上限，避免后台写日志导致视图集合异常。运行验证以 Windows / .NET 10 为基准；既有格式转换示例仍有反射 / AOT 分析警告，跨平台与 NativeAOT 发布需单独验证。
