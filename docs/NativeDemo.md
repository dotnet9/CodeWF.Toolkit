# CodeWF.Toolkit 原生 Demo

保留 Ribbon + Dock，按功能分类查找单个帮助类。原型与实现由 `design/catalog.json` 共享导航契约，详细清单见 [设计目录](../design/README.md)。

## 页面结构

- 首页仅包含介绍、关于；介绍先展示 API 目录。
- 主功能组的一个菜单叶子对应一个帮助类或公开 API。
- 综合示例统一放在末组，首页的 真实事件总线、Int32 编解码和文件流水线 默认折叠。
- 演示始终可见；源码按需展开，有内容才显示相应语言标签。
- 标题栏色圈和下拉同步切换 .NET 紫、AI 极光、AI 暖陶与 Semi 原有配色。

## 独立构建与 NuGet 依赖

共享 UI 与外观接口归 `CodeWF.Avalonia`。图形 Demo 通过 NuGet 使用 Controls、TreeDataGrid、Dock、Lang、Log，版本固定为 `12.1.4.7`；没有跨仓库 ProjectReference、源码路径属性或额外检出步骤。只需克隆本仓：

```powershell
git clone https://github.com/dotnet9/CodeWF.Toolkit.git
cd CodeWF.Toolkit
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia
```

本地联调可将这五个 UI 项目打包到仓库外的临时目录，用外部 NuGet 配置执行一次 `dotnet restore --configfile <临时配置>`，随后构建和 AOT 发布均消费真实 nupkg。仓库不携带本地包源、绝对路径或相邻源码要求；CI / 发布使用 nuget.org。升级依赖时先发布 Avalonia 的兼容版本，再发布 Toolkit。

Toolkit 的 11 个生产库仍没有 UI 依赖，Demo 通过同仓 ProjectReference 演示当前实现。Prism 集成使用 `DryIoc.dll 4.8.0`，与既有 Prism 的 DryIoc DLL 一致，避免源码包重复编入同名类型。

## MVVM 与国际化

帮助类页面由 `HelperDemoViewModel` 管理输入、异步运行、忙碌状态和结果；`HelperDemoCatalog` 显式调用生产 API。AXAML 后置只保留视图接线，生产工具仍归对应 Toolkit 包。固定中文文案位于 `I18n/` 资源文件，切换语言不会覆盖用户输入与执行结果。


## 目录与回归检查

目录以源生成 JSON 元数据读取，页面工厂使用显式分支，避免 AOT 中依赖反射发现页面。测试检查重复菜单 / API、双语标题、显式工厂和原型生成数据一致性；修改目录后执行 `pwsh scripts/sync_design_catalog.ps1`。

```powershell
dotnet test CodeWF.Toolkit.slnx -c Release
```

## Windows NativeAOT 验收

验收环境为 Windows x64、.NET SDK 10.0.401，版本 `1.0.0.6`。用本仓脚本发布后直接启动原生 EXE：

```powershell
pwsh scripts/publish_toolkit_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Toolkit.Demo.Avalonia/CodeWF.Toolkit.Demo.Avalonia.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture ./artifacts/studio-qa' -Wait -PassThru
```

冒烟退出 0：49 个页面、9 个场景步骤、637 次主题渲染、5 次外壳渲染；另外验证首页总线退订、Int32 极值往返、JSON / YAML、ZIP、QR / ICO，以及新增帮助类的实调用。TCP、UDP、Socket 事件总线与远程目录请求使用本机实例；Windows NativeLibraryHelper 解析 kernel32 的导出地址。UpdateChecker 需要用户点击才访问 GitHub，不纳入离线冒烟。

`--capture` 忽略用户存档布局，输出新配色的深浅、实验、英文、紧凑窗口和独立 API 页面截图，结束后退出。README 的 PNG 与 GIF 来自本轮原生 EXE。

## 验收限制

第三方库及动态反射路径仍有裁剪 / AOT 分析警告。远程文件上传、下载、断点续传及 Excel 等未在本轮原生冒烟中逐项验证；Linux / macOS 未在本机验收。NativeLibraryHelper 目前只支持 Windows。 页面渲染通过不等于全部 API、异常路径和平台行为均已验证。

版本标签 `v1.0.0.6` 触发现有 NuGet 与 Demo 发布流程。
