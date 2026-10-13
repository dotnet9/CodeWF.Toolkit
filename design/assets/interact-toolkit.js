/* ═══════════════════════════════════════════════════════════════════════════
   CodeWF.Toolkit.Demo 原型 · 外壳与公共件
   信息架构：具体 API 优先（运行态工作台 / 能力实验室）+ 能力字典（3 个页签）
   外壳：Ribbon（7 页签 / 双击折叠）+ Dock 工作区 + 属性面板 + 日志面板 + 状态栏
   页面注册：assets/tk-pages-workbench.js（场景页）、assets/tk-pages-dict.js（能力字典）
   ═══════════════════════════════════════════════════════════════════════════ */
"use strict";

var $ = function (id) { return document.getElementById(id); };
var NL = String.fromCharCode(10);
function esc(s) { return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"); }
function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }
function guard(fn, tag) { try { return fn(); } catch (e) { dlog("JS 异常[" + tag + "]：" + e.message, "error"); } }

/* ═══ 双语文案表（zh-CN / en-US） ═══ */
var LANG = "zh-CN";
var I18N = {
 "zh-CN": {
  "tabs.home": "首页", "tabs.runtime": "运行态工作台", "tabs.lab": "能力实验室",
  "tabs.core": "能力字典·核心", "tabs.bus": "能力字典·通信", "tabs.files": "能力字典·数据",
  "tabs.logging": "能力字典·日志",
  "nav": "导航树", "props": "属性", "log": "日志", "ready": "就绪",
  "dock.nav": "导航树", "dock.props": "属性", "dock.log": "日志", "dock.max": "最大化", "dock.reset": "重置",
  "dock.hide": "隐藏", "qk.save": "保存",
  "log.opendir": "打开目录", "log.clear": "清空", "status.app": "演示台", "status.logpanel": "日志面板",
  "st.where": "未打开文档", "st.docs": " 篇文档", "st.level": "日志级别 ", "st.ready": "就绪", "st.sync": "最近检查 ",
  "tree.filter": "过滤页面…", "tree.empty": "无匹配页面", "tree.rename": "双击或点 ✎ 重命名", "tree.reset": "恢复原名",
  "nc.ok": "成功", "nc.warn": "警告", "nc.err": "错误", "nc.info": "提示",
  "cmd.ph": "搜索页面或命令（Esc 关闭，↑↓ 选择，Enter 执行）", "cmd.pages": "页面", "cmd.cmds": "命令",
  "dlg.cancel": "取消", "dlg.yes": "确定", "dlg.del.t": "确认执行", "dlg.del.d": "此操作不可撤销，确定继续吗？",
  "dlg.reset.t": "重置工作区确认", "dlg.reset.d": "将恢复面板显隐、色板、深浅与密度为默认值。",
  "dlg.stop.t": "停止服务确认", "dlg.stop.d": "正在运行的服务端会被关闭，进行中的传输会中断。",
  "dlg.leave.t": "有未保存的更改", "dlg.leave.d": "当前文档被修改过，关闭后将丢失改动。确定关闭吗？",
  "empty.title": "还没有打开文档", "empty.desc": "从顶部 Ribbon 页签或左侧导航树选择一个页面开始。", "empty.action": "打开欢迎页",
  "busy.export": "正在处理，可取消…", "busy.cancel": "取消", "busy.done": "完成",
  "err.title": "操作失败", "err.retry": "重试",
  "toast.saved": "布局已保存到浏览器本地", "toast.reset": "工作区已重置为默认值",
  "toast.density": "密度：", "toast.palette": "色板：", "toast.theme": "主题：", "toast.lang": "语言：",
  "toast.copied": "已复制到剪贴板", "toast.exported": "已完成",
  "density.cozy": "舒适", "density.compact": "紧凑", "theme.light": "浅色", "theme.dark": "深色",
  "g.prev": "上一步", "g.next": "下一步", "g.skip": "跳过", "g.close": "关闭",
  "g.1.t": "欢迎使用 Toolkit 演示台", "g.1.d": "Ribbon 按帮助类功能分组；菜单项对应具体 API。综合示例位于最后一组。",
  "g.2.t": "导航树", "g.2.d": "与 Ribbon 同源；顶部可过滤页面，双击条目可就地重命名。",
  "g.3.t": "文档区与空态", "g.3.d": "所有演示以文档页签承载：无文档时是空态，打开瞬间显示骨架屏。",
  "g.4.t": "属性面板", "g.4.d": "当前文档的可调项；被改动过的行左侧出现色条并加粗。",
  "g.5.t": "日志面板与状态栏", "g.5.d": "底部是 Toolkit 日志能力的自我演示；状态栏显示当前位置、文档数、日志级别与最近检查更新时间。",
  "ph.top": "本页概览", "ph.basic": "基础参数", "ph.adv": "进阶参数", "ph.state": "运行状态", "ph.doc": "当前文档"
 },
 "en-US": {
  "tabs.home": "Home", "tabs.runtime": "Runtime Console", "tabs.lab": "Lab",
  "tabs.core": "Lib · Core", "tabs.bus": "Lib · Comm", "tabs.files": "Lib · Data",
  "tabs.logging": "Lib · Logging",
  "nav": "Explorer", "props": "Properties", "log": "Log", "ready": "Ready",
  "dock.nav": "Explorer", "dock.props": "Properties", "dock.log": "Log", "dock.max": "Maximize", "dock.reset": "Reset",
  "dock.hide": "Hide", "qk.save": "Save",
  "log.opendir": "Open Dir", "log.clear": "Clear", "status.app": "Demo Shell", "status.logpanel": "Log Panel",
  "st.where": "No document", "st.docs": " docs", "st.level": "Log level ", "st.ready": "Ready", "st.sync": "Checked ",
  "tree.filter": "Filter pages…", "tree.empty": "No matching page", "tree.rename": "Double-click or ✎ to rename", "tree.reset": "Restore names",
  "nc.ok": "Success", "nc.warn": "Warning", "nc.err": "Error", "nc.info": "Info",
  "cmd.ph": "Search pages or commands (Esc close, arrows move, Enter run)", "cmd.pages": "Pages", "cmd.cmds": "Commands",
  "dlg.cancel": "Cancel", "dlg.yes": "OK", "dlg.del.t": "Confirm", "dlg.del.d": "This cannot be undone. Continue?",
  "dlg.reset.t": "Confirm workspace reset", "dlg.reset.d": "Panel visibility, palette, theme and density go back to defaults.",
  "dlg.stop.t": "Confirm stop", "dlg.stop.d": "Running servers will be stopped and transfers in flight will abort.",
  "dlg.leave.t": "Unsaved changes", "dlg.leave.d": "This document was edited. Closing discards the changes. Continue?",
  "empty.title": "No document open", "empty.desc": "Pick a page from the Ribbon tabs or the explorer on the left.", "empty.action": "Open welcome",
  "busy.export": "Working, cancellable…", "busy.cancel": "Cancel", "busy.done": "Done",
  "err.title": "Operation failed", "err.retry": "Retry",
  "toast.saved": "Layout saved to browser storage", "toast.reset": "Workspace reset to defaults",
  "toast.density": "Density: ", "toast.palette": "Palette: ", "toast.theme": "Theme: ", "toast.lang": "Language: ",
  "toast.copied": "Copied to clipboard", "toast.exported": "Done",
  "density.cozy": "Cozy", "density.compact": "Compact", "theme.light": "Light", "theme.dark": "Dark",
  "g.prev": "Back", "g.next": "Next", "g.skip": "Skip", "g.close": "Close",
  "g.1.t": "Welcome", "g.1.d": "Seven tabs: home + two scenario workflows (runtime console / lab) + three library sections. Double-click a tab to collapse the command area.",
  "g.2.t": "Explorer", "g.2.d": "Same source as the Ribbon. Filter on top; double-click an entry to rename it in place.",
  "g.3.t": "Documents & empty state", "g.3.d": "Every demo is a document tab: an empty state when none is open, a skeleton while the first one loads.",
  "g.4.t": "Properties", "g.4.d": "Editable options of the current document; changed rows get an accent bar and a bold label.",
  "g.5.t": "Log panel & status bar", "g.5.d": "Bottom is the Toolkit logging demo; the status bar shows location, document count, log level and last update check.",
  "ph.top": "Overview", "ph.basic": "Basic", "ph.adv": "Advanced", "ph.state": "State", "ph.doc": "Document"
 }
};
function tt(k) { return (I18N[LANG] && I18N[LANG][k]) || (I18N["zh-CN"] && I18N["zh-CN"][k]) || k; }

