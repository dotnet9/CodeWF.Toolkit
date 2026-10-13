# Toolkit 原生演示台

`design/` 原型冻结。图形 Demo 用真实 Toolkit API 展示基础设施如何组成工作流，复用 Semi、Dock 与 CodeWF.Avalonia 的现有 UI。

## 首页实验

- **事件流**：真正订阅、发布与退订 `GreetCommand`，显示接收计数和日志；动画按发布、路由、接收与日志阶段推进。
- **字节编码**：Int32 经 NetWeaver 序列化和反序列化，显示小端字节与往返结果，支持负数、边界值和无效输入。
- **文件流水线**：通过 Core 源生成 JSON、Files 转 YAML、SevenZipCompressor 打 ZIP、Image 生成二维码。每次在系统临时目录创建独立文件夹，可直接打开产物。
- 六张能力卡片进入事件驱动、跨进程消息、二进制协议、文件与数据、图像工具链、运行期观测页面。原有导航、属性编辑、源码、场景工作流及布局存档仍可使用。

默认 .NET 紫色暗色；AI 极光（紫 / 青渐变）和 AI 暖陶也支持深浅模式，原有六套 Semi 配色沿用原来的行为。动画使用 Avalonia 原生能力。首页状态与命令在 `WelcomeViewModel`，视图后置仅做初始化和平台接线。

## 独立构建与 NuGet 依赖

共享 UI 与外观接口归 `CodeWF.Avalonia`。图形 Demo 通过 NuGet 使用 Controls、TreeDataGrid、Dock、Lang、Log，版本固定为 `12.1.4.6`；没有跨仓库 ProjectReference、源码路径属性或额外检出步骤。只需克隆本仓：

```powershell
git clone https://github.com/dotnet9/CodeWF.Toolkit.git
cd CodeWF.Toolkit
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia
```

本地联调可将这五个 UI 项目打包到仓库外的临时目录，用外部 NuGet 配置执行一次 `dotnet restore --configfile <临时配置>`，随后构建和 AOT 发布均消费真实 nupkg。仓库不携带本地包源、绝对路径或相邻源码要求；CI / 发布使用 nuget.org。升级依赖时先发布 Avalonia 的兼容版本，再发布 Toolkit。

Toolkit 的 11 个生产库仍没有 UI 依赖，Demo 通过同仓 ProjectReference 演示当前实现。Prism 集成使用 `DryIoc.dll 4.8.0`，与既有 Prism 的 DryIoc DLL 一致，避免源码包重复编入同名类型。

## 国际化与验证

中文显示文本与代码示例位于 `demo/CodeWF.Toolkit.Demo.Avalonia/I18n/`；AXAML 使用 I18n，C# 组合视图的固定文案用 `StudioUi` 的 `@资源键` 绑定。切换中英文会更新标题、配色名称、固定文案和属性面板，保留用户编辑及产物结果。

```powershell
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia -- --smoke
dotnet run --project demo/CodeWF.Toolkit.Demo.Avalonia -- --capture ./artifacts/studio-qa
dotnet test tests/CodeWF.Toolkit.Tests --filter FullyQualifiedName~DemoResourceTests
```

首页冒烟检查真实总线接收 / 退订、Int32 极值往返、无效输入、源生成 JSON / YAML 往返、ZIP 条目、二维码与多尺寸 ICO 文件、六个导航入口。另以本机回环 TCP 验证数据包收发和 Socket 事件总线实际握手、订阅、投递、停止；Prism 和 ASP.NET Core 按钮执行真实 DI 注册、扫描与 Query，不启动 HTTP 服务。其余冒烟逐页 / 逐主题渲染并检查场景步骤。

测试验证双语键集合、固定文案缺键和英文资源。截图模式覆盖三套新配色的深浅、三个实验页签、英文、能力卡片、最小窗口及完整功能页面，结束后退出。

日志集合通过 UI 线程追加并保持容量上限。NetWrapper 后台收发与停止流程不捕获调用者的 UI 同步上下文，回归测试使用不泵消息的上下文验证同步停止不会死锁。Socket 内部协议类型显式保留反射元数据，防止 AOT 握手数据被裁剪。

## 原型与库功能映射

| 原型入口 | 原生页面 | 生产库 / 实际调用 |
|---|---|---|
| 事件驱动 | Bus、首页事件流 | EventBus 的 Publish / Subscribe / Unsubscribe / Query |
| 事件驱动的宿主集成 | BusIntegrations | EventBus.Prism 的 AddEventBus / UseEventBus；真实 DryIoc 注册与 Query |
| 事件驱动的宿主集成 | BusIntegrations | EventBus.AspNetCore 的 AddEventBus / UseEventBus；ServiceProvider 注册与 Query |
| 跨进程消息 | BusIntegrations、RuntimeFlow | EventBus.Socket 的 broker、握手、订阅、发布与停止 |
| 二进制协议 | NetPacket、首页字节编码 | NetWeaver 字段声明、对象序列化与 Int32 极值往返 |
| 二进制协议的传输 | NetSocket、RuntimeFlow | NetWrapper TCP / UDP 页面；冒烟真实验证回环 TCP |
| 文件与数据 | FilesFormat、FilesArchive、FilesIndex、首页流水线 | Files 的 JSON / YAML / CSV / Excel 入口、归档与索引；冒烟验证 JSON / YAML / ZIP |
| 图像工具链 | ImageIcon、ImageQr、LabFlow | Image 的图标 / 转换 / 二维码入口；冒烟生成 QR 与多尺寸 ICO |
| 运行期观测 | LogCore、LogOptions、底部日志 | Logging 配置、健康快照、文件日志与 UI 事件流 |
| 运行期观测的 MEL 桥接 | LogMicrosoft、RuntimeFlow | Logging.Microsoft 的 AddCodeWF、结构化日志与模板解析 |
| 运行期观测的核心工具 | Core、首页流水线 | Core 的 JsonTypeInfo 序列化、编码 / 扩展及目录 / 更新检查入口 |

原型已经包含这些库功能。本轮新增的注册按钮、真实 API 冒烟与生成产物使既有入口可执行，没有扩展原型的产品范围，`design/` 保持冻结。原生界面沿用 Semi / Dock 控件模板，窗口按钮、属性编辑器与字体存在平台差异，不宣称像素级一致。

## Windows NativeAOT 验收

当前环境为 Windows x64、.NET SDK 10.0.401，Toolkit 版本 `1.0.0.5`。执行各自的仓库脚本，然后直接启动原生 EXE：

```powershell
pwsh scripts/publish_toolkit_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Toolkit.Demo.Avalonia/CodeWF.Toolkit.Demo.Avalonia.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture ./artifacts/studio-qa' -Wait -PassThru
```

退出 0 才算通过。冒烟包含上述实调用、16 个页面、9 个场景步骤、208 次页面渲染和 5 次外壳渲染；日志位于系统临时目录 `CodeWFToolkitDemo/logs`。README 的 PNG 来自原生产物，GIF 由主题 / 实验切换时的原生界面帧生成。

第三方库和反射路径仍有裁剪 / AOT 分析警告，包括旧 Prism 对话框与动态 XAML Loader 的版本兼容警告。当前 Demo 不走这两条路径。验收不等同于所有库 API、异常路径与 CSV / Excel、UDP、断线补发等操作均已在 NativeAOT 下验证；Linux / macOS 留给发布矩阵验证。`v1.0.0.5` 标签触发 NuGet 和 Demo 发布。
