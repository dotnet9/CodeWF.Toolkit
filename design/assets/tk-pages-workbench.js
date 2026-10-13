/* ═══════════════════════════════════════════════════════════════════════════
   CodeWF.Toolkit.Demo 原型 · 页面：欢迎 / 场景工作台 / 核心与通信字典
   ═══════════════════════════════════════════════════════════════════════════ */
"use strict";

/* 页面注册辅助：html + 属性（沿用原原型的页面内容） */
function RH(id, html, props) { REG(id, { zh: { html: html }, props: props }); }

/* 状态栏：最近检查更新时间 */
function setSync(text) { var el = $("stSync"); if (el) el.textContent = text; }

/* ═══════════════════════════ 页面：欢迎总览 ═══════════════════════════ */
REG("welcome", {
 zh: {
  t: "欢迎总览",
  d: "CodeWF.Toolkit：无 UI 依赖的 .NET 基础设施库合集（11 个包 / 6 大能力域）。演示台把能力拆成「运行态工作台」与「能力实验室」两个连续场景，其余按能力域列在字典页里。",
  f: ["16 页", "2 个场景工作流", "11 个包", "中英双语"]
 },
 render: function () {
  return '<div class="card"><h4>适用场景</h4><table class="tb"><thead><tr><th>场景</th><th>包</th></tr></thead><tbody>' +
   '<tr><td>进程内解耦：发布订阅、Command/Query、<code>[EventHandler]</code> 扫描</td><td><code>EventBus</code>（+ Prism / AspNetCore 集成）</td></tr>' +
   '<tr><td>跨进程消息：TCP 分布式总线，心跳保活、断线重连、离线补发</td><td><code>EventBus.Socket</code></td></tr>' +
   '<tr><td>文件与图片批处理：Json/Yaml/Csv/Excel、7z、多尺寸 ico、二维码</td><td><code>Files</code> / <code>Image</code></td></tr>' +
   '<tr><td>统一日志：控制台 + 文件轮转 + UI 事件流，可桥接 MEL</td><td><code>Logging</code> / <code>Logging.Microsoft</code></td></tr>' +
   '<tr><td>二进制协议与 TCP/UDP 通信（含远程文件系统与断点续传）</td><td><code>NetWeaver</code> / <code>NetWrapper</code></td></tr>' +
   '<tr><td>应用基础设施：数据目录迁移、GitHub Release 更新检查、常用扩展</td><td><code>Core</code></td></tr>' +
   '</tbody></table></div>' +
   '<div class="card"><h4>快速开始</h4><pre class="code">dotnet add package CodeWF.Toolkit.Core' + NL +
   'dotnet add package CodeWF.Toolkit.EventBus' + NL + 'dotnet add package CodeWF.Toolkit.Logging' + NL +
   'dotnet add package CodeWF.Toolkit.Files' + NL + 'dotnet add package CodeWF.Toolkit.Image' + NL +
   'dotnet add package CodeWF.Toolkit.NetWrapper</pre>' +
   '<div class="hint">只有 Logging 依赖 Microsoft.Extensions.Logging.Abstractions；Core / EventBus / NetWeaver 零第三方依赖。</div></div>' +
   '<div class="card"><h4>两种演示形态</h4><table class="tb"><tbody>' +
   '<tr><td><code>demo/CodeWF.Toolkit.Demo</code></td><td>控制台演示：日志 → 总线 → Socket → 序列化，<code>--smoke</code> 供 CI / AOT 冒烟</td></tr>' +
   '<tr><td><code>demo/CodeWF.Toolkit.Demo.Avalonia</code></td><td>图形演示台：即本原型对应的界面，逐页演示同一批 API</td></tr>' +
   '</tbody></table></div>' +
   '<div class="card"><h4>快捷键</h4><div class="hint" style="margin:0">Ctrl+K 命令面板 · Ctrl+S 保存布局 · Ctrl+D 深浅 · Ctrl+L 语言 · F1 引导 · 双击 Ribbon 页签折叠命令区</div></div>';
 }
});

