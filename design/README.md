# CodeWF.Toolkit Demo 设计

直接打开 `CodeWF.Toolkit.Demo原型.html`。原型与原生 Demo 共用 `catalog.json` 目录；Ribbon 与导航树读取同一份分类。

## 信息架构

按功能分类，每个主导航叶子对应一个具体帮助类或公开 API；介绍、关于和综合示例是明确的辅助入口。保留 Ribbon + Dock 外壳，将组合工作流集中到最后一组，首页先展示 API 目录，再提供默认折叠的实验区。

| Ribbon 分类 | 菜单对应 API |
|---|---|
| 首页 / Home | `介绍`、`关于` |
| 核心扩展 / Core Extensions | `StringExtensions`、`RegexExtensions`、`DoubleExtensions`、`NumberExtension`、`RandomExtension`、`DateTimeExtension`、`TimeSpanExtensions`、`DateTimeConverter`、`IConvertibleExtensions`、`ObjectExtension`、`LinqExtension`、`EnumExtension`、`AssemblyExtensions`、`ValidationExtension` |
| 系统与文件 / System & Files | `ActionHelper`、`NativeLibraryHelper`、`FileHelper`、`FileExtension`、`SevenZipCompressor`、`AppDataMigrator`、`UpdateChecker`、`UpdateService`、`AppConfigHelper` |
| 数据与图像 / Data & Images | `JsonExtensions`、`YamlExtensions`、`DataTableExtensions`、`ImageHelper`、`QrCodeGenerator` |
| 国际化 / Localization | `I18nManager`、`JsonLocalizer` |
| 事件与集成 / Events & Integration | `EventBus`、`Prism.EventBusExtensions`、`AspNetCore.EventBusExtensions`、`EventServer`、`EventClient` |
| 协议与网络 / Protocol & Network | `SerializeHelper`、`NetHelper`、`IpHelper`、`TcpSocketServer`、`TcpSocketClient`、`UdpSocketServer`、`UdpSocketClient`、`TcpSocketClientFileSystemFeature`、`TcpSocketServerFileSystemFeature` |
| 日志与观测 / Logging & Diagnostics | `Logger`、`CodeWFLoggingBuilderExtensions`、`LogEventFeed`、`CodeWFLogHealth` |
| 综合示例 / Examples | `事件与日志导览`、`文件处理工作流` |

共 9 个分类、52 个页面。文档区默认完整展示功能演示；源码面板由“查看源码”打开，只提供有内容的 XAML / C# 标签。页面标题展示 API 名、NuGet 包与功能说明。

## 配色与交互

默认 .NET 品牌紫 `#512BD4`，常规配色共八套：.NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏、夜空。浅色、深色、跟随系统独立选择；色圈、下拉和文档属性共用状态，系统模式响应系统主题变化，布局保存模式选择。AI 配色是视觉设计名称。

保留主题、语言、密度、命令搜索、布局存档、Dock 面板显隐与引导。页面进入和按钮悬停有动画，并遵循浏览器“减少动态效果”设置。主导航菜单与首页目录点击后同步当前 Ribbon 分类。

## 维护

1. 在 `catalog.json` 维护分类、API、包名、页面构造表达式、路由与图标。`icon` 是导航字形，`iconKey` 引用 Semi 原生矢量资源，`prototypeIcon` 是对应 MIT 图标的浏览器 SVG 内容，两者均由目录管理。
2. 在 Demo 的 `I18n/` JSON 中维护中英文标题和说明。资源键使用稳定的业务语义，例如 `Demo.Shell.GlobalSettings.PaletteLabel`；不要使用方法名加位置编号，修改文案时保留原键。
3. 实现原生页面，并为原型注册交互演示。执行 `pwsh scripts/sync_design_catalog.ps1`，同步浏览器数据与 `DemoCatalog.g.cs`。页面标识 `DemoPages` 和显式工厂由脚本生成，无需手工维护 switch 或图标字典。
4. 执行 `pwsh scripts/sync_design_catalog.ps1 -Check`、目录一致性测试和原生 Demo 冒烟。生成文件需要提交，正常 `dotnet build` 不依赖 PowerShell 或 Node.js。
5. 执行 `npm ci --prefix design/tests`，在 `design/tests` 运行 `npx playwright install chromium` 和 `npm test`。浏览器门禁遍历全部路由 × 中英文 × 1440/1120 两种宽度，检查实际 Ribbon/导航点击、页面内容、源码面板和脚本错误。失败截图保存在 `design/tests/artifacts/`；CI 使用同一脚本。

`design/catalog.json` 是 Demo 的嵌入式构建输入，属于仓库源码，不能当草稿删除。`scripts/prototype/assets/catalog-data.js` 与 `DemoCatalog.g.cs` 是生成文件；`catalog-gallery.js/css` 负责浏览器布局。每个仓库独立携带全部资源。资源文件采用 UTF-8，允许有或无 BOM；测试检查无效字节和替换字符，但不能识别所有已经被错误转码的合法文字。

## 实现与验证边界