/* ═══ 日志 / 状态栏 / 通知中心 / 对话框 ═══ */
var LOG_LEVEL = "Info";
function dlog(msg, lv) {
 var b = $("logP"); if (!b) return;
 var d = new Date(), tm = ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2);
 var e = document.createElement("div");
 e.className = lv === "info" ? "ll1" : lv === "warn" ? "ll2" : lv === "error" ? "ll3" : "";
 e.innerHTML = '<span class="ltm">' + tm + "</span>" + msg;
 b.appendChild(e); b.scrollTop = b.scrollHeight;
 if (lv === "info" || lv === "warn" || lv === "error") LOG_LEVEL = lv === "info" ? "Info" : lv === "warn" ? "Warn" : "Error";
 var st = $("stLogLv"); if (st) st.textContent = LOG_LEVEL;
}
/* 状态栏：最近检查更新时间 / 最近同步时间 */
function setSync(text) { var el = $("stSync"); if (el) el.textContent = text; }
function stampNow() { var d = new Date(); return ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2); }
function setStatus(msg) { var el = $("stTx"); if (el) el.textContent = msg; setSync(stampNow()); }
function log2both(msg, lv) {
 dlog(msg, lv);
 var s = $("lgStream");
 if (s) {
  var e = document.createElement("div");
  e.className = lv === "warn" ? "w" : lv === "error" ? "e" : lv === "debug" ? "d" : "";
  e.innerHTML = "<span class='d'>" + new Date().toLocaleTimeString() + "</span> " + msg;
  s.appendChild(e); s.scrollTop = s.scrollHeight;
 }
 setStatus(msg);
}
function notify(kind, title, body) {
 var box = $("nc"); if (!box) return;
 var k = kind === "ok" ? "s" : kind === "warn" ? "w" : kind === "err" ? "e" : "i";
 var card = document.createElement("div");
 card.className = "ncard " + k;
 card.innerHTML = '<span>●</span><div style="flex:1"><div class="nt">' + esc(title) + '</div><div class="nb">' + esc(body || "") + "</div></div>" +
  '<button class="nx" title="' + esc(tt("g.close")) + '">✕</button>';
 card.querySelector(".nx").onclick = function () { if (card.parentNode === box) box.removeChild(card); };
 box.appendChild(card);
 while (box.children.length > 4) box.removeChild(box.children[0]);
 setTimeout(function () { if (card.parentNode === box) box.removeChild(card); }, 6000);
}
var _dlgYes = null;
function confirmDlg(title, body, onYes, danger) {
 var m = $("dlgm"); if (!m) { onYes(); return; }
 $("dlgT").textContent = title; $("dlgD").textContent = body;
 $("dlgYes").textContent = tt("dlg.yes"); $("dlgNo").textContent = tt("dlg.cancel");
 $("dlgYes").className = "btn " + (danger === false ? "pri" : "danger");
 _dlgYes = onYes; m.classList.add("on"); $("dlgYes").focus();
}
function closeDlg() { $("dlgm").classList.remove("on"); _dlgYes = null; }