/* ═══════════════ 场景一：运行态工作台 ═══════════════ */
var RT_STEPS = ["启动服务端", "客户端连接", "发布 / 订阅", "查询与集成", "日志与停止"];
var rtStep = 0, rtSub = "bus";
var RT_SUB = ["bus", "bus-integrations", "log-core", "log-microsoft"];
REG("flow-runtime", {
 zh: {
  t: "运行态工作台",
  d: "把「事件总线 + 日志」编成一条连续工作流：启动服务端 → 客户端连接 → 发布订阅 → 查询与集成 → 观测日志与停止。子标签复用能力字典里的同一批页面，底层调用完全一致。",
  f: ["5 步工作流", "总线 + 日志联动", "真实交互"]
 },
 props: [
  { id: "rt.host", label: "监听地址", kind: "ro", value: "127.0.0.1" },
  { id: "rt.port", label: "端口", type: "number", value: 39561, apply: function (v) { var el = $("esPort"); if (el) el.value = v; log2both("端口 = " + v); } },
  { id: "rt.hb", label: "心跳(秒)", type: "number", value: 10, apply: function (v) { var el = $("esHb"); if (el) el.value = v; log2both("心跳 = " + v); } },
  { id: "rt.resend", label: "离线补发", type: "bool", value: true, apply: function (v) { log2both("OfflineResend = " + v); } },
  { id: "rt.heartbeat", label: "心跳保活", type: "bool", value: true, apply: function (v) { log2both("Heartbeat = " + v); } }
 ],
 render: function (el) {
  var up = window._esOn;
  el.innerHTML = '<div class="flow" id="rtFlow">' + RT_STEPS.map(function (s, i) {
   return '<span class="st' + (i === rtStep ? " cur" : i < rtStep ? " done" : "") + '" data-s="' + i + '">' + (i + 1) + ". " + s + "</span>";
  }).join("") + "</div>" +
   '<div class="card"><h4>服务端 / 客户端</h4><div class="g2">' +
   '<div><div class="kv"><span class="k">监听</span><input class="tin" id="esHost" value="127.0.0.1" style="flex:1"></div>' +
   '<div class="kv"><span class="k">端口</span><input class="tin" id="esPort" value="39561" style="flex:1"></div>' +
   '<div class="kv"><span class="k">心跳间隔</span><input class="tin" id="esHb" value="10" style="flex:1"></div>' +
   '<div class="row1"><button class="b pri" onclick="rtStart()">启动服务端（EventServer）</button>' +
   '<button class="b" onclick="rtStopAsk()">停止</button></div>' +
   '<div class="kv"><span class="k">状态</span><span id="esSt">' + (up ? "● 运行中" : "● 已停止") + "</span></div>" +
   '<div class="kv"><span class="k">已连接客户端</span><span id="esClients">' + (up ? "1" : "0") + "</span></div></div>" +
   '<div><div class="kv"><span class="k">主题</span><input class="tin" id="esTopic" value="demo.greeting" style="flex:1"></div>' +
   '<div class="kv"><span class="k">内容</span><input class="tin" id="esMsg" value="hello" style="flex:1"></div>' +
   '<div class="row1"><button class="b" onclick="rtConnect()">客户端连接</button>' +
   '<button class="b" onclick="rtPublish()">Publish</button><button class="b" onclick="rtQuery()">Query&lt;Ping,Pong&gt;</button></div>' +
   '<div class="lgst" id="esFeed"></div></div></div>' +
   '<div class="hint" id="rtState"></div></div>' +
   '<div class="card"><h4>运行态观测</h4><div class="row1">' +
   '<button class="b" onclick="openDoc(\'log-core\')">切到 Logger 页面</button>' +
   '<button class="b" onclick="rtLog(\'info\')">Info</button><button class="b" onclick="rtLog(\'warn\')">Warn</button>' +
   '<button class="b" onclick="rtLog(\'error\')">Error</button>' +
   '<button class="b" onclick="rtCheck()">检查更新（GitHub Release）</button></div>' +
   '<pre class="code" id="rtOut">等待操作</pre>' +
   '<div class="hint">Milestone 3：把「启动 → 连接 → 发布 → 查询 → 观测 → 停止」做成一个连续流程，页内状态与底部日志同步。</div></div>' +
   '<div class="subtabs" id="rtSub">' +
   [["bus", "进程内总线"], ["bus-integrations", "集成形态"], ["log-core", "Logger 核心"], ["log-microsoft", "MEL 接入"]].map(function (t) {
    return '<button data-p="' + t[0] + '"' + (t[0] === rtSub ? ' class="on"' : "") + ">" + t[1] + "</button>";
   }).join("") + '</div><div id="rtBody"></div>';
  el.querySelectorAll("#rtFlow .st").forEach(function (n) {
   n.onclick = function () {
    rtStep = +n.dataset.s; rtSub = RT_SUB[rtStep];
    log2both("工作流步骤 " + (rtStep + 1) + "：" + RT_STEPS[rtStep]);
    renderInto("flow-runtime", $("seg-flow-runtime") || el);
   };
  });
  el.querySelectorAll("#rtSub button").forEach(function (n) {
   n.onclick = function () {
    rtSub = n.dataset.p;
    document.querySelectorAll("#rtSub button").forEach(function (x) { x.classList.toggle("on", x === n); });
    renderInto(rtSub, el.querySelector("#rtBody"));
   };
  });
  var st = el.querySelector("#rtState");
  if (st) st.textContent = "当前步骤 " + (rtStep + 1) + " / 5 · " + RT_STEPS[rtStep] + " · 服务端：" + (up ? "运行中" : "已停止") + " · 子标签 " + pt(rtSub);
  renderInto(rtSub, el.querySelector("#rtBody"));
 }
});

