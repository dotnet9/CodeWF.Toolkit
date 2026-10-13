/* ═══════════════════════════════════════════════════════════════════════════
   CodeWF.Toolkit.Demo 原型 · 页面：图像 / 日志 / 网络（能力字典）
   本文件末尾负责启动演示台（TK_BOOT）。
   ═══════════════════════════════════════════════════════════════════════════ */
"use strict";

/* ═══════════════════════════ 页面：图标生成 ═══════════════════════════ */
REG("image-icon", {
 zh: {
  html: '<h3 class="dh">图标生成：多尺寸 ico 合成与拆分</h3><p class="dsub">ImageHelper.MergeGenerateIcon / SeparateGenerateIcon，基于 Magick.NET-Q16-AnyCPU。</p>' +
   '<div class="g2"><div class="card"><h4>合成</h4>' +
   '<div class="row1"><input class="tin" id="icSrc" value="logo.png" style="flex:1"><span>→</span><input class="tin" id="icDst" value="logo.ico" style="flex:1"></div>' +
   '<div class="row1" id="icSizes"></div>' +
   '<div class="row1"><button class="b pri" onclick="icMerge()">MergeGenerateIcon</button></div>' +
   '<div class="card" style="margin:0"><h4 style="margin-bottom:4px">预览</h4><div id="icPrev" style="display:flex;gap:8px;align-items:flex-end;height:70px"></div></div></div>' +
   '<div class="card"><h4>拆分</h4>' +
   '<div class="row1"><input class="tin" id="icSrc2" value="logo.ico" style="flex:1"><input class="tin" id="icDir" value="out/icons" style="flex:1"></div>' +
   '<div class="row1"><button class="b" onclick="icSplit()">SeparateGenerateIcon</button></div>' +
   '<pre class="code" id="icOut">等待操作</pre>' +
   '<pre class="code">await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [16,24,32,48,64,128,256]);' + NL + 'await ImageHelper.SeparateGenerateIcon("logo.ico", "out/icons", [24, 32]);</pre></div></div>' +
   '<div class="card"><h4>其它图像能力</h4><table class="tb"><tbody>' +
   '<tr><td>格式转换</td><td>png / jpg / webp / bmp 互转</td></tr><tr><td>缩放</td><td>等比、指定宽高、质量压制</td></tr>' +
   '<tr><td>水印</td><td>文字/图片水印，九宫格定位与透明度</td></tr></tbody></table></div>'
 },
 props: [{ id: "ic.sizes", name: "包含尺寸", kind: "ro", value: "16,24,32,48,64,128,256" },
  { id: "ic.compress", name: "PNG 压缩", kind: "sw", value: true },
  { id: "ic.quality", name: "质量", kind: "num", value: 92 }],
 after: function () { if (window.icMerge) icMerge(); }
});
window.icMerge = function () {
 var sizes = [16, 24, 32, 48, 64, 128, 256], p = $("icPrev");
 if (p) {
  p.innerHTML = sizes.map(function (s) {
   var px = Math.max(14, Math.min(s, 64));
   return "<div style='width:" + px + "px;height:" + px + "px;border-radius:6px;background:linear-gradient(135deg,var(--pri),var(--pri-h));display:flex;align-items:center;justify-content:center;color:#fff;font-size:" + Math.max(9, px / 3) + "px;font-weight:700'>" + s + "</div>";
  }).join("");
 }
 var o = $("icOut");
 if (o) o.textContent = "写入 " + sizes.length + " 个尺寸 → " + (($("icDst") || {}).value || "logo.ico") + " (" + sizes.join(", ") + ")";
 log2both("MergeGenerateIcon 完成，" + sizes.length + " 个尺寸");
};
window.icSplit = function () {
 var o = $("icOut");
 if (o) o.textContent = "SeparateGenerateIcon(logo.ico) → 输出到 " + (($("icDir") || {}).value || "out/icons") + NL + "24.png  32.png";
 log2both("SeparateGenerateIcon 完成");
};