/* ═══ 目录：7 页签 / 16 页（2 个场景页 + 14 个能力字典页） ═══ */
var CATALOG = window.CodeWFDemoCatalog.map(function (group) {
 return { id:group.id, icon:"◆", items:group.items.map(function (entry) { return {id:entry.prototypeId}; }) };
});
window.CodeWFDemoCatalog.forEach(function (group) {
 I18N["zh-CN"]["tabs."+group.id]=group.titleZh;
 I18N["en-US"]["tabs."+group.id]=group.titleEn;
});
/* 页面短名：中文 ≤ 4 字，保证 Ribbon 按钮与树条目都放得下 */
var PAGE_NAME = {
 "welcome": "欢迎总览", "flow-runtime": "运行工作台", "flow-lab": "能力实验室",
 "core": "核心工具", "bus": "进程总线", "bus-integrations": "总线集成",
 "files-json": "格式读写", "files-archive": "压缩配置", "files-format": "数据索引",
 "image-icon": "图标生成", "image-qr": "二维码",
 "log-core": "日志核心", "log-microsoft": "MEL 接入", "logging-options": "日志选项",
 "net-packet": "序列化", "net-socket": "网络通信"
};
/* 页面原名（英文 API / 包名）：页面正文小字与命令面板备注用 */
var PAGE_ALIAS = {
 "welcome": "Welcome", "flow-runtime": "运行态工作流", "flow-lab": "能力流水线",
 "core": "CodeWF.Toolkit.Core", "bus": "EventBus", "bus-integrations": "EventBus.Prism / AspNetCore / Socket",
 "files-json": "JsonExtensions / YamlExtensions", "files-archive": "SevenZipCompressor / AppConfigHelper", "files-format": "Files 能力矩阵",
 "image-icon": "ImageHelper.MergeGenerateIcon", "image-qr": "QrCodeGenerator",
 "log-core": "Logger / LogEventFeed", "log-microsoft": "CodeWFLoggerProvider", "logging-options": "LoggerOptions / SelfDiagnostics",
 "net-packet": "NetWeaver", "net-socket": "NetWrapper"
};
/* 图标：统一 24 网格线性图标（图标思路对齐 IconPark / TDesign，实现期可直接映射到 Semi / Ursa / Iconica） */
function svg(view, extra) {
 return '<svg class="svgic ' + (extra || "s20") + '" viewBox="0 0 24 24" aria-hidden="true">' + view + "</svg>";
}
var TAB_ICON = {
 "welcome": '<path d="M4.5 10.8L12 4l7.5 6.8"/><path d="M6.5 10v9.5h11V10"/><path d="M10.5 19.5v-4.8h3v4.8"/>',
 "flow-runtime": '<rect x="3" y="4.5" width="18" height="12.5" rx="1.7"/><path d="M8 21h8M12 17v4"/><path d="M7 9.5l2.5 2 3-3 4 4"/>',
 "flow-lab": '<path d="M10 3.5v6.2L5.6 17a2.4 2.4 0 0 0 2.1 3.5h8.6a2.4 2.4 0 0 0 2.1-3.5L14 9.7V3.5"/><path d="M8.5 3.5h7"/><path d="M7.6 14h8.8"/>',
 "core": '<path d="M12 3.5l7.5 4.2v8.6L12 20.5 4.5 16.3V7.7z"/><path d="M9.5 12l2 2 3.5-4"/>',
 "bus": '<circle cx="12" cy="12" r="2.6"/><circle cx="5" cy="6.5" r="2"/><circle cx="19" cy="6.5" r="2"/><circle cx="5" cy="17.5" r="2"/><circle cx="19" cy="17.5" r="2"/><path d="M6.6 7.8l3.6 2.9M17.4 7.8l-3.6 2.9M6.6 16.2l3.6-2.9M17.4 16.2l-3.6-2.9"/>',
 "bus-integrations": '<rect x="4" y="4" width="7" height="7" rx="1.5"/><rect x="13" y="13" width="7" height="7" rx="1.5"/><path d="M11 7.5h4a2 2 0 0 1 2 2v3.5"/><path d="M13 16.5H9a2 2 0 0 1-2-2V11"/>',
 "files-json": '<path d="M13.5 3.5H7a1.6 1.6 0 0 0-1.6 1.6v13.8A1.6 1.6 0 0 0 7 20.5h10a1.6 1.6 0 0 0 1.6-1.6V8.6z"/><path d="M13.5 3.5v5.1h5.1"/><path d="M9.5 13.5l-1.2 1.5 1.2 1.5M14.5 13.5l1.2 1.5-1.2 1.5"/>',
 "files-archive": '<path d="M4 7.5h16v11.5H4z"/><path d="M2.5 4.5h19v3h-19z"/><path d="M11 12h2v3h-2z"/>',
 "files-format": '<ellipse cx="12" cy="6" rx="7" ry="2.8"/><path d="M5 6v6c0 1.5 3.1 2.8 7 2.8s7-1.3 7-2.8V6"/><path d="M5 12v6c0 1.5 3.1 2.8 7 2.8s7-1.3 7-2.8v-6"/>',
 "image-icon": '<rect x="3.5" y="4.5" width="17" height="15" rx="1.7"/><circle cx="9" cy="10" r="1.8"/><path d="M4.5 17l4.5-4.5 3.5 3.5 2.5-2.5 5 5"/>',
 "image-qr": '<rect x="4" y="4" width="6.5" height="6.5" rx="1"/><rect x="13.5" y="4" width="6.5" height="6.5" rx="1"/><rect x="4" y="13.5" width="6.5" height="6.5" rx="1"/><path d="M13.5 13.5h3v3h-3zM17.5 17.5h2.5v2.5h-2.5z"/>',
 "log-core": '<path d="M5.5 4.5h13v15h-13z"/><path d="M8.5 9h7M8.5 12.5h7M8.5 16h4"/>',
 "log-microsoft": '<path d="M12 3.5l8.5 4.5v8L12 20.5 3.5 16V8z"/><path d="M8.5 12h7M12 8.5v7"/>',
 "logging-options": '<circle cx="12" cy="12" r="3"/><path d="M12 3.5v3M12 17.5v3M3.5 12h3M17.5 12h3M6 6l2.1 2.1M15.9 15.9L18 18M18 6l-2.1 2.1M8.1 15.9L6 18"/>',
 "net-packet": '<path d="M12 3.5l8 4.5v8l-8 4.5-8-4.5v-8z"/><path d="M4 8l8 4.5 8-4.5M12 12.5v8"/>',
 "net-socket": '<path d="M9.5 14.5l5-5"/><path d="M7.5 10.5l-2 2a3.5 3.5 0 0 0 5 5l2-2"/><path d="M16.5 13.5l2-2a3.5 3.5 0 0 0-5-5l-2 2"/>'
};
function iconSvg(id, extra) { return svg(TAB_ICON[id] || TAB_ICON["welcome"], extra || "s20"); }
/* 页签图标（Ribbon 页签行的极简小图标） */
var TAB_TAB_ICON = {
 home: '<path d="M4.5 10.8L12 4l7.5 6.8"/><path d="M6.5 10v9.5h11V10"/>',
 runtime: '<rect x="3" y="4.5" width="18" height="12.5" rx="1.7"/><path d="M8 21h8M12 17v4"/>',
 lab: '<path d="M10 3.5v6.2L5.6 17a2.4 2.4 0 0 0 2.1 3.5h8.6a2.4 2.4 0 0 0 2.1-3.5L14 9.7V3.5"/><path d="M8.5 3.5h7"/>',
 core: '<path d="M12 3.5l7.5 4.2v8.6L12 20.5 4.5 16.3V7.7z"/>',
 bus: '<circle cx="12" cy="12" r="2.6"/><circle cx="5" cy="6.5" r="2"/><circle cx="19" cy="17.5" r="2"/><path d="M6.6 7.8l3.6 2.9M17.4 16.2l-3.6-2.9"/>',
 files: '<path d="M13.5 3.5H7a1.6 1.6 0 0 0-1.6 1.6v13.8A1.6 1.6 0 0 0 7 20.5h10a1.6 1.6 0 0 0 1.6-1.6V8.6z"/><path d="M13.5 3.5v5.1h5.1"/>',
 logging: '<path d="M5.5 4.5h13v15h-13z"/><path d="M8.5 9h7M8.5 12.5h7M8.5 16h4"/>'
};
function tabIconSvg(id) { return svg(TAB_TAB_ICON[id] || TAB_TAB_ICON["home"], "s14"); }
function tabName(id) { return tt("tabs." + id); }
function pt(id) { return PAGE_NAME[id] || id; }
function pa(id) { return PAGE_ALIAS[id] || ""; }
/* 页面索引（欢迎页用）：短名 + 原名，保证「能力没被藏起来」 */
function pageIndexHtml() {
 return CATALOG.map(function (g) {
  return '<tr><td style="white-space:nowrap">' + esc(tabName(g.id)) + "</td><td>" +
   g.items.map(function (it) { return esc(pt(it.id)) + (pa(it.id) ? ' <span style="color:var(--tx3)">(' + esc(pa(it.id)) + ")</span>" : ""); }).join(" · ") +
   "</td></tr>";
 }).join("");
}
function groupOf(id) {
 for (var i = 0; i < CATALOG.length; i++) {
  if (CATALOG[i].items.some(function (x) { return x.id === id; })) return CATALOG[i];
 }
 return null;
}
function allPageIds() {
 var n = [];
 CATALOG.forEach(function (g) { g.items.forEach(function (it) { n.push(it.id); }); });
 return n;
}
function docCrumb(id) { var g = groupOf(id); return g ? tabName(g.id) + " / " + pt(id) : pt(id); }