/* ═══════════════ 场景二：能力实验室 ═══════════════ */
var LAB_STEPS = ["文件格式转换", "归档与配置", "图标与二维码", "长任务与导出"];
var labStep = 0, labSub = "files-json";
var LAB_SUB = ["files-json", "files-archive", "image-icon", "image-qr"];
var LAB_ROWS = {};
REG("flow-lab", {
 zh: {
  t: "能力实验室",
  d: "把 Files / Image 的真实 API 编排成一条流水线：格式转换 → 归档与配置 → 图标与二维码 → 长任务导出。每一步都能单独点，也能一键跑完（串行、带进度）。",
  f: ["4 步流水线", "真实脚本编排", "一键全流程"]
 },
 props: [
  { id: "lab.quality", label: "导出质量", type: "number", value: 92, apply: function (v) { var el = $("imgQ"); if (el) el.value = v; log2both("Quality = " + v); } },
  { id: "lab.auto", label: "完成后通知", type: "bool", value: true, apply: function (v) { log2both("NotifyOnDone = " + v); } }
 ],
 render: function (el) {
  el.innerHTML = '<div class="flow" id="labFlow">' + LAB_STEPS.map(function (s, i) {
   return '<span class="st' + (i === labStep ? " cur" : i < labStep ? " done" : "") + '" data-s="' + i + '">' + (i + 1) + ". " + s + "</span>";
  }).join("") + "</div>" +
   '<div class="card"><h4>流水线</h4><div class="row1">' +
   '<button class="b pri" id="labRunBtn" onclick="labRunAll()">一键全流程（4 步）</button>' +
   '<button class="b" onclick="labRunStep()">只跑当前步骤</button>' +
   '<button class="b" onclick="labReset()">重置结果</button></div>' +
   '<div class="barwrap" id="labBar" style="display:none;margin:8px 0"><i></i></div>' +
   '<table class="tb"><thead><tr><th>#</th><th>步骤</th><th>调用</th><th>输出</th><th>状态</th></tr></thead><tbody id="labTb"></tbody></table>' +
   '<div class="hint" id="labMsg">每一步都调用下方子标签里的真实脚本（ToJson/ToYaml、7z 压缩、Icon 合成、二维码生成）。</div></div>' +
   '<div class="subtabs" id="labSub">' +
   [["files-json", "格式读写"], ["files-archive", "压缩 / 配置"], ["image-icon", "图标生成"], ["image-qr", "二维码 / 图片"]].map(function (t) {
    return '<button data-p="' + t[0] + '"' + (t[0] === labSub ? ' class="on"' : "") + ">" + t[1] + "</button>";
   }).join("") + '</div><div id="labBody"></div>';
  el.querySelectorAll("#labFlow .st").forEach(function (n) {
   n.onclick = function () {
    labStep = +n.dataset.s; labSub = LAB_SUB[labStep];
    log2both("流水线步骤 " + (labStep + 1) + "：" + LAB_STEPS[labStep]);
    renderInto("flow-lab", $("seg-flow-lab") || el);
   };
  });
  el.querySelectorAll("#labSub button").forEach(function (n) {
   n.onclick = function () {
    labSub = n.dataset.p;
    document.querySelectorAll("#labSub button").forEach(function (x) { x.classList.toggle("on", x === n); });
    renderInto(labSub, el.querySelector("#labBody"));
   };
  });
  labDraw();
  renderInto(labSub, el.querySelector("#labBody"));
 }
});

/* ═══════════════════════════ 页面：Core 工具箱 ═══════════════════════════ */
RH("core", '<h3 class="dh">Core：扩展方法与基础设施</h3><p class="dsub">零第三方依赖。左侧是可实时试用的扩展方法，右侧是更新检查与数据目录。</p>' +
 '<div class="g2"><div class="card"><h4>字符串 / 数值扩展（可试）</h4>' +
 '<div class="row1"><input class="tin" id="coreIn" style="flex:1" value="CodeWF.Toolkit 是 .NET 扩展集合"></div>' +
 '<div class="row1"><button class="b" onclick="coreRun(\'truncate\')">截断 18</button><button class="b" onclick="coreRun(\'slug\')">转标识符</button><button class="b" onclick="coreRun(\'bytes\')">字节单位</button></div>' +
 '<pre class="code" id="coreOut">点击上方按钮查看结果</pre>' +
 '<table class="tb"><tbody><tr><td><code>StringExtensions</code></td><td>正则快捷判断、截断</td></tr><tr><td><code>DoubleExtensions</code></td><td>数值格式化</td></tr><tr><td><code>ByteUnit</code></td><td>字节 → 可读单位</td></tr><tr><td><code>EnumExtension</code></td><td><code>EnumDescriptionAttribute</code> 取描述</td></tr></tbody></table></div>' +
 '<div class="card"><h4>更新检查 <code>UpdateChecker</code></h4>' +
 '<div class="kv"><span class="k">仓库</span><input class="tin" id="upRepo" value="dotnet9/CodeWF.Toolkit" style="flex:1"></div>' +
 '<div class="kv"><span class="k">当前版本</span><input class="tin" id="upCur" value="1.0.0.0" style="flex:1"></div>' +
 '<div class="row1"><button class="b pri" onclick="upCheck()">检查更新</button><label><input type="checkbox" id="upEtag" checked> 启用 ETag 缓存</label></div>' +
 '<pre class="code" id="upOut">尚未检查</pre>' +
 '<div class="hint">命中 304 时不消耗 API 配额；失败与“无更新”通过 UpdateCheckResult 区分，不误报。</div></div></div>' +
 '<div class="card"><h4>应用数据目录 <code>AppDataMigrator</code></h4><pre class="code">AppDataMigrator.Ensure("CodeWF.Toolkit");   // %LOCALAPPDATA%\\CodeWF.Toolkit' + NL + 'AppDataMigrator.Migrate("OldAppName");      // 换包名后首次启动搬运</pre>' +
 '<table class="tb"><tbody><tr><td><code>FileHelper</code></td><td>GetFileEncodeType 编码探测、安全读写重试、打开文件夹/选中文件</td></tr><tr><td><code>IpHelper</code></td><td>IPv4/IPv6 判断与转换</td></tr><tr><td><code>NativeLibraryHelper</code></td><td>原生库加载路径解析</td></tr></tbody></table></div>',
 [{ id: "core.namespace", name: "命名空间", kind: "ro", value: "CodeWF.Toolkit.Core" },
  { id: "core.regex", name: "启用正则扩展", kind: "sw", value: true },
  { id: "core.etag", name: "ETag 增量检查", kind: "sw", value: true },
  { id: "core.truncate", name: "截断长度", kind: "num", value: 18 }]);