/* ═══════════════════════════ 页面：二维码与图片 ═══════════════════════════ */
REG("image-qr", {
 zh: {
  html: '<h3 class="dh">二维码与图片处理</h3><p class="dsub">QrCodeGenerator（ZXing.Net.Bindings.Magick）输出 MagickImage，可与 ImageHelper 无缝衔接。</p>' +
   '<div class="g2"><div class="card"><h4>生成二维码</h4>' +
   '<div class="row1"><input class="tin" id="qrTxt" style="flex:1" value="https://github.com/dotnet9/CodeWF.Toolkit"></div>' +
   '<div class="row1"><span>尺寸</span><input class="tin" id="qrSize" type="number" value="180" style="width:80px"><button class="b pri" onclick="qrMake()">生成</button></div>' +
   '<div style="display:flex;justify-content:center;padding:6px 0"><canvas id="qrCanvas" width="180" height="180" style="background:#fff;border:1px solid var(--bd);border-radius:6px"></canvas></div>' +
   '<div class="hint">原型用确定性图案示意；实现里由 ZXing 真实生成。真实签名为 QrCodeGenerator.GenerateQrCode(title, content, imagePath, subTitle)。</div></div>' +
   '<div class="card"><h4>处理管线</h4>' +
   '<div class="row1"><span>水印</span><input class="tin" id="wmTxt" value="CodeWF" style="flex:1"></div>' +
   '<div class="row1"><span>透明度</span><input class="tin" id="wmAlpha" type="number" value="35" style="width:70px">' +
   '<select class="tin" id="wmPos"><option>右下</option><option>居中</option><option>左下</option><option>平铺</option></select></div>' +
   '<div class="row1"><button class="b pri" onclick="wmRun()">应用水印</button><button class="b" onclick="convRun()">转 WebP</button></div>' +
   '<pre class="code" id="wmOut">等待操作</pre></div></div>' +
   '<div class="card"><h4>常用调用</h4><pre class="code">QrCodeGenerator.GenerateQrCode("CodeWF", "https://github.com/dotnet9/CodeWF.Toolkit", "qrcode.png");' + NL + NL + 'await ImageHelper.MergeGenerateIcon("logo.png", "logo.ico", [24, 32, 48]);</pre></div>'
 },
 props: [{ id: "qr.size", name: "二维码尺寸", kind: "num", value: 180 },
  { id: "qr.ecc", name: "纠错级别", kind: "sel", value: "M", options: ["L", "M", "Q", "H"] },
  { id: "wm.alpha", name: "水印透明度", kind: "num", value: 35 }],
 after: function () { if (window.qrMake) qrMake(); }
});
window.qrMake = function () {
 var c = $("qrCanvas"); if (!c) return;
 var s = parseInt((($("qrSize") || {}).value) || "180", 10);
 c.width = s; c.height = s;
 var g = c.getContext("2d");
 g.fillStyle = "#fff"; g.fillRect(0, 0, s, s);
 g.fillStyle = "#000";
 var n = 21, cell = s / n, txt = (($("qrTxt") || {}).value) || "";
 var h = 0;
 for (var i = 0; i < txt.length; i++) h = (h * 31 + txt.charCodeAt(i)) >>> 0;
 for (var y = 0; y < n; y++) {
  for (var x = 0; x < n; x++) {
   var v = ((h >> ((x * 3 + y * 5) % 28)) ^ (x * y)) & 1;
   var finder = (x < 7 && y < 7) || (x >= n - 7 && y < 7) || (x < 7 && y >= n - 7);
   var on = finder ? ((x % 6 === 0 || y % 6 === 0) || (x > 1 && x < 5 && y > 1 && y < 5)) : v;
   if (on) g.fillRect(Math.round(x * cell), Math.round(y * cell), Math.ceil(cell), Math.ceil(cell));
  }
 }
 log2both("QrCodeGenerator.GenerateQrCode(" + s + "px)");
};
window.wmRun = function () {
 var o = $("wmOut");
 if (o) o.textContent = "水印 \"" + (($("wmTxt") || {}).value) + "\" · 透明度 " + (($("wmAlpha") || {}).value) + "% · 位置 " + ($("wmPos") || {}).value + NL + "ImageHelper.ApplyWatermark → output.png";
 log2both("应用水印完成");
};
window.convRun = function () {
 var o = $("wmOut");
 if (o) o.textContent = "ImageHelper.Convert(→ webp, quality 92) → output.webp";
 log2both("格式转换完成：webp");
};