/* ═══ 页面注册表 ═══ */
var PROTO = {};
function REG(id, o) { PROTO[id] = o; }
function renderInto(id, el) {
 var pg = PROTO[id]; if (!pg || !el) return;
 el.replaceChildren();
 if (pg.render) {
  var out = pg.render(el);
  if (typeof out === "string") el.innerHTML = out;
 } else el.innerHTML = (pg.zh && pg.zh.html) || "";
}
function toWindow(names) {
 names.forEach(function (n) { try { window[n] = eval(n); } catch (e) { } });
}

/* ═══ 属性面板：全局项（各处共用） ═══ */
var SELF_PROPS = [
 { id: "appearance.density", label: "密度", type: "combo", options: ["舒适", "紧凑"], value: "舒适", global: true,
  apply: function (v) { setDensity(v === "紧凑" ? "compact" : "cozy"); } },
 { id: "appearance.palette", label: "色板", type: "combo", options: ["默认", "水生", "沙漠", "黄昏", "夜空", "高对比"], value: "默认", global: true,
  apply: function (v) {
   var m = { "默认": "default", "水生": "aqua", "沙漠": "desert", "黄昏": "dusk", "夜空": "night", "高对比": "contrast" };
   setPalette(m[v] || "default");
  } },
 { id: "appearance.dark", label: "深色模式", type: "bool", value: true, global: true,
  apply: function (v) { setTheme(v ? "dark" : "light"); } },
 { id: "appearance.lang", label: "语言", type: "combo", options: ["简体中文", "English"], value: "简体中文", global: true,
  apply: function (v) { setLang(v === "English" ? "en-US" : "zh-CN"); } }
];

/* ═══ Ribbon / 树 / 文档 / 属性 ═══ */
var curTab = "home", OPEN_DOCS = [], ACTIVE_DOC = null, DIRTY = {}, RENAMES = {}, NAV_FILTER = "";
var SEG = {}, MODIFIED = {};

