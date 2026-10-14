# CodeWF.Toolkit Demo 设计

直接打开 `CodeWF.Toolkit.Demo原型.html`。原型与原生 Demo 共用 `catalog.json` 目录；Ribbon 与导航树读取同一份分类。

## 信息架构

按功能分类，每个主导航叶子对应一个具体帮助类或公开 API；介绍、关于和综合示例是明确的辅助入口。保留 Ribbon + Dock 外壳，将组合工作流集中到最后一组，首页先展示 API 目录，再提供默认折叠的实验区。

| Ribbon 分类 | 菜单对应 API |
|---|---|
| 首页 / Home | `介绍`、`关于` |
| 核心扩展 / Core Extensions | `StringExtensions`、`RegexExtensions`、`DoubleExtensions`、`NumberExtension`、`DateTimeExtension`、`TimeSpanExtensions`、`RandomExtension`、`IConvertibleExtensions`、`ObjectExtension`、`LinqExtension`、`EnumExtension`、`AssemblyExtensions`、`ValidationExtension`、`DateTimeConverter` |
| 系统与文件 / System & Files | `ActionHelper`、`IpHelper`、`NativeLibraryHelper`、`FileHelper`、`AppDataMigrator`、`UpdateChecker`、`FileExtension`、`SevenZipCompressor`、`AppConfigHelper` |
| 数据与图像 / Data & Images | `JsonExtensions`、`YamlExtensions`、`DataTableExtensions`、`ImageHelper`、`QrCodeGenerator` |
| 事件与集成 / Events & Integration | `EventBus`、`Prism.EventBusExtensions`、`AspNetCore.EventBusExtensions`、`EventServer`、`EventClient` |
| 协议与网络 / Protocol & Network | `SerializeHelper`、`TcpSocketServer`、`TcpSocketClient`、`UdpSocketServer`、`UdpSocketClient`、`NetHelper`、`TcpSocketClientFileSystemFeature`、`TcpSocketServerFileSystemFeature` |
| 日志与观测 / Logging & Diagnostics | `Logger`、`CodeWFLoggingBuilderExtensions`、`LogEventFeed`、`CodeWFLogHealth` |
| 综合示例 / Examples | `事件与日志导览`、`文件处理工作流` |

共 8 个分类、49 个页面。文档区默认完整展示功能演示；源码面板由“查看源码”打开，只提供有内容的 XAML / C# 标签。页面标题展示 API 名、NuGet 包与功能说明。

## 配色与交互

默认 .NET 品牌紫 `#512BD4`，另有 AI Aurora（紫 / 青）与 AI Ember（暖陶）视觉主题，均支持深浅切换。AI 配色是视觉设计名称。标题栏色圈与下拉框共享选中状态，继续提供 Semi 原有配色。

保留主题、语言、密度、命令搜索、布局存档、Dock 面板显隐与引导。页面进入和按钮悬停有动画，并遵循浏览器“减少动态效果”设置。主导航菜单与首页目录点击后同步当前 Ribbon 分类。

## 维护

1. 在 `catalog.json` 维护分类、API、NuGet 包、页面工厂说明和原型路由。
2. 在 Demo 的 `I18n/` 配置中维护双语标题与 `Demo.Api.*.Desc` 功能说明。
3. 为原生 `DemoCatalog` 添加显式页面工厂，为原型注册对应演示。
4. 执行 `pwsh scripts/sync_design_catalog.ps1` 更新浏览器目录与文案。
5. 执行目录一致性测试与 Demo 冒烟检查。

`assets/catalog-data.js` 是生成文件；`catalog-gallery.js` 和 `catalog-gallery.css` 负责目录布局与浏览器预览。各仓库独立携带所有原型资源。

## 实现与验证边界

浏览器原型展示交互与布局，不运行 .NET、原生窗口、Socket 或文件导出。页面明确提示仅为浏览器预览；实际 API 结果在原生 Demo 中产生。原生控件使用 Semi 与 Dock 的模板，字体、窗口按钮和平台交互存在差异。

浏览器逐页检查 49 个路由、中英文和导航切换，未发现脚本异常。NativeAOT 运行范围、命令与限制见 [原生 Demo 文档](../docs/NativeDemo.md)。

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