/* ═══════════════════════════ 页面：Logger 核心 ═══════════════════════════ */
REG("log-core", {
 zh: {
  html: '<h3 class="dh">Logger 核心</h3><p class="dsub">静态 Logger、控制台 + 文件轮转、UI 事件流（LogEventFeed）。下方是实时日志流与级别过滤。</p>' +
   '<div class="g2"><div class="card"><h4>日志流</h4>' +
   '<div class="row1"><button class="b" onclick="lgWrite(\'debug\')">Debug</button><button class="b" onclick="lgWrite(\'info\')">Info</button>' +
   '<button class="b" onclick="lgWrite(\'warn\')">Warn</button><button class="b" onclick="lgWrite(\'error\')">Error</button>' +
   '<button class="b" onclick="lgBurst()">批量 ×10</button></div>' +
   '<div class="lgst" id="lgStream"></div>' +
   '<div class="row1"><label><input type="checkbox" id="lgA" checked onchange="lgFilter()"> Debug</label><label><input type="checkbox" id="lgB" checked onchange="lgFilter()"> Info</label>' +
   '<label><input type="checkbox" id="lgC" checked onchange="lgFilter()"> Warn</label><label><input type="checkbox" id="lgD" checked onchange="lgFilter()"> Error</label>' +
   '<button class="b" onclick="lgClear()">清空</button></div>' +
   '<div class="hint" id="lgCount"></div></div>' +
   '<div class="card"><h4>初始化</h4><pre class="code">Logger.TryInitialize(new LoggerOptions' + NL + '{' + NL + '    MinimumLevel = LogLevel.Information,' + NL + '    EnableConsole = true,' + NL + '    File = new FileLogOptions' + NL + '    {' + NL + '        DirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs"),' + NL + '        MaxFileSizeBytes = 32 * 1024 * 1024,' + NL + '        RetentionDays = 30,' + NL + '    },' + NL + '});' + NL + NL + 'Logger.Info("服务已启动");' + NL + 'Logger.Error("处理失败", ex);' + NL + 'await Logger.ShutdownAsync();   // 退出前刷盘</pre>' +
   '<div class="hint">文件写入为批量异步，崩溃前请 ShutdownAsync；UI 事件流回调在后台线程，订阅方自行调度到 UI 线程。</div></div></div>' +
   '<div class="card"><h4>选项一览</h4><table class="tb"><tbody>' +
   '<tr><td><code>MinimumLevel</code></td><td>最低级别</td></tr><tr><td><code>EnableConsole</code></td><td>控制台输出</td></tr>' +
   '<tr><td><code>File</code></td><td>FileLogOptions：目录/单文件上限/保留天数/批量写入</td></tr>' +
   '<tr><td><code>LineTemplate</code></td><td>LineTemplateController 自定义输出模板</td></tr>' +
   '<tr><td><code>LogEventFeed</code></td><td>UI 日志事件快照，配合 CodeWF.Avalonia.Log 使用</td></tr>' +
   '<tr><td><code>LoggerSelfDiagnostics</code></td><td>日志系统自身诊断</td></tr></tbody></table></div>'
 },
 props: [{ id: "lg.level", name: "最低级别", kind: "sel", value: "Information", options: ["Debug", "Information", "Warning", "Error"] },
  { id: "lg.console", name: "控制台输出", kind: "sw", value: true },
  { id: "lg.rotate", name: "文件轮转", kind: "sw", value: true },
  { id: "lg.maxmb", name: "单文件上限(MB)", kind: "num", value: 32 },
  { id: "lg.days", name: "保留天数", kind: "num", value: 30 }],
 after: function () {
  var s = $("lgStream");
  if (s && !s.children.length) s.innerHTML = "<div class='d'>LogEventFeed 已订阅（快照 0 条）</div>";
  lgFilter();
 }
});
window._lg = { debug: true, info: true, warn: true, error: true };
window.lgWrite = function (k) {
 var msgs = { debug: "调试：加载配置", info: "服务已启动", warn: "内存水位偏高", error: "连接已超时" };
 var lv = k === "debug" ? "d" : k === "warn" ? "w" : k === "error" ? "e" : "";
 log2both(msgs[k], k === "debug" ? null : k === "warn" ? "warn" : k === "error" ? "error" : "info");
 var s = $("lgStream");
 if (s) {
  var e = document.createElement("div");
  e.className = lv;
  e.dataset.lv = k;
  e.innerHTML = "<span class='d'>" + new Date().toLocaleTimeString() + "</span> [" + k.toUpperCase() + "] " + esc(msgs[k]);
  s.appendChild(e); s.scrollTop = s.scrollHeight;
 }
 lgFilter();
};
window.lgBurst = function () {
 var i = 0;
 var t = setInterval(function () {
  lgWrite(i % 3 === 0 ? "warn" : "info");
  if (++i >= 10) clearInterval(t);
 }, 80);
};
window.lgClear = function () { var s = $("lgStream"); if (s) s.innerHTML = ""; dlog("日志流已清空"); lgFilter(); };
window.lgFilter = function () {
 var n = 0;
 document.querySelectorAll("#lgStream [data-lv]").forEach(function (e) {
  var on = $("lg" + e.dataset.lv[0].toUpperCase()) ? $("lg" + e.dataset.lv[0].toUpperCase()).checked : true;
  e.style.display = on ? "" : "none";
  if (on) n++;
 });
 var c = $("lgCount");
 if (c) c.textContent = "LogEventFeed：可见 " + n + " 条（级别过滤在控件侧完成）";
};