/* ═══════════════════════════ 页面：进程内总线 ═══════════════════════════ */
RH("bus", '<h3 class="dh">进程内事件总线</h3><p class="dsub">发布订阅 + Command/Query。下面的总线是真实运行的：订阅、发布、查询都在浏览器里执行。</p>' +
 '<div class="g2"><div class="card"><h4>会话</h4><div class="lgst" id="busFeed"></div>' +
 '<div class="row1"><input class="tin" id="busMsg" style="flex:1" value="hello"><button class="b pri" onclick="busPublish()">Publish(GreetCommand)</button></div>' +
 '<div class="row1"><button class="b" onclick="busQuery()">Query(TimeQuery)</button><button class="b" onclick="busClear()">清空</button></div>' +
 '<div class="hint">异步发布走 PublishAsync；处理器异常不会静默吞掉。</div></div>' +
 '<div class="card"><h4>订阅表</h4><table class="tb"><thead><tr><th>订阅</th><th>类型</th><th>Order</th><th>状态</th></tr></thead><tbody>' +
 '<tr><td><code>GreetCommand</code></td><td>lambda</td><td>—</td><td id="busSub1">已订阅</td></tr>' +
 '<tr><td><code>GreetCommand</code></td><td>[EventHandler]</td><td>1</td><td id="busSub2">已订阅</td></tr>' +
 '<tr><td><code>TimeQuery</code></td><td>[EventHandler]</td><td>—</td><td id="busSub3">已订阅</td></tr>' +
 '</tbody></table><div class="row1"><button class="b" onclick="busToggle2()" id="busT2Btn">取消第二条订阅</button></div>' +
 '<pre class="code">public sealed class TimeQueryHandler' + NL + '{' + NL + '    [EventHandler]' + NL + '    private void ReceiveTimeQueryAsync(TimeQuery query)' + NL + '        =&gt; query.Result = DateTime.Now;' + NL + '}</pre></div></div>' +
 '<div class="card"><h4>注册方式</h4><table class="tb"><tbody>' +
 '<tr><td><code>EventBus</code></td><td>手动 Subscribe/Publish，零依赖</td></tr>' +
 '<tr><td><code>EventBus.Prism</code></td><td>Prism 容器注册、扫描与启动</td></tr>' +
 '<tr><td><code>EventBus.AspNetCore</code></td><td>DI 注册 + 请求管线启动，处理器自动发现</td></tr>' +
 '<tr><td><code>EventBusRegistrationExtensions</code></td><td>通用 IOC：<code>AddEventBus(addSingleton, addScoped, assemblies)</code> + <code>UseEventBus(resolve, assemblies)</code></td></tr>' +
 '</tbody></table></div>',
 [{ id: "bus.name", name: "总线实例", kind: "ro", value: "EventBus" },
  { id: "bus.order", name: "处理器顺序", kind: "sw", value: true },
  { id: "bus.async", name: "异步发布", kind: "sw", value: true },
  { id: "bus.assembly", name: "程序集扫描", kind: "sw", value: false }]);