浏览器原型展示交互与布局，不运行 .NET、原生窗口、Socket 或文件导出。页面明确提示仅为浏览器预览；实际 API 结果在原生 Demo 中产生。原生控件使用 Semi 与 Dock 的模板，字体、窗口按钮和平台交互存在差异。

浏览器逐页检查 52 个路由、中英文和导航切换，未发现脚本异常。NativeAOT 运行范围、命令与限制见 [原生 Demo 文档](../docs/NativeDemo.md)。

## 重点页面用例

一个帮助类保留一个主菜单，方法和场景放在页内。

| API | 页面用例 |
|---|---|
| DataTableExtensions | CSV / Excel 分别导出并回读、表格预览、单元格一致性与真实文件路径 |
| ActionHelper | 输入操作耗时与等待上限；true 为限期内完成，false 为等待超时，后台操作不会被取消 |
| TcpSocketClientFileSystemFeature | 本机会话、目录浏览 / 创建 / 删除、上传 / 下载、进度 / 取消与 SHA256 校验 |
| TcpSocketServerFileSystemFeature | 随机本机端口与独立 RootDirectory，以配套客户端请求观察服务端真实响应 |
| TCP / UDP | 启动、保持连接、重复发送与接收校验、断开 / 停止 |
| EventServer / EventClient | 会话生命周期、发布 / 订阅 / 退订与再次订阅 |

`assets/helper-examples.js` 展示以上浏览器交互预览，不发送 Socket 请求或生成 Excel。实际结果来自原生 Demo，浏览器结果标记为预览。文件操作限于本次会话的临时目录，关闭文档、重置布局或关闭窗口会停止相关会话。

“事件与日志导览”组织独立示例，各步骤拥有自己的状态与连接，不宣称共享同一运行会话。Ribbon、导航与 Dock 当前文档保持选中状态一致；无页面参数时右侧显示全局设置。

## 设计判断与验收标准

菜单以帮助类 / API 为单位，分类以用途为单位：数据导入导出放在数据与图像，事件传播放在事件与集成，传输协议和远程文件请求放在协议与网络。接口、响应模型和内部实现类型由所属帮助类页面说明；介绍、关于及综合示例不计入帮助类覆盖率。

每页先说明输入，再提供真实调用及可核对的结果。同一个帮助类的 CSV / Excel、发送 / 接收、订阅 / 退订等用例放在页内。需要连接或传输的 API 提供显式会话与停止操作，避免把一次性文本输出当成完整功能演示。

还原检查包括首屏可见、表格列头与数据行、选中状态、深浅主题、中英文、紧凑窗口，以及调用结果。原型中的浏览器预览不等于 API 已执行；原生 Demo 的回读、接收和哈希检查作为对应功能的运行证据。界面依赖从公开 NuGet 安装，单独拉取 Toolkit 即可构建。


## 排版与 Ribbon 校准

保留 9 个一级分类，在命令区显示明确的功能子组；每个菜单仍对应单个 API。超过 7 项的分类使用双行命令，保留完整 API 名，窗口较窄时横向滚动。分类标题 12px、命令 11px、子组标题 10px；标题栏 52px、分类行 42px、命令区约 104px。页头标题 19px、说明 12px，导航叶子 11.5px。

原型对照时裁掉 HTML 的装饰性桌面背景，使用 1440×860 和 1120×640 两种窗口尺寸、中英文 72 个 Ribbon 状态。原生继续使用 Semi / Dock 模板；窗口按钮、PropertyGrid 编辑器和平台字体的几何不做 HTML 仿制。不能把路由检查或截图数量当作全状态逐像素相同的证明。

IpHelper 归到协议与网络。核心扩展按文本、数值、日期时间、类型与查询分组，系统与文件按执行、文件、应用配置分组；每个帮助类在页内展示参数、实际调用和结果，网络页提供持续会话。

属性栏只显示该页参数，介绍页提供全局外观与语言设置；不额外堆放空参数和状态统计。全局标签、密度选项和配色名称也从原生语言资源生成。

Ribbon 命令图标统一为 32×32 Semi 矢量图标，快捷图标 18×18、按钮热区 30×30；组仅显示标题。Semi 图形资源遵循其 MIT 许可。

快捷区使用 Semi 线性图标、2px 间距和独立分隔留白；鼠标悬停、按下及键盘焦点各有反馈。Ribbon 分类复用 Semi LineTabStrip，单层 13px 水平内边距。标题栏密度与通知入口复用 Semi 图标；导航叶子与 Ribbon 使用同一目录图形。应用默认字体覆盖 Dock 浮动窗和弹出层，代码区域仍使用等宽字体。

## 单文件交付

`design/` 中的主 HTML 已内嵌样式、脚本、图标和图片。只复制这一个 HTML 到空目录即可离线查看并操作，无需旁边的资源文件或本地服务。

维护源码放在 `scripts/prototype/`；执行 `pwsh scripts/build_standalone_prototype.ps1` 重新生成，`-Check` 校验生成文件。不要直接修改交付 HTML。交互审计会复制 HTML 到空目录，阻止其他文件和网络资源请求，并检查脚本错误与图片完整性。