/* ═══════════════════════════ 页面：MEL 接入 ═══════════════════════════ */
REG("log-microsoft", {
 zh: {
  html: '<h3 class="dh">Microsoft.Extensions.Logging 提供程序</h3><p class="dsub">把 MEL 生态日志统一接入核心 Logger（文件轮转 + UI 事件流 + 结构化模板）。</p>' +
   '<div class="card"><h4>注册</h4><pre class="code">builder.Logging.AddCodeWFLogger(options =&gt;' + NL + '{' + NL + '    MinimumLevel = LogLevel.Information,' + NL + '    File = new FileLogOptions { RetentionDays = 7 },' + NL + '});</pre></div>' +
   '<div class="g2"><div class="card"><h4>结构化模板解析</h4>' +
   '<div class="row1"><input class="tin" id="mtTpl" style="flex:1" value="用户 {User} 在 {Time} 登录 {Ip}"></div>' +
   '<div class="row1"><input class="tin" id="mtVals" style="flex:1" value="admin,10:24,192.168.1.8"><button class="b pri" onclick="mtRun()">解析</button></div>' +
   '<pre class="code" id="mtOut">等待解析</pre>' +
   '<div class="hint">MessageTemplateParser 解析结构化模板；CodeWFUserLogState 支持按用户标记。</div></div>' +
   '<div class="card"><h4>选项</h4><table class="tb"><tbody>' +
   '<tr><td><code>CodeWFLoggerOptions</code></td><td>统一选项，支持配置绑定</td></tr>' +
   '<tr><td><code>CodeWFConsoleLoggerOptions</code></td><td>控制台输出</td></tr>' +
   '<tr><td><code>CodeWFFileLoggerOptions</code></td><td>文件轮转</td></tr>' +
   '<tr><td><code>CodeWFEventFeedOptions</code></td><td>UI 事件流</td></tr>' +
   '<tr><td><code>CodeWFCaptureOptions</code> / <code>CodeWFQueueOptions</code></td><td>捕获范围与队列策略</td></tr>' +
   '</tbody></table><div class="row1"><button class="b" onclick="mtDemo()">模拟一条 MEL 日志</button></div></div></div>'
 },
 props: [{ id: "mel.level", name: "最低级别", kind: "sel", value: "Information", options: ["Trace", "Debug", "Information", "Warning", "Error"] },
  { id: "mel.scope", name: "包含作用域", kind: "sw", value: true },
  { id: "mel.user", name: "按用户标记", kind: "sw", value: false },
  { id: "mel.queue", name: "队列满策略", kind: "sel", value: "DropOldest", options: ["DropOldest", "DropNewest", "Block"] }]
});
window.mtRun = function () {
 var t = (($("mtTpl") || {}).value) || "", v = ((($("mtVals") || {}).value) || "").split(",");
 var keys = [];
 t.replace(/\{([^}]+)\}/g, function (_, k) { keys.push(k); return _; });
 var o = $("mtOut");
 if (!o) return;
 o.textContent = "模板键：" + keys.join(", ") + NL + "解析结果：" +
  t.replace(/\{([^}]+)\}/g, function (_, k) { var i = keys.indexOf(k); return "<" + k + ":" + (v[i] || "?").trim() + ">"; });
 log2both("MessageTemplateParser 解析 " + keys.length + " 个占位符");
};
window.mtDemo = function () {
 log2both("MEL: 用户 admin 在 10:24 登录 192.168.1.8", "info");
 var s = $("lgStream");
 if (s) {
  var e = document.createElement("div");
  e.dataset.lv = "info";
  e.innerHTML = "<span class='d'>" + new Date().toLocaleTimeString() + "</span> [MEL/Information] 用户 admin 在 10:24 登录 192.168.1.8     (User=admin, Ip=192.168.1.8)";
  s.appendChild(e); s.scrollTop = s.scrollHeight;
 }
 lgFilter();
};