/* ═══════════════════════════ 页面：总线集成形态 ═══════════════════════════ */
RH("bus-integrations", '<h3 class="dh">总线集成形态</h3><p class="dsub">同一套处理器模型，三种宿主形态；Socket 形态用于跨进程/跨机。</p>' +
 '<div class="g2"><div class="card"><h4>Prism</h4><pre class="code">containerRegistry.RegisterEventBus(assemblies);' + NL + '// 或在 PrismApplication 中' + NL + 'Container.UseEventBus();</pre>' +
 '<div class="hint">适合 Prism.DryIoc.Avalonia 桌面应用。</div></div>' +
 '<div class="card"><h4>ASP.NET Core</h4><pre class="code">builder.Services.AddEventBus(assemblies);' + NL + 'app.UseEventBus();</pre>' +
 '<div class="hint">处理器自动发现，随请求管线启动。</div></div></div>' +
 '<div class="card"><h4>Socket 分布式总线 <code>EventServer</code> / <code>EventClient</code></h4>' +
 '<div class="g2"><div><div class="kv"><span class="k">监听地址</span><input class="tin" id="esHost" value="127.0.0.1" style="flex:1"></div>' +
 '<div class="kv"><span class="k">端口</span><input class="tin" id="esPort" value="39561" style="flex:1"></div>' +
 '<div class="kv"><span class="k">心跳间隔</span><input class="tin" id="esHb" value="10" style="flex:1"></div>' +
 '<div class="row1"><button class="b pri" onclick="esStart()">启动服务端</button><button class="b" onclick="esStop()">停止</button></div>' +
 '<div class="kv"><span class="k">状态</span><span id="esSt">● 已停止</span></div></div>' +
 '<div><div class="kv"><span class="k">主题</span><input class="tin" id="esTopic" value="demo.greeting" style="flex:1"></div>' +
 '<div class="kv"><span class="k">内容</span><input class="tin" id="esMsg" value="hello" style="flex:1"></div>' +
 '<div class="row1"><button class="b" onclick="esPub()">Publish</button><button class="b" onclick="esQuery()">Query&lt;Ping,Pong&gt;</button></div>' +
 '<div class="lgst" id="esFeed"></div></div></div>' +
 '<pre class="code">var server = new EventServer(); server.Start("127.0.0.1", 39561);' + NL + 'await client.ConnectAsync("127.0.0.1", 39561);' + NL + 'client.Subscribe&lt;string&gt;("demo.greeting", msg =&gt; Console.WriteLine(msg));' + NL + 'client.Publish("demo.greeting", "hello", out _);' + NL + 'var (result, error) = await client.QueryAsync&lt;Ping, Pong&gt;(ping);</pre>' +
 '<div class="hint">心跳保活、断线重连、离线消息补发；底层传输由 NetWrapper 承担。跨版本互联请保持两端包版本一致。</div></div>',
 [{ id: "bus.host", name: "宿主形态", kind: "sel", value: "Socket", options: ["进程内", "Prism", "ASP.NET Core", "Socket"] },
  { id: "bus.port", name: "端口", kind: "num", value: 39561 },
  { id: "bus.heartbeat", name: "心跳(秒)", kind: "num", value: 10 },
  { id: "bus.resend", name: "离线补发", kind: "sw", value: true }]);

/* ═══════════════════════════ 页面：文件格式读写 ═══════════════════════════ */
RH("files-json", '<h3 class="dh">文件读写：Json / Yaml / Csv / Excel</h3><p class="dsub">基于框架内置 System.Text.Json 与 YamlDotNet / CsvHelper / MiniExcel。下面是往返转换演示。</p>' +
 '<div class="g2"><div class="card"><h4>对象 → 文本</h4><div class="row1">' +
 '<button class="b pri" onclick="fmtRun(\'json\')">ToJson</button><button class="b" onclick="fmtRun(\'yaml\')">ToYaml</button>' +
 '<button class="b" onclick="fmtRun(\'csv\')">ToCsv</button></div><pre class="code" id="fmtOut">选择一种格式</pre></div>' +
 '<div class="card"><h4>文本 → 对象</h4><pre class="code" id="fmtBack">等待反向解析</pre>' +
 '<div class="hint">扩展方法带 out error 参数：<code>student.ToYaml(out var error)</code>，失败不外抛。</div></div></div>' +
 '<div class="card"><h4>数据模型</h4><pre class="code">public sealed class Student' + NL + '{' + NL + '    public string Name { get; set; } = "张三";' + NL + '    public int Age { get; set; } = 20;' + NL + '    public string City { get; set; } = "成都";' + NL + '}</pre></div>' +
 '<div class="card"><h4>其它能力</h4><table class="tb"><tbody>' +
 '<tr><td><code>DataTableExtensions</code></td><td>Csv / Excel 导出导入（CsvHelper / MiniExcel）</td></tr>' +
 '<tr><td><code>FileExtension</code></td><td>带重试的安全字符串/字节读写</td></tr>' +
 '<tr><td><code>AppConfigHelper</code></td><td>App.config appSettings 读写（见“压缩与配置”页）</td></tr>' +
 '</tbody></table></div>',
 [{ id: "files.format", name: "默认格式", kind: "sel", value: "Yaml", options: ["Json", "Yaml", "Csv", "Excel"] },
  { id: "files.retry", name: "写入重试", kind: "num", value: 3 },
  { id: "files.bom", name: "UTF-8 BOM", kind: "sw", value: false }]);

