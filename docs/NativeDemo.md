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

验收环境为 Windows x64、.NET SDK 10.0.401，版本 `1.0.0.7`。用本仓脚本发布后直接启动原生 EXE：

```powershell
pwsh scripts/publish_toolkit_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Toolkit.Demo.Avalonia/CodeWF.Toolkit.Demo.Avalonia.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture ./artifacts/studio-qa' -Wait -PassThru
```

冒烟退出 0：49 个页面、9 个场景步骤、637 次主题渲染、5 次外壳渲染；另外验证首页总线退订、Int32 极值往返、JSON / YAML、ZIP、QR / ICO，以及新增帮助类的实调用。TCP、UDP 与 Socket 事件总线验证真实会话的启动、发送、接收和停止，事件页同时验证退订 / 再订阅；文件系统页验证浏览、创建、删除、取消 / 续传、上传 / 下载与 SHA256；表格页验证 CSV / Excel 往返；Windows NativeLibraryHelper 解析 kernel32 的导出地址。UpdateChecker 需要用户点击才访问 GitHub，不纳入离线冒烟。

`--capture` 忽略用户存档布局，输出新配色的深浅、实验、英文、紧凑窗口和独立 API 页面截图，结束后退出。README 的 PNG 与 GIF 来自本轮原生 EXE。

表格与文件页显式加载 `TreeDataGridSemiTheme`，逐主题检查实际列头与数据行；截图等待 Dock 文档加载并验证行已经显示。`studio-interactions.gif` 是主题与实验截图巡览。

## 验收限制

第三方库及动态反射路径仍有裁剪 / AOT 分析警告。本轮文件传输限于本机配套服务和临时目录，未验证跨网段、断网重连及大规模并发；Linux / macOS 未在本机验收。NativeLibraryHelper 目前只支持 Windows。 页面渲染通过不等于全部 API、异常路径和平台行为均已验证。

版本标签 `v1.0.0.7` 触发现有 NuGet 与 Demo 发布流程。

## 本轮改进

- DataTableExtensions 使用真实表格和 CSV / Excel 两类产物；耗时调用在后台执行。
- 网络页改为 MVVM 会话，保留连接供重复操作；文件系统页提供完整操作、进度、取消与结果校验，关闭文档 / 窗口及重置布局时释放资源。
- ActionHelper 展示可控耗时及超时语义。运行态综合页准确命名为“事件与日志导览”，各步骤是独立示例。
- Ribbon / 导航与 Dock 当前文档同步，按钮复用 Semi OutlineButton，无参数页面显示全局设置。
- NuGet 主包通过 `--no-symbols` 单独推送；符号包在独立步骤上传，超时显示警告且不改变主包的成功结果。两类上传均使用 600 秒超时。

自动传输检查使用 256 KB 样例，界面默认提供 4 MB 样例。取消后再次上传复用原文件以便观察续传；完整下载后比较 SHA256。失败、跨平台及长时间运行仍需按实际使用环境补充验证。

本轮记录：2026-10-14，Windows x64 / .NET SDK 10.0.401。本仓 235 项单元测试通过；两个仓库各自发布脚本执行成功，最终原生 EXE 的 `--smoke` 均退出 0。浏览器全部目录页以及重点交互检查无脚本异常。仅使用公开 NuGet 源且没有相邻 Avalonia 仓库的源码快照，独立恢复、构建及完整托管冒烟均通过。