/* ═══════════════════════════ 页面：日志选项与诊断（新） ═══════════════════════════ */
REG("logging-options", {
 zh: {
  t: "日志选项与诊断",
  d: "把 LoggerOptions / FileLogOptions 的每个参数摊开，并演示 LoggerSelfDiagnostics 与 LogEventFeed 快照；与 CodeWF.Avalonia.Log 的对接点也在这里。",
  f: ["选项全览", "自诊断", "事件流快照"]
 },
 props: [
  { id: "lo.template", name: "行模板", kind: "sel", value: "DefaultTemplate", options: ["DefaultTemplate", "Compact", "Json", "自定义"] },
  { id: "lo.localized", name: "本地化级别", kind: "sw", value: true },
  { id: "lo.feed", name: "UI 事件流", kind: "sw", value: true },
  { id: "lo.batch", name: "批量写入", kind: "sw", value: true }
 ],
 render: function () {
  return '<div class="card"><h4>参数全览</h4><table class="tb"><thead><tr><th>参数</th><th>默认</th><th>说明</th></tr></thead><tbody>' +
   '<tr><td><code>MinimumLevel</code></td><td>Information</td><td>低于该级别的日志直接丢弃</td></tr>' +
   '<tr><td><code>EnableConsole</code></td><td>true</td><td>控制台输出（桌面应用可关）</td></tr>' +
   '<tr><td><code>File.DirectoryPath</code></td><td>AppContext.BaseDirectory/logs</td><td>日志目录</td></tr>' +
   '<tr><td><code>File.MaxFileSizeBytes</code></td><td>32 MB</td><td>单文件上限，超限轮转</td></tr>' +
   '<tr><td><code>File.RetentionDays</code></td><td>30</td><td>保留天数</td></tr>' +
   '<tr><td><code>LineTemplate</code></td><td>DefaultTemplate</td><td>LineTemplateController 可自定义</td></tr>' +
   '<tr><td><code>LocalizedLevelFormatter</code></td><td>zh / en</td><td>级别文案本地化</td></tr>' +
   '</tbody></table></div>' +
   '<div class="card"><h4>自诊断与事件流</h4><div class="row1">' +
   '<button class="b pri" onclick="loDiag()">LoggerSelfDiagnostics</button>' +
   '<button class="b" onclick="loFeed()">LogEventFeed 快照</button>' +
   '<button class="b" onclick="loFlush()">ShutdownAsync 刷盘</button>' +
   '<button class="b" onclick="log2both(\'打开日志目录（跨平台内联）\',\'warn\')">打开日志目录</button></div>' +
   '<pre class="code" id="loOut">等待操作</pre>' +
   '<div class="hint">CodeWF.Avalonia.Log 的 LogView 订阅的就是 LogEventFeed；回调在后台线程，界面侧需自行调度到 UI 线程。</div></div>' +
   '<div class="card"><h4>常见取值</h4><pre class="code">Logger.TryInitialize(new LoggerOptions' + NL + '{' + NL + '    MinimumLevel = LogLevel.Debug,' + NL + '    EnableConsole = false,' + NL + '    File = new FileLogOptions { RetentionDays = 7, MaxFileSizeBytes = 8 * 1024 * 1024 },' + NL + '    LineTemplate = LineTemplateController.DefaultTemplate,' + NL + '});</pre></div>';
 }
});
window.loDiag = function () {
 var o = $("loOut");
 if (o) o.textContent = "LoggerSelfDiagnostics：" + NL + "  writer      = 批量写入线程运行中" + NL + "  queueDepth  = 0" + NL + "  dropped     = 0" + NL + "  lastFlush   = " + new Date().toLocaleTimeString();
 log2both("LoggerSelfDiagnostics 输出完成", "info");
};
window.loFeed = function () {
 var o = $("loOut");
 if (o) o.textContent = "LogEventFeed.Snapshot() → 3 条" + NL + "  [Information] 服务已启动" + NL + "  [Warning] 内存水位偏高" + NL + "  [Error] 连接已超时";
 log2both("LogEventFeed 快照：3 条", "info");
};
window.loFlush = function () {
 var o = $("loOut");
 if (o) o.textContent = "await Logger.ShutdownAsync() → 队列已刷盘，文件句柄已释放";
 log2both("日志队列已刷盘", "info");
};