function renderRTabs() {
 $("rTabs").innerHTML = CATALOG.map(function (g) {
  return '<div class="t2' + (g.id === curTab ? " on" : "") + '" data-t="' + g.id + '" role="tab" tabindex="0" title="' +
   esc(tabName(g.id)) + '（双击折叠命令区）">' + tabIconSvg(g.id) + "<span>" + esc(tabName(g.id)) + "</span></div>";
 }).join("");
 document.querySelectorAll(".rt2 .t2").forEach(function (n) {
  n.onclick = function () { switchTab(n.dataset.t); };
  n.ondblclick = function () { toggleRibbon(); };
  n.onkeydown = function (e) { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); switchTab(n.dataset.t); } };
 });
 renderRBody();
}
function cmdBtn(it) {
 var label = pt(it.id), alias = pa(it.id);
 return '<button class="cbtn" data-p="' + it.id + '" title="' + esc(label + (alias ? "（" + alias + "）" : "")) +
  '" aria-pressed="' + (ACTIVE_DOC === it.id) + '">' +
  '<span class="ic">' + iconSvg(it.id) + '</span><span class="lb">' + esc(label) + "</span></button>";
}
function renderRBody() {
 var g = CATALOG.filter(function (x) { return x.id === curTab; })[0];
 if (!g) { $("rBody").innerHTML = ""; return; }
 var sceneFirst = g.items[0].id.indexOf("flow-") === 0;
 var html = "";
 if (sceneFirst) html += '<div class="grp"><div class="cmds">' + cmdBtn(g.items[0]) + '</div><div class="gname">场景</div></div>';
 var rest = sceneFirst ? g.items.slice(1) : g.items;
 html += '<div class="grp"><div class="cmds">' + rest.map(cmdBtn).join("") + '</div><div class="gname">' +
  esc(sceneFirst ? "能力页" : tabName(g.id)) + "</div></div>";
 $("rBody").innerHTML = html;
 document.querySelectorAll(".rbn .cbtn").forEach(function (b) { b.onclick = function () { openDoc(b.dataset.p); }; });
}
function switchTab(id) {
 curTab = id;
 renderRTabs(); renderTree();
 var g = CATALOG.filter(function (x) { return x.id === id; })[0];
 if (g && g.items.length) openDoc(g.items[0].id);
 log2both("切换页签：" + tabName(id));
}
function toggleRibbon() {
 document.body.classList.toggle("rmn");
 log2both((document.body.classList.contains("rmn") ? "折叠" : "展开") + "命令区");
}
function renderTree() {
 var q = NAV_FILTER.toLowerCase();
 var total = 0;
 var html = '<div class="fnd"><input id="navQ" placeholder="' + esc(tt("tree.filter")) + '" value="' + esc(NAV_FILTER) + '" title="' + esc(tt("tree.filter")) + '">' +
  (Object.keys(RENAMES).length ? '<button class="hbtn" style="visibility:visible" onclick="clearRenames()" title="' + esc(tt("tree.reset")) + '">↺</button>' : "") + "</div>";
 CATALOG.forEach(function (g) {
  var items = g.items.filter(function (it) {
   return !q || pt(it.id).toLowerCase().indexOf(q) >= 0 || pa(it.id).toLowerCase().indexOf(q) >= 0 || tabName(g.id).toLowerCase().indexOf(q) >= 0;
  });
  total += items.length;
  if (!items.length && q) return;
  var open = (g.id === curTab || (q && items.length)) ? " open" : "";
  html += '<div class="nd' + open + '" data-g="' + g.id + '">' +
   '<div class="hd" tabindex="0"><span class="arw">▶</span><span class="gi">' + tabIconSvg(g.id) + "</span><span>" + esc(tabName(g.id)) + "</span>" +
   '<span style="margin-left:auto;color:var(--tx3);font-size:10px">' + items.length + "</span></div><div class=\"kids\">" +
   items.map(function (it) {
    return '<div class="lf' + (it.id === ACTIVE_DOC ? " on" : "") + '" data-p="' + it.id + '" title="' + esc(pt(it.id)) + '">' +
     '<span class="ic">' + iconSvg(it.id, "s14") + '</span><span>' + esc(RENAMES[it.id] || pt(it.id)) + '</span><span class="sp2"></span>' +
     '<button class="hbtn" data-rn="' + it.id + '" title="' + esc(tt("tree.rename")) + '">✎</button></div>';
   }).join("") + "</div></div>";
 });
 if (!total) html += '<div class="empty"><div class="et">' + esc(tt("tree.empty")) + "</div></div>";
 $("lTree").innerHTML = html;
 var qi = $("navQ");
 if (qi) {
  qi.oninput = function () {
   NAV_FILTER = qi.value;
   var pos = qi.selectionStart;
   renderTree();
   var n2 = $("navQ");
   if (n2) { n2.focus(); try { n2.setSelectionRange(pos, pos); } catch (e) { } }
  };
 }
 document.querySelectorAll(".tree .nd .hd").forEach(function (h) {
  h.onclick = function (e) { if (e.target.tagName === "INPUT") return; switchTab(h.parentNode.dataset.g); };
 });
 document.querySelectorAll(".tree .lf").forEach(function (l) {
  l.onclick = function (e) { if (e.target.classList.contains("hbtn")) return; openDoc(l.dataset.p); };
  l.ondblclick = function (e) { if (e.target.classList.contains("hbtn")) return; startRename(l.dataset.p, l); };
 });
 document.querySelectorAll(".tree .hbtn[data-rn]").forEach(function (b) {
  b.onclick = function (e) { e.stopPropagation(); startRename(b.dataset.rn, b.closest(".lf")); };
 });
 var c = $("navCnt"); if (c) c.textContent = "(" + total + ")";
}
function startRename(id, lf) {
 if (!lf) return;
 lf.innerHTML = '<input class="rnm" value="' + esc(RENAMES[id] || pt(id)) + '">';
 var inp = lf.querySelector("input");
 inp.focus(); inp.select();
 var done = function (save) {
  if (save && inp.value.trim()) {
   RENAMES[id] = inp.value.trim();
   log2both("就地重命名：" + pt(id) + " → " + inp.value.trim());
  }
  renderTree(); renderDocTabs(); renderRBody();
 };
 inp.onblur = function () { done(true); };
 inp.onkeydown = function (e) { if (e.key === "Enter") done(true); if (e.key === "Escape") done(false); };
}
function clearRenames() {
 RENAMES = {};
 renderTree(); renderDocTabs(); renderRBody();
 notify("info", tt("nc.info"), tt("tree.reset"));
}
function openDoc(id) {
 if (!PROTO[id]) return;
 if (OPEN_DOCS.indexOf(id) < 0) OPEN_DOCS.push(id);
 ACTIVE_DOC = id;
 renderDocTabs(); renderDoc(id); renderTree(); renderRBody();
 log2both("打开文档：" + pt(id));
}
function closeDoc(id) {
 if (DIRTY[id]) { confirmDlg(tt("dlg.leave.t"), tt("dlg.leave.d"), function () { DIRTY[id] = false; doCloseDoc(id); }); return; }
 doCloseDoc(id);
}
function doCloseDoc(id) {
 OPEN_DOCS = OPEN_DOCS.filter(function (x) { return x !== id; });
 if (ACTIVE_DOC === id) ACTIVE_DOC = OPEN_DOCS[OPEN_DOCS.length - 1] || null;
 renderDocTabs(); renderTree(); renderRBody();
 if (ACTIVE_DOC) renderDoc(ACTIVE_DOC); else renderEmptyDoc();
 renderPGrid(ACTIVE_DOC);
 log2both("关闭文档：" + pt(id));
}
function closeAllDocsAsk() {
 confirmDlg(tt("dlg.del.t"), tt("dlg.del.d"), function () {
  OPEN_DOCS = []; ACTIVE_DOC = null; DIRTY = {};
  renderDocTabs(); renderEmptyDoc(); renderTree(); renderRBody(); renderPGrid(null);
  notify("warn", tt("nc.warn"), "已关闭全部文档");
  log2both("已关闭全部文档", "warn");
 });
}
function markDirty(id) {
 if (DIRTY[id]) return;
 DIRTY[id] = true;
 renderDocTabs();
 log2both("文档已修改：" + pt(id), "warn");
}
function renderDocTabs() {
 $("dT").innerHTML = OPEN_DOCS.map(function (id) {
  var name = (RENAMES[id] || pt(id)) + (DIRTY[id] ? " ●" : "");
  return '<div class="dt' + (id === ACTIVE_DOC ? " on" : "") + '" data-p="' + id + '" title="' + esc(name) + '">' +
   "<span>" + esc(name) + '</span><span class="x" data-x="' + id + '" title="关闭（未保存会二次确认）">✕</span></div>';
 }).join("");
 document.querySelectorAll(".dt").forEach(function (n) {
  n.onclick = function (e) {
   var x = e.target.closest("[data-x]");
   if (x) closeDoc(x.getAttribute("data-x")); else openDoc(n.getAttribute("data-p"));
  };
 });
 var st = $("stDocs"); if (st) st.textContent = OPEN_DOCS.length;
}
function renderEmptyDoc() {
 $("dBody").innerHTML = '<div class="empty"><div class="ei">' +
  '<svg class="svgic" style="width:34px;height:34px;color:var(--tx3)" viewBox="0 0 24 24" aria-hidden="true">' +
  '<path d="M12 3.5l8 4.5v8l-8 4.5-8-4.5v-8z"/><path d="M4 8l8 4.5 8-4.5M12 12.5v8"/></svg>' +
  '</div><div class="et">' + esc(tt("empty.title")) + "</div>" +
  '<div class="ed">' + esc(tt("empty.desc")) + '</div>' +
  '<button class="btn pri" onclick="openDoc(\'welcome\')">' + esc(tt("empty.action")) + "</button></div>";
 var w = $("stWhere"); if (w) w.textContent = tt("st.where");
}
/* 页面元信息：既支持新的 zh{t,d,f}，也支持旧的 zh{html}（10 个字典页沿用旧结构） */
function pageMeta(pg) {
 var zh = pg.zh || {};
 return { t: zh.t || pt(pg.id), d: zh.d || "", f: zh.f || [], html: zh.html, props: zh.props || [] };
}
function renderDoc(id) {
 var pg = PROTO[id]; if (!pg) return;
 pg.id = id;
 var meta = pageMeta(pg);
 SEG[id] = SEG[id] || "demo";
 var seg = SEG[id];
 var hasDemo = !!pg.render || !!meta.html;
 $("dBody").innerHTML =
  '<div class="crumb">' + esc(docCrumb(id)) + "</div><h1>" + esc(RENAMES[id] || meta.t) + "</h1>" +
  (pa(id) ? '<div class="alias">' + esc(pa(id)) + "</div>" : "") +
  (meta.d ? '<div class="dsc">' + esc(meta.d) + "</div>" : "") +
  (meta.f.length ? '<div class="ftg">' + meta.f.map(function (f) { return "<span>" + esc(f) + "</span>"; }).join("") + "</div>" : "") +
  '<div class="smt" id="smt-' + id + '">' +
  [["demo", "演示"], ["xaml", "XAML"], ["cs", "C#"]].filter(function (s) {
   return (s[0] === "demo" && hasDemo) || (s[0] !== "demo" && pg[s[0]]);
  }).map(function (s) {
   return '<button data-s="' + s[0] + '"' + (seg === s[0] ? ' class="on"' : "") + ">" + s[1] + "</button>";
  }).join("") + '</div><div id="seg-' + id + '"></div>';
 var target = $("seg-" + id);
 if (seg === "demo") {
  if (pg.render) {
   target.innerHTML = '<div class="skel"><i></i><i></i><i></i><i></i><i></i><i></i></div>';
   setTimeout(function () {
    if (ACTIVE_DOC !== id) return;
    var t2 = $("seg-" + id); if (!t2) return;
    renderInto(id, t2);
    if (pg.after) guard(function () { pg.after(); }, "after:" + id);
   }, 90);
  } else {
   target.innerHTML = meta.html || "";
   if (pg.after) guard(function () { pg.after(); }, "after:" + id);
  }
 } else {
  target.innerHTML = '<div class="smcode">' + esc(pg[seg]) + "</div>";
 }
 document.querySelectorAll("#smt-" + id + " button").forEach(function (b) {
  b.onclick = function () {
   SEG[id] = b.dataset.s;
   document.querySelectorAll("#smt-" + id + " button").forEach(function (x) { x.classList.toggle("on", x === b); });
   renderDoc(id);
  };
 });
 renderPGrid(id);
 var w = $("stWhere"); if (w) w.textContent = docCrumb(id);
}