/* ═══════════════════════════ 页面：压缩与配置 ═══════════════════════════ */
RH("files-archive", '<h3 class="dh">7z 压缩与 App.config</h3><p class="dsub">SevenZipCompressor 基于 SharpCompress；AppConfigHelper 读写传统 appSettings。</p>' +
 '<div class="g2"><div class="card"><h4>7z 压缩</h4>' +
 '<div class="row1"><input class="tin" id="szSrc" value="docs/导出结果" style="flex:1"><button class="b" onclick="szAdd()">添加</button></div>' +
 '<div class="list" id="szList"><div class="it">design/原型.html</div><div class="it">docs/说明文档.md</div></div>' +
 '<div class="row1"><button class="b pri" onclick="szRun()">Compress → archive.7z</button><button class="b" onclick="szExtract()">Extract</button></div>' +
 '<pre class="code" id="szOut">等待操作</pre>' +
 '<div class="hint">分卷与加密请评估 SharpCompress 能力边界。</div></div>' +
 '<div class="card"><h4>App.config</h4><table class="tb" id="cfgTb"><thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>' +
 '<tr><td>App.Language</td><td>zh-CN</td></tr><tr><td>App.Theme</td><td>Default</td></tr><tr><td>Log.MinimumLevel</td><td>Information</td></tr>' +
 '</tbody></table><div class="row1"><input class="tin" id="cfgK" placeholder="Key" style="width:120px"><input class="tin" id="cfgV" placeholder="Value" style="flex:1">' +
 '<button class="b pri" onclick="cfgSet()">写入</button></div>' +
 '<pre class="code">AppConfigHelper.Set("App.Language", "zh-CN");' + NL + 'var lang = AppConfigHelper.TryGet&lt;string&gt;("App.Language", out var v);</pre></div></div>',
 [{ id: "sz.level", name: "压缩级别", kind: "num", value: 9 },
  { id: "sz.solid", name: "固实压缩", kind: "sw", value: true },
  { id: "sz.password", name: "加密", kind: "sw", value: false },
  { id: "sz.volume", name: "分卷大小(MB)", kind: "num", value: 0 }]);

/* ═══════════════════════════════════════════════════════════════════════════
   页面脚本（沿用原原型实现，补充场景页与状态联动）
   ═══════════════════════════════════════════════════════════════════════════ */