/* ═══════════════════════════ 页面：NetWeaver 序列化 ═══════════════════════════ */
REG("net-packet", {
 zh: {
  html: '<h3 class="dh">NetWeaver：特性驱动的二进制序列化</h3><p class="dsub">属性即协议：<code>[NetHead(systemId, version)]</code> 声明包头，<code>[NetFieldOrder(n)]</code> 声明字段顺序。</p>' +
   '<div class="g2"><div class="card"><h4>字段表</h4><table class="tb"><thead><tr><th>#</th><th>属性</th><th>类型</th><th></th></tr></thead><tbody id="pktTb"></tbody></table>' +
   '<div class="row1"><input class="tin" id="pktNew" placeholder="新属性名" style="flex:1"><button class="b" onclick="pktAdd()">添加字段</button></div>' +
   '<div class="row1"><button class="b pri" onclick="pktSer()">Serialize</button><button class="b" onclick="pktDes()">Deserialize</button></div></div>' +
   '<div class="card"><h4>包头</h4>' +
   '<div class="kv"><span class="k">systemId</span><input class="tin" id="pktSys" value="1" style="flex:1"></div>' +
   '<div class="kv"><span class="k">version</span><input class="tin" id="pktVer" value="1" style="flex:1"></div>' +
   '<div class="kv"><span class="k">字节序</span><span>大端（Big-Endian）</span></div>' +
   '<pre class="code" id="pktOut">等待序列化</pre></div></div>' +
   '<div class="card"><h4>协议声明</h4><pre class="code">[NetHead(0x01, 1)]' + NL + 'public sealed class SamplePacket : INetObject' + NL + '{' + NL + '    [NetFieldOrder(0)] public int Id { get; set; }' + NL + '    [NetFieldOrder(1)] public float Score { get; set; }' + NL + '    [NetFieldOrder(2)] public string Title { get; set; } = string.Empty;' + NL + '}' + NL + NL + 'var buffer = packet.Serialize(systemId: 0);' + NL + 'var restored = buffer.Deserialize&lt;SamplePacket&gt;();</pre>' +
   '<div class="hint"><code>[NetIgnoreMember]</code> 跳过成员；位域场景用 <code>NetFieldOffsetAttribute</code> 精确布局；裁剪/AOT 下通过 DynamicDependency 保留。</div></div>'
 },
 props: [{ id: "pkt.systemid", name: "systemId", kind: "num", value: 1 },
  { id: "pkt.version", name: "version", kind: "num", value: 1 },
  { id: "pkt.bigendian", name: "大端序", kind: "sw", value: true },
  { id: "pkt.offset", name: "位域布局", kind: "sw", value: false }],
 after: function () { if (window.pktDraw) pktDraw(); }
});
window._pkt = [{ n: "Id", t: "int" }, { n: "Score", t: "float" }, { n: "Title", t: "string" }];
window.pktDraw = function () {
 var tb = $("pktTb"); if (!tb) return;
 tb.innerHTML = window._pkt.map(function (f, i) {
  return "<tr><td>" + i + "</td><td>" + esc(f.n) + "</td><td>" + f.t + "</td><td><button class='b' style='height:20px;padding:0 6px' onclick='pktDel(" + i + ")'>×</button></td></tr>";
 }).join("");
};
window.pktAdd = function () {
 var v = ($("pktNew") || {}).value;
 if (!v) return;
 window._pkt.push({ n: v, t: "string" });
 $("pktNew").value = "";
 pktDraw();
 log2both("[NetFieldOrder(" + (window._pkt.length - 1) + ")] 新增字段 " + v);
};
window.pktDel = function (i) {
 var f = window._pkt[i];
 if (!f) return;
 log2both("移除字段 " + f.n, "warn");
 window._pkt.splice(i, 1);
 pktDraw();
};
window.pktSer = function () {
 var sys = ($("pktSys") || {}).value || "1", ver = ($("pktVer") || {}).value || "1";
 var bytes = ["0x01", "0x" + ("00" + sys).slice(-2), "0x" + ("00" + ver).slice(-2)];
 window._pkt.forEach(function (f) {
  if (f.t === "int") bytes.push("0x2A", "0x00", "0x00", "0x00");
  else if (f.t === "float") bytes.push("0x00", "0x00", "0x80", "0x3F");
  else bytes.push("0x05", "0x68", "0x65", "0x6C", "0x6C", "0x6F");
 });
 var o = $("pktOut");
 if (o) o.textContent = "包头 [NetHead(0x01, " + ver + ")]  systemId=" + sys + NL + "长度 " + bytes.length + " 字节" + NL + bytes.join(" ");
 log2both("Serialize(systemId=" + sys + ") → " + bytes.length + " 字节");
};
window.pktDes = function () {
 var o = $("pktOut");
 if (!o) return;
 o.textContent = "Deserialize<SamplePacket>() →" + NL + window._pkt.map(function (f) {
  return "  " + f.n + " = " + (f.t === "int" ? "42" : f.t === "float" ? "1.0" : "\"hello\"");
 }).join(NL);
 log2both("Deserialize 成功，字段数与声明一致");
};