/* ═══ 属性面板渲染 ═══ */
function propKey(pageId, p) { return (p.global ? "" : (pageId || "")) + "|" + p.id; }
function rowHtml(pageId, p) {
 var mod = MODIFIED[propKey(pageId, p)] ? " mod" : "";
 var ctl;
 if (p.kind === "ro") ctl = '<span class="ro" title="' + esc(String(p.value)) + '">' + esc(String(p.value)) + "</span>";
 else if (p.type === "combo" || p.kind === "sel") {
  var opts = p.options || [];
  ctl = '<select data-id="' + p.id + '"' + (p.readonly ? " disabled" : "") + ">" +
   opts.map(function (o) { return "<option" + (o === p.value ? " selected" : "") + ">" + esc(o) + "</option>"; }).join("") + "</select>";
 } else if (p.type === "bool" || p.kind === "sw") {
  ctl = '<div class="sw' + (p.value ? " on" : "") + '" data-id="' + p.id + '" role="switch" tabindex="0" title="' + esc(p.label || p.name) + '"></div>';
 } else if (p.type === "range") {
  ctl = '<input type="range" data-id="' + p.id + '" min="' + p.min + '" max="' + p.max + '" step="' + (p.step || 1) + '" value="' + p.value + '">';
 } else {
  ctl = '<input type="number" data-id="' + p.id + '" value="' + esc(String(p.value)) + '">';
 }
 return '<div class="rw' + mod + '"><span class="nm" title="' + esc(p.label || p.name) + '">' + esc(p.label || p.name) + '</span><span class="vl">' + ctl + "</span></div>";
}
function renderPGrid(id) {
 var pg = id ? PROTO[id] : null;
 var pageProps = pg ? (pg.props || pageMeta(pg).props) : [];
 var html = '<div class="cat">' + esc(tt("ph.top")) + " · " + esc(tt("ph.basic")) + "</div>" +
  SELF_PROPS.map(function (p) { return rowHtml(id, p); }).join("");
 html += '<div class="cat">' + esc(id ? pt(id) : tt("ph.doc")) + "</div>";
 html += pageProps.length ? pageProps.map(function (p) { return rowHtml(id, p); }).join("")
  : '<div class="pgp-empty">当前文档无属性</div>';
 html += '<div class="cat">' + esc(tt("ph.state")) + "</div>" +
  '<div class="rw"><span class="nm">打开文档数</span><span class="vl"><span class="ro">' + OPEN_DOCS.length + "</span></span></div>" +
  '<div class="rw"><span class="nm">日志级别</span><span class="vl"><span class="ro">' + esc(LOG_LEVEL) + "</span></span></div>";
 $("pGrid").innerHTML = html;
 var cnt = $("pgCnt"); if (cnt) cnt.textContent = "(" + (SELF_PROPS.length + pageProps.length) + " 项)";
 SELF_PROPS.concat(pageProps).forEach(function (p) {
  var el = document.querySelector('#pGrid [data-id="' + p.id + '"]');
  if (!el) return;
  if (p.type === "range") el.oninput = function () { applyProp(id, p, +el.value, true); };
  else if (p.type === "bool" || p.kind === "sw") el.onclick = function () { el.classList.toggle("on"); applyProp(id, p, el.classList.contains("on"), true); };
  else if (p.kind === "ro") { }
  else if (p.type === "combo" || p.kind === "sel") el.onchange = function () { applyProp(id, p, el.value); };
  else el.onchange = function () { applyProp(id, p, el.value); };
 });
}
function applyProp(pageId, p, v, light) {
 p.value = v;
 var k = propKey(pageId, p);
 var first = !MODIFIED[k];
 MODIFIED[k] = true;
 if (p.apply) guard(function () { p.apply(v); }, "prop:" + p.id);
 if (light && !first) return;
 renderPGrid(ACTIVE_DOC);
}

/* ═══ 外观 / 语言 / 面板 / 布局 ═══ */
var PAL_LIST = ["default", "aqua", "desert", "dusk", "night", "contrast"];
var PAL_COLOR = { default: "#0064fa", aqua: "#0091a8", desert: "#c26a1d", dusk: "#7a5cc4", night: "#2b6cb0", contrast: "#888" };
function setPalette(v) {
 document.documentElement.dataset.palette = v;
 var sel = $("palSel"); if (sel) sel.value = v;
 SELF_PROPS[1].value = { default: "默认", aqua: "水生", desert: "沙漠", dusk: "黄昏", night: "夜空", contrast: "高对比" }[v] || "默认";
 renderPalDots();
 log2both(tt("toast.palette") + SELF_PROPS[1].value);
}
function renderPalDots() {
 var cur = document.documentElement.dataset.palette;
 $("palDots").innerHTML = PAL_LIST.map(function (p) {
  return '<i style="background:' + PAL_COLOR[p] + '" data-p="' + p + '" title="' + p + '"' + (p === cur ? ' class="on"' : "") + "></i>";
 }).join("");
 document.querySelectorAll(".pal i").forEach(function (i) { i.onclick = function () { setPalette(i.getAttribute("data-p")); }; });
}
function setTheme(v) {
 document.documentElement.dataset.theme = v;
 SELF_PROPS[2].value = (v === "dark");
 log2both(tt("toast.theme") + (v === "dark" ? tt("theme.dark") : tt("theme.light")));
}
function toggleDark() { setTheme(document.documentElement.dataset.theme === "dark" ? "light" : "dark"); }
function setDensity(v) {
 document.documentElement.dataset.density = v;
 SELF_PROPS[0].value = (v === "compact" ? "紧凑" : "舒适");
 log2both(tt("toast.density") + (v === "compact" ? tt("density.compact") : tt("density.cozy")));
}
function setLang(l) {
 LANG = l;
 document.documentElement.lang = l;
 SELF_PROPS[3].value = (l === "en-US" ? "English" : "简体中文");
 document.querySelectorAll("[data-i18n]").forEach(function (n) { n.textContent = tt(n.dataset.i18n); });
 var cp = $("cpmQ"); if (cp) cp.placeholder = tt("cmd.ph");
 renderRTabs(); renderTree(); renderDocTabs();
 if (ACTIVE_DOC) renderDoc(ACTIVE_DOC); else renderEmptyDoc();
 renderPGrid(ACTIVE_DOC);
 log2both(tt("toast.lang") + l);
}
function togglePanel(which) {
 var key = which === "L" ? "hideL" : which === "R" ? "hideR" : "hideLog";
 document.body.classList.toggle(key);
 var hidden = document.body.classList.contains(key);
 log2both((which === "L" ? "导航树" : which === "R" ? "属性" : "日志") + "面板：" + (hidden ? "隐藏" : "显示"));
 if (ACTIVE_DOC === "flow-runtime" || ACTIVE_DOC === "flow-lab") renderDoc(ACTIVE_DOC);
}
var LAYOUT_KEY = "codewf-toolkit-demo-layout";
function saveLayout() {
 var st = {
  palette: document.documentElement.dataset.palette, theme: document.documentElement.dataset.theme,
  density: document.documentElement.dataset.density, lang: LANG, tab: curTab,
  hideL: document.body.classList.contains("hideL"), hideR: document.body.classList.contains("hideR"),
  hideLog: document.body.classList.contains("hideLog"), docs: OPEN_DOCS.slice(0, 8), active: ACTIVE_DOC
 };
 var ok = true;
 try { localStorage.setItem(LAYOUT_KEY, JSON.stringify(st)); } catch (e) { ok = false; }
 notify(ok ? "ok" : "warn", ok ? tt("nc.ok") : tt("nc.warn"), ok ? tt("toast.saved") : "浏览器本地存储不可用（file:// 下部分浏览器会禁用）");
 log2both(tt("toast.saved"));
}
function loadLayout() {
 var raw = null;
 try { raw = localStorage.getItem(LAYOUT_KEY); } catch (e) { return false; }
 if (!raw) return false;
 try {
  var st = JSON.parse(raw);
  if (st.palette) setPalette(st.palette);
  if (st.theme) setTheme(st.theme);
  if (st.density) setDensity(st.density);
  ["hideL", "hideR", "hideLog"].forEach(function (k) { document.body.classList.toggle(k, !!st[k]); });
  if (st.tab) curTab = st.tab;
  if (st.docs && st.docs.length) {
   OPEN_DOCS = st.docs.filter(function (d) { return !!PROTO[d]; });
   ACTIVE_DOC = OPEN_DOCS.indexOf(st.active) >= 0 ? st.active : OPEN_DOCS[OPEN_DOCS.length - 1];
  }
  if (st.lang) setLang(st.lang);
  if (st.density) setDensity(st.density);
  return true;
 } catch (e) { return false; }
}
function layoutResetAsk() {
 confirmDlg(tt("dlg.reset.t"), tt("dlg.reset.d"), function () {
  document.body.classList.remove("hideL", "hideR", "hideLog", "rmn");
  setDensity("cozy"); setPalette("dotnet"); setTheme("dark");
  try { localStorage.removeItem(LAYOUT_KEY); } catch (e) { }
  notify("warn", tt("nc.warn"), tt("toast.reset"));
  log2both(tt("toast.reset"), "warn");
  if (ACTIVE_DOC) renderDoc(ACTIVE_DOC); else renderEmptyDoc();
  renderPGrid(ACTIVE_DOC);
 });
}