function TK_CORE_FN() {

/* ---- Core 扩展 + 更新检查 ---- */
window.coreRun = function (k) {
 var s = ($("coreIn") || {}).value || "";
 if (k === "truncate") { $("coreOut").textContent = "原文：" + s + NL + "截断 18：" + (s.length > 18 ? s.slice(0, 18) + "…" : s); }
 else if (k === "slug") { $("coreOut").textContent = "正则替换非字母数字：" + s.replace(/[^A-Za-z0-9]+/g, "_").toLowerCase(); }
 else { $("coreOut").textContent = "ByteUnit 格式化：" + [1024, 1048576, 3221225472].map(function (b, i) { return ["1.00 KB", "1.00 MB", "3.00 GB"][i] + "  (" + b + " B)"; }).join(NL); }
 log2both("Core 扩展演示：" + k);
};
window.upCheck = function () {
 var repo = ($("upRepo") || {}).value || "", cur = ($("upCur") || {}).value || "";
 var etag = ($("upEtag") || {}).checked, out = $("upOut");
 if (!out) return;
 out.textContent = "GET https://api.github.com/repos/" + repo + "/releases/latest" + (etag ? '  If-None-Match: W/"etag-cache"' : "") + NL + "→ 200 OK（命中缓存则为 304，不消耗配额）";
 setTimeout(function () {
  out.textContent += NL + NL + "UpdateCheckResult.Succeeded = true" + NL + "Update = { Version: 1.1.0, Tag: v1.1.0 }" + NL + "→ 对比当前 " + cur + "：发现新版本 v1.1.0";
  setSync(new Date().toLocaleTimeString());
  notify("warn", tt("nc.warn"), "发现新版本 v1.1.0");
  log2both("更新检查：发现新版本 v1.1.0", "warn");
 }, 260);
};

/* ---- 进程内总线 ---- */
window._busOn = true;
window.busPublish = function () {
 var m = ($("busMsg") || {}).value || "";
 log2both("Publish(GreetCommand{ Name = \"" + m + "\" })");
 if (window._busOn) {
  setTimeout(function () { var f = $("busFeed"); if (!f) return; f.innerHTML += "<div>[lambda] GreetCommand → " + esc(m) + "</div>"; f.scrollTop = f.scrollHeight; }, 90);
  setTimeout(function () { var f = $("busFeed"); if (!f) return; f.innerHTML += "<div class='d'>[EventHandler Order=1] → " + esc(m) + "</div>"; f.scrollTop = f.scrollHeight; }, 180);
 }
};
window.busQuery = function () {
 var f = $("busFeed");
 log2both("Query(new TimeQuery())");
 setTimeout(function () { if (!f) return; f.innerHTML += "<div class='w'>TimeQuery.Result = " + new Date().toLocaleTimeString() + "</div>"; f.scrollTop = f.scrollHeight; }, 140);
};
window.busClear = function () { var f = $("busFeed"); if (f) f.innerHTML = ""; log2both("总线会话已清空"); };
window.busToggle2 = function () {
 window._busOn = !window._busOn;
 var b = $("busT2Btn"), s = $("busSub2");
 if (b) b.textContent = window._busOn ? "取消第二条订阅" : "恢复第二条订阅";
 if (s) s.textContent = window._busOn ? "已订阅" : "已取消";
 log2both("第二条 GreetCommand 订阅：" + (window._busOn ? "启用" : "取消"));
};

/* ---- Socket 分布式总线（运行态工作台与集成形态页共用同一批入口） ---- */
window._esOn = false;
window.esStart = function () {
 window._esOn = true;
 var s = $("esSt"); if (s) s.textContent = "● 运行中";
 var c = $("esClients"); if (c) c.textContent = "1";
 log2both("EventServer.Start(" + (($("esHost") || {}).value || "127.0.0.1") + ":" + (($("esPort") || {}).value || 39561) + ")");
 var f = $("esFeed"); if (f) f.innerHTML += "<div class='d'>服务端已启动，等待客户端连接…</div>";
};
window.esStop = function () {
 window._esOn = false;
 var s = $("esSt"); if (s) s.textContent = "● 已停止";
 var c = $("esClients"); if (c) c.textContent = "0";
 log2both("EventServer 已停止", "warn");
};
window.esPub = function () {
 if (!window._esOn) { log2both("请先启动服务端", "error"); return; }
 var t = ($("esTopic") || {}).value || "demo.greeting", m = ($("esMsg") || {}).value || "hello", f = $("esFeed");
 log2both("Publish(" + t + ", \"" + m + "\")");
 if (f) f.innerHTML += "<div>[" + esc(t) + "] " + esc(m) + "</div>";
};
window.esQuery = function () {
 if (!window._esOn) { log2both("请先启动服务端", "error"); return; }
 var f = $("esFeed");
 log2both("QueryAsync<Ping, Pong>");
 setTimeout(function () { if (f) f.innerHTML += "<div class='w'>Pong ← roundtrip 3 ms</div>"; }, 160);
};
/* 运行态工作台入口 */
window.rtStart = function () { esStart(); guard(function () { rtStep = 1; renderInto("flow-runtime", $("seg-flow-runtime")); }, "rtStart"); };
window.rtConnect = function () {
 if (!window._esOn) { log2both("请先启动服务端", "error"); return; }
 var f = $("esFeed");
 log2both("await client.ConnectAsync(" + (($("esHost") || {}).value || "127.0.0.1") + ", " + (($("esPort") || {}).value || 39561) + ")");
 if (f) f.innerHTML += "<div>客户端 127.0.0.1:52104 已连接</div>";
 var c = $("esClients"); if (c) c.textContent = "1";
 rtStep = 2; guard(function () { renderInto("flow-runtime", $("seg-flow-runtime")); }, "rtConnect");
};
window.rtPublish = function () { esPub(); rtStep = 3; guard(function () { renderInto("flow-runtime", $("seg-flow-runtime")); }, "rtPublish"); };
window.rtQuery = function () { esQuery(); rtStep = 4; guard(function () { renderInto("flow-runtime", $("seg-flow-runtime")); }, "rtQuery"); };
window.rtLog = function (lv) { log2both("运行态日志：" + lv, lv); };
window.rtCheck = function () {
 var out = $("rtOut");
 if (out) out.textContent = "GET https://api.github.com/repos/dotnet9/CodeWF.Toolkit/releases/latest" + NL + "→ 304 Not Modified（ETag 命中，未消耗配额）";
 setSync(new Date().toLocaleTimeString());
 log2both("检查更新：已是最新（304 缓存命中）", "info");
};
window.rtStopAsk = function () {
 if (!window._esOn) { log2both("服务端未在运行", "warn"); return; }
 confirmDlg(tt("dlg.stop.t"), tt("dlg.stop.d"), function () {
  esStop(); rtStep = 0;
  guard(function () { renderInto("flow-runtime", $("seg-flow-runtime")); }, "rtStop");
 });
};

/* ---- 文件格式 / 归档 / 配置 ---- */
window.fmtRun = function (k) {
 var o = $("fmtOut"), b = $("fmtBack");
 if (!o) return;
 if (k === "json") {
  o.textContent = '{' + NL + '  "name": "张三",' + NL + '  "age": 20,' + NL + '  "city": "成都"' + NL + '}';
  if (b) b.textContent = "FromJson<Student> → name=张三, age=20, city=成都  (error = null)";
 } else if (k === "yaml") {
  o.textContent = "name: 张三" + NL + "age: 20" + NL + "city: 成都";
  if (b) b.textContent = "FromYaml<Student>(out var obj, out error) → obj.City = 成都  (error = null)";
 } else {
  o.textContent = "Name,Age,City" + NL + "张三,20,成都";
  if (b) b.textContent = "DataTableExtensions → DataTable(1 行 3 列) → 可再导出 xlsx";
 }
 log2both("文件格式演示：" + k);
};
window.szAdd = function () {
 var v = ($("szSrc") || {}).value, l = $("szList");
 if (!l || !v) return;
 l.innerHTML += "<div class='it'>" + esc(v) + "</div>";
 log2both("加入压缩项：" + v);
};
window.szRun = function () {
 var l = $("szList"), n = l ? l.children.length : 0, o = $("szOut");
 if (o) o.textContent = "SevenZipCompressor.Compress(" + n + " 项) → archive.7z" + NL + "压缩级别 9 · 固实压缩：开启" + NL + "完成：archive.7z (1.2 MB)";
 log2both("7z 压缩完成：" + n + " 项");
};
window.szExtract = function () {
 var o = $("szOut");
 if (o) o.textContent = "SevenZipCompressor.Extract(archive.7z) → 还原 " + (((($("szList") || {}).children || []).length) || 0) + " 项";
 log2both("7z 解压完成");
};
window.cfgSet = function () {
 var k = ($("cfgK") || {}).value, v = ($("cfgV") || {}).value;
 if (!k) return;
 var tb = $("cfgTb");
 if (tb) { var tr = document.createElement("tr"); tr.innerHTML = "<td>" + esc(k) + "</td><td>" + esc(v) + "</td>"; tb.tBodies[0].appendChild(tr); }
 $("cfgK").value = ""; $("cfgV").value = "";
 log2both("AppConfigHelper.Set(" + k + ", " + v + ")");
};

/* ---- 能力实验室流水线 ---- */
function labDefs() {
 return [
  { n: "文件格式转换", call: "ToJson / ToYaml / ToCsv", run: function () { fmtRun("yaml"); return "student.yaml (3 字段)"; } },
  { n: "归档与配置", call: "SevenZipCompressor.Compress", run: function () { szRun(); return "archive.7z (1.2 MB)"; } },
  { n: "图标与二维码", call: "MergeGenerateIcon + QrCodeGenerator", run: function () { if (window.icMerge) icMerge(); if (window.qrMake) qrMake(); return "logo.ico + qrcode.png"; } },
  { n: "长任务与导出", call: "DataTableExtensions 导出 xlsx", run: function () { labExport(); return "students.xlsx (86 KB)"; } }
 ];
}
function labExport() {
 var bar = $("labBar");
 if (!bar) return;
 bar.style.display = "block";
 bar.querySelector("i").style.width = "0%";
 var i = 0, t = setInterval(function () {
  i += 20;
  bar.querySelector("i").style.width = Math.min(i, 100) + "%";
  if (i >= 100) clearInterval(t);
 }, 90);
}
function labDraw() {
 var tb = $("labTb"); if (!tb) return;
 var st = LAB_ROWS;
 tb.innerHTML = labDefs().map(function (d, i) {
  var s = st[i] || "待运行";
  return "<tr><td>" + (i + 1) + "</td><td>" + esc(d.n) + "</td><td><code>" + esc(d.call) + "</code></td><td>" +
   esc(s === "待运行" ? "—" : s) + "</td><td>" + (s === "待运行" ? "待运行" : "✅ 完成") + "</td></tr>";
 }).join("");
}
function labRunIdx(idx) {
 var d = labDefs()[idx];
 if (!d) return;
 var out = d.run();
 LAB_ROWS[idx] = out;
 labDraw();
 labStep = idx;
 log2both("流水线步骤 " + (idx + 1) + " 完成：" + d.n + " → " + out, "info");
}
window.labRunStep = function () { labRunIdx(labStep); guard(function () { renderInto("flow-lab", $("seg-flow-lab")); }, "labStep"); };
window.labRunAll = function () {
 LAB_ROWS = {};
 var bar = $("labBar"), msg = $("labMsg"), btn = $("labRunBtn");
 if (bar) { bar.style.display = "block"; bar.querySelector("i").style.width = "0%"; }
 if (msg) msg.textContent = tt("busy.export");
 if (btn) btn.disabled = true;
 var i = 0;
 var t = setInterval(function () {
  labRunIdx(i);
  i++;
  if (bar) bar.querySelector("i").style.width = Math.min(i / 4 * 100, 100) + "%";
  if (i >= 4) {
   clearInterval(t);
   if (msg) msg.textContent = tt("busy.done") + "：4 步全部完成";
   if (btn) btn.disabled = false;
   notify("ok", tt("nc.ok"), "能力实验室：4 步流水线完成");
   log2both("流水线全部完成", "info");
   guard(function () { renderInto("flow-lab", $("seg-flow-lab")); }, "labAll");
  }
 }, 480);
};
window.labReset = function () {
 LAB_ROWS = {};
 var bar = $("labBar"); if (bar) bar.style.display = "none";
 var msg = $("labMsg"); if (msg) msg.textContent = "每一步都调用下方子标签里的真实脚本（ToJson/ToYaml、7z 压缩、Icon 合成、二维码生成）。";
 log2both("流水线结果已重置", "warn");
 guard(function () { renderInto("flow-lab", $("seg-flow-lab")); }, "labReset");
};
window.labDraw = labDraw;
}
TK_CORE_FN();