/* ═══════════════════════════ 页面：NetWrapper 通信 ═══════════════════════════ */
REG("net-socket", {
 zh: {
  html: '<h3 class="dh">NetWrapper：TCP/UDP 通信与远程文件</h3><p class="dsub">TcpSocketServer / TcpSocketClient 封装、命令分发，内置远程文件系统与断点续传。</p>' +
   '<div class="g2"><div class="card"><h4>服务端</h4>' +
   '<div class="kv"><span class="k">监听</span><input class="tin" id="nsHost" value="127.0.0.1" style="flex:1"></div>' +
   '<div class="kv"><span class="k">端口</span><input class="tin" id="nsPort" value="39561" style="flex:1"></div>' +
   '<div class="row1"><button class="b pri" onclick="nsStart()">StartAsync</button><button class="b" onclick="nsStop()">Stop</button></div>' +
   '<div class="kv"><span class="k">状态</span><span id="nsSt">● 未启动</span></div>' +
   '<div class="kv"><span class="k">客户端数</span><span id="nsClients">0</span></div>' +
   '<div class="hint">客户端上线/断开、错误通过 SocketClientChangedCommand / TcpClientErrorCommand 经 EventBus 广播。</div></div>' +
   '<div class="card"><h4>客户端与消息</h4>' +
   '<div class="kv"><span class="k">命令</span><select class="tin" id="nsCmd"><option>Heartbeat</option><option>BrowseFileSystemRequest</option><option>FileDownloadRequest</option></select></div>' +
   '<div class="row1"><button class="b pri" onclick="nsSend()">SendCommandAsync</button><button class="b" onclick="nsUdp()">UDP 发送</button></div>' +
   '<div class="lgst" id="nsFeed"></div></div></div>' +
   '<div class="card"><h4>远程文件系统</h4><table class="tb"><thead><tr><th>文件</th><th>大小</th><th>进度</th></tr></thead><tbody id="nsFiles">' +
   '<tr><td>readme.md</td><td>12 KB</td><td>—</td></tr><tr><td>dataset.zip</td><td>86 MB</td><td>—</td></tr><tr><td>firmware.bin</td><td>4.2 MB</td><td>—</td></tr>' +
   '</tbody></table><div class="row1"><button class="b pri" onclick="nsTransfer()">下载 dataset.zip（分块 + 断点续传）</button><button class="b" onclick="nsResume()">断点续传</button></div>' +
   '<div class="hint">大端序分块 + 校验；FileChunkAck / FileTransferProgress 事件驱动进度。</div></div>'
 },
 props: [{ id: "ns.host", name: "主机", kind: "ro", value: "127.0.0.1" },
  { id: "ns.port", name: "端口", kind: "num", value: 39561 },
  { id: "ns.chunk", name: "分块大小(KB)", kind: "num", value: 64 },
  { id: "ns.resume", name: "断点续传", kind: "sw", value: true },
  { id: "ns.heartbeat", name: "心跳(秒)", kind: "num", value: 10 }]
});
window._nsOn = false;
window.nsStart = function () {
 window._nsOn = true;
 var s = $("nsSt"); if (s) s.textContent = "● 运行中";
 var c = $("nsClients"); if (c) c.textContent = "1";
 log2both("TcpSocketServer.StartAsync(" + (($("nsHost") || {}).value) + ":" + (($("nsPort") || {}).value) + ")");
 var f = $("nsFeed"); if (f) f.innerHTML += "<div class='d'>客户端 127.0.0.1:52104 已连接</div>";
};
window.nsStop = function () {
 window._nsOn = false;
 var s = $("nsSt"); if (s) s.textContent = "● 未启动";
 var c = $("nsClients"); if (c) c.textContent = "0";
 log2both("服务端已停止", "warn");
};
window.nsSend = function () {
 if (!window._nsOn) { log2both("请先启动服务端", "error"); return; }
 var cmd = ($("nsCmd") || {}).value, f = $("nsFeed");
 log2both("SendCommandAsync(new " + cmd + "())");
 if (f) f.innerHTML += "<div>→ " + esc(cmd) + " … 200 OK</div>";
};
window.nsUdp = function () {
 var f = $("nsFeed");
 log2both("UdpSocketClient.Send(discovery)");
 if (f) f.innerHTML += "<div class='w'>UDP 广播已发出</div>";
};
window.nsTransfer = function () {
 var rows = $("nsFiles").rows, i = 1, tick = 0;
 log2both("FileDownloadRequest(dataset.zip) 开始");
 var t = setInterval(function () {
  tick += 20;
  if (rows[i]) rows[i].cells[2].textContent = Math.min(tick, 100) + "%";
  if (tick >= 100) {
   clearInterval(t);
   notify("ok", tt("nc.ok"), "dataset.zip 传输完成，校验通过");
   log2both("FileTransferComplete(dataset.zip) 校验通过");
  }
 }, 220);
};
window.nsResume = function () {
 var rows = $("nsFiles").rows;
 if (rows[1]) rows[1].cells[2].textContent = "从 62% 续传…";
 log2both("断点续传：FileChunkAck 从最后确认块继续");
};

/* ═══ 启动演示台 ═══ */
if (typeof TK_BOOT === "function") TK_BOOT();