/* ═══ Guide（5 步，可跳过） ═══ */
var gSteps = [], gIdx = 0;
function guideOpen() {
 gSteps = [{ t: tt("g.1.t"), d: tt("g.1.d"), sel: ".rtabs" },
  { t: tt("g.2.t"), d: tt("g.2.d"), sel: ".p-nav" },
  { t: tt("g.3.t"), d: tt("g.3.d"), sel: ".p-doc" },
  { t: tt("g.4.t"), d: tt("g.4.d"), sel: ".p-props" },
  { t: tt("g.5.t"), d: tt("g.5.d"), sel: ".p-log" }];
 gIdx = 0; $("gMask").classList.add("on"); $("gTip").classList.add("on"); showStep();
}
function guideGo(steps) { gSteps = steps; gIdx = 0; $("gMask").classList.add("on"); $("gTip").classList.add("on"); showStep(); }
function showStep() {
 var s = gSteps[gIdx];
 if (!s) { gCloseGuide(); return; }
 var el = document.querySelector(s.sel);
 if (!el) { gIdx++; showStep(); return; }
 var r = el.getBoundingClientRect(), sp = $("gSpot");
 sp.style.display = "block";
 sp.style.left = (r.left - 6) + "px"; sp.style.top = (r.top - 6) + "px";
 sp.style.width = (r.width + 12) + "px"; sp.style.height = (r.height + 12) + "px";
 var tip = $("gTip");
 tip.style.display = "block";
 tip.classList.remove("on"); void tip.offsetWidth; tip.classList.add("on");
 var tw = tip.offsetWidth || 320;
 tip.style.left = clamp(r.left, 10, Math.max(10, innerWidth - tw - 10)) + "px";
 var ty = r.bottom + 12;
 if (ty + 170 > innerHeight) ty = Math.max(10, r.top - 170);
 tip.style.top = ty + "px";
 $("gT").textContent = s.t; $("gD").textContent = s.d;
 $("gN").textContent = (gIdx + 1) + " / " + gSteps.length;
 $("gPrev").textContent = tt("g.prev"); $("gNext").textContent = tt("g.next");
 $("gSkip").textContent = tt("g.skip"); $("gClose").textContent = tt("g.close");
 log2both("引导：" + s.t);
}
function gNextStep() { gIdx++; if (gIdx >= gSteps.length) gCloseGuide(); else showStep(); }
function gPrevStep() { gIdx = Math.max(0, gIdx - 1); showStep(); }
function gCloseGuide() { $("gMask").classList.remove("on"); $("gTip").classList.remove("on"); $("gSpot").style.display = "none"; $("gTip").style.display = "none"; }

/* ═══ 命令面板（Ctrl+K） ═══ */
var cpmAll = [], cpmIdx = 0;
function cmdList() {
 var list = allPageIds().map(function (id) {
  return {
   label: pt(id), group: tt("cmd.pages") + " · " + docCrumb(id), icon: TAB_ICON[id] || "◆",
   run: function () { curTab = (groupOf(id) || { id: "home" }).id; renderRTabs(); renderTree(); openDoc(id); }
  };
 });
 var C = [
  { label: "切换深色 / 浅色", icon: "◐", run: toggleDark },
  { label: "切换语言 中 / EN", icon: "文A", run: function () { setLang(LANG === "zh-CN" ? "en-US" : "zh-CN"); } },
  { label: "切换密度 舒适 / 紧凑", icon: "▤", run: function () { setDensity(document.documentElement.dataset.density === "compact" ? "cozy" : "compact"); } },
  { label: "保存布局", icon: "💾", run: saveLayout },
  { label: "重置工作区", icon: "↺", run: layoutResetAsk },
  { label: "切换日志面板", icon: "▤", run: function () { togglePanel("Log"); } },
  { label: "切换导航树", icon: "◈", run: function () { togglePanel("L"); } },
  { label: "切换属性面板", icon: "⚙", run: function () { togglePanel("R"); } },
  { label: "关闭全部文档", icon: "✕", run: closeAllDocsAsk },
  { label: "打开引导", icon: "❔", run: guideOpen },
  { label: "运行态工作台：检查更新", icon: "⬆", run: function () { openDoc("flow-runtime"); guard(function () { rtCheck(); }, "cmd:update"); } },
  { label: "能力实验室：一键全流程", icon: "🧪", run: function () { openDoc("flow-lab"); guard(function () { labRunAll(); }, "cmd:lab"); } }
 ];
 C.forEach(function (c) { c.group = tt("cmd.cmds"); list.push(c); });
 var palLabels = SELF_PROPS[1].options;
 PAL_LIST.forEach(function (p, i) {
  list.push({ label: tt("toast.palette") + palLabels[i], group: tt("cmd.cmds"), icon: "🎨", run: function () { setPalette(p); } });
 });
 return list;
}
function cpmOpen() { $("cpm").classList.add("on"); $("cpmQ").value = ""; cpmIdx = 0; cpmDraw(); $("cpmQ").focus(); }
function cpmClose() { $("cpm").classList.remove("on"); }
function cpmDraw() {
 var q = ($("cpmQ").value || "").toLowerCase();
 cpmAll = cmdList().filter(function (c) { return !q || c.label.toLowerCase().indexOf(q) >= 0 || c.group.toLowerCase().indexOf(q) >= 0; });
 if (cpmIdx >= cpmAll.length) cpmIdx = 0;
 $("cpmLs").innerHTML = cpmAll.length ? cpmAll.map(function (c, i) {
  return '<div class="it' + (i === cpmIdx ? " on" : "") + '" data-i="' + i + '"><span>' + c.icon + "</span><span>" +
   esc(c.label) + '</span><span class="g">' + esc(c.group) + "</span></div>";
 }).join("") : '<div class="empty"><div class="et">无匹配</div></div>';
 document.querySelectorAll("#cpmLs .it").forEach(function (n) { n.onclick = function () { cpmIdx = +n.dataset.i; cpmRun(); }; });
}
function cpmMove(d) {
 if (!cpmAll.length) return;
 cpmIdx = (cpmIdx + d + cpmAll.length) % cpmAll.length;
 cpmDraw();
 var on = document.querySelector("#cpmLs .it.on");
 if (on) on.scrollIntoView({ block: "nearest" });
}
function cpmRun() { var c = cpmAll[cpmIdx]; if (!c) return; cpmClose(); log2both("命令面板执行：" + c.label); guard(c.run, "cmd"); }

/* ═══ 事件绑定 ═══ */
function bindShell() {
 $("rTabsBar").ondblclick = function (e) { if (e.target.classList.contains("t2")) toggleRibbon(); };
 $("palSel").onchange = function () { setPalette(this.value); };
 $("darkBtn").onclick = toggleDark;
 $("darkBtn2").onclick = toggleDark;
 $("langBtn").onclick = function () { setLang(LANG === "zh-CN" ? "en-US" : "zh-CN"); };
 $("langBtn2").onclick = $("langBtn").onclick;
 $("qkGuide").onclick = guideOpen;
 $("qkCmd").onclick = cpmOpen;
 $("qkDensity").onclick = function () { setDensity(document.documentElement.dataset.density === "compact" ? "cozy" : "compact"); };
 $("qkSave").onclick = saveLayout;
 $("qkReset").onclick = layoutResetAsk;
 $("qkPalette").onclick = function () {
  var i = PAL_LIST.indexOf(document.documentElement.dataset.palette);
  setPalette(PAL_LIST[(i + 1) % PAL_LIST.length]);
 };
 $("hideNav").onclick = function () { togglePanel("L"); };
 $("hideProp2").onclick = function () { togglePanel("R"); };
 $("hideLogBtn").onclick = function () { togglePanel("Log"); };
 $("logToggleBtn").onclick = function () { togglePanel("Log"); };
 $("dkNav").onclick = $("hideNav").onclick;
 $("dkProps").onclick = $("hideProp2").onclick;
 $("dkLog").onclick = $("hideLogBtn").onclick;
 $("dkMax").onclick = function () { document.body.classList.add("hideL", "hideR"); log2both("文档区最大化"); };
 $("dkReset").onclick = layoutResetAsk;
 $("openLogDir").onclick = function () { log2both("打开日志目录（跨平台内联：Explorer / xdg-open / open）", "warn"); };
 $("clrLogBtn").onclick = function () {
  confirmDlg(tt("dlg.del.t"), "清空底部日志面板？该操作不影响日志文件。", function () {
   $("logP").innerHTML = "";
   notify("warn", tt("nc.warn"), "日志面板已清空");
   log2both("日志面板已清空", "warn");
  });
 };
 $("cpmQ").oninput = cpmDraw;
 $("cpmQ").onkeydown = function (e) {
  if (e.key === "ArrowDown") { e.preventDefault(); cpmMove(1); }
  else if (e.key === "ArrowUp") { e.preventDefault(); cpmMove(-1); }
  else if (e.key === "Enter") { e.preventDefault(); cpmRun(); }
  else if (e.key === "Escape") { cpmClose(); }
 };
 $("cpm").onclick = function (e) { if (e.target === $("cpm")) cpmClose(); };
 $("dlgYes").onclick = function () { var f = _dlgYes; closeDlg(); if (f) guard(f, "dlg"); };
 $("dlgNo").onclick = closeDlg;
 $("dlgm").onclick = function (e) { if (e.target === $("dlgm")) closeDlg(); };
 $("gNext").onclick = gNextStep;
 $("gPrev").onclick = gPrevStep;
 $("gSkip").onclick = function () { gCloseGuide(); log2both("引导已跳过"); };
 $("gClose").onclick = gCloseGuide;
 $("gMask").onclick = gCloseGuide;
 document.addEventListener("keydown", function (e) {
  var k = (e.key || "").toLowerCase();
  var mod = e.ctrlKey || e.metaKey;
  if (mod && k === "k") { e.preventDefault(); $("cpm").classList.contains("on") ? cpmClose() : cpmOpen(); return; }
  if (mod && k === "d") { e.preventDefault(); toggleDark(); return; }
  if (mod && k === "l") { e.preventDefault(); setLang(LANG === "zh-CN" ? "en-US" : "zh-CN"); return; }
  if (mod && k === "s") { e.preventDefault(); saveLayout(); return; }
  if (e.key === "F1") { e.preventDefault(); guideOpen(); return; }
  if (e.key === "Escape") {
   if ($("cpm").classList.contains("on")) cpmClose();
   else if ($("dlgm").classList.contains("on")) closeDlg();
   else gCloseGuide();
  }
 });
}

/* ═══ 启动（由最后一个页面脚本调用） ═══ */
function boot() {
 bindShell();
 toWindow(["openDoc", "closeDoc", "markDirty", "clearRenames", "startRename", "closeAllDocsAsk", "layoutResetAsk",
  "saveLayout", "loadLayout", "togglePanel", "setLang", "setPalette", "setTheme", "setDensity", "toggleDark",
  "guideOpen", "guideGo", "gCloseGuide", "notify", "confirmDlg", "closeDlg", "cpmOpen", "cpmClose", "toggleRibbon",
  "dlog", "log2both", "renderTree", "renderRBody", "renderDoc", "renderPGrid", "switchTab", "setStatus"]);
 var restored = false;
 guard(function () { restored = loadLayout(); }, "loadLayout");
 if (!restored) renderPalDots();
 renderRTabs(); renderTree(); renderPalDots();
 if (OPEN_DOCS.length) { renderDocTabs(); renderDoc(ACTIVE_DOC); }
 else openDoc("welcome");
 renderPGrid(ACTIVE_DOC);
 dlog("CodeWF.Toolkit 演示台就绪（7 页签 / 16 页）", "info");
 dlog("信息架构：2 个场景工作流（运行态工作台 / 能力实验室）+ 4 个能力字典分区（核心 / 通信 / 数据 / 日志）", "info");
 if (restored) dlog("已从浏览器本地恢复上次布局", "info");
}
window.TK_BOOT = boot;
