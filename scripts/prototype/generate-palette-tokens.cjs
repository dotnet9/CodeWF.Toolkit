/* 从 CodeWF.Avalonia《配色与排版令牌规则.md》附录 A 生成原型配色令牌（规则 §14：禁止手写快照）。
 * 单一事实源：规则文档附录 A JSON 块。本脚本产出（均带上游版本戳）：
 *   assets/palette-tokens.css   9 套配色 × 明暗 + indigo 默认回退块（原型短变量词汇表）
 *   assets/palette-data.js      配色选择器数据：9 套 label/en/note/8 色条 + 旧 key 别名 + 默认 key
 * 共享派生引擎：assets/palette-rules.js（与 Vex 设计仓、CodeWF.Avalonia 原型逐字节同源，UMD，禁改公式）。
 * 用法：node scripts/prototype/generate-palette-tokens.cjs [--rules <规则文档路径>] [--check] [--force]
 *   --check 只校验不落盘；校验失败（规则 §5）默认退出码 1 且不落盘，--force 可强制。 */
const fs = require('node:fs');
const path = require('node:path');
const Rules = require('./assets/palette-rules.js');

const root = __dirname;
const args = process.argv.slice(2);
const flag = name => args.indexOf(name);
/* 规则文档由 CodeWF.Avalonia 维护，本仓原型只消费；默认取同级 Libs 仓库，避免手填绝对路径。 */
const rulesPath = flag('--rules') >= 0 ? path.resolve(args[flag('--rules') + 1])
  : (process.env.CODEWF_RULES_DOC || path.resolve(root, '../../../CodeWF.Avalonia/docs/配色与排版令牌规则.md'));
const upstreamRoot = path.dirname(path.dirname(rulesPath));

const doc = fs.readFileSync(rulesPath, 'utf8');
const appendix = doc.slice(Math.max(doc.indexOf('## 附录 A'), 0));
const dataMatch = appendix.match(/```json\s*([\s\S]*?)```/);
if (!dataMatch) throw new Error('规则文档附录 A 中未找到 json 数据块: ' + rulesPath);
const data = JSON.parse(dataMatch[1]);

function upstreamVersion() {
  try {
    const props = fs.readFileSync(path.join(upstreamRoot, 'Directory.Build.props'), 'utf8');
    return props.match(/<Version>([^<]+)<\/Version>/)[1];
  } catch (_) { return 'unknown'; }
}
const version = upstreamVersion();
const stamp = '生成物：scripts/prototype/generate-palette-tokens.cjs 产出，禁止手写（规则 §14）。'
  + ' 上游事实源：CodeWF.Avalonia ' + version + ' docs/配色与排版令牌规则.md（ruleVersion ' + data.ruleVersion + '）。';

/* 组装一套配色的完整基础色（状态四键缺省取全局默认，规则 §1）。 */
function baseOf(palette, mode) {
  const side = palette[mode];
  return Object.assign({}, side, {
    danger: side.danger || data.statusDefaults[mode].danger,
    success: side.success || data.statusDefaults[mode].success,
    warning: side.warning || data.statusDefaults[mode].warning,
    info: side.info || data.statusDefaults[mode].info
  });
}

/* ═══ 短变量映射表：规则语义键 → 原型既有 CSS 变量词汇（旧快照的全部 50 个变量逐一给出来源） ═══
 * 右侧为规则语义（§1 基础色 / §3 派生键 / §4 语义键）在 palette-rules.js deriveVars 输出中的对应项。
 * ORDER 即旧 palette-tokens.css 的变量顺序，便于 diff 审阅。 */
const ORDER = [
  'bg', 'bg2', 'bg3', 'tx', 'tx2', 'tx3', 'bd', 'bd2', 'line', 'hov', 'sel', 'workbg',
  'pri', 'pri-h', 'brand', 'glow', 'secondary', 'text-gradient-end',
  'button-bg', 'button-hover', 'button-pressed', 'on-primary', 'ok', 'warn', 'err',
  'badge-text', 'card-top',
  'badge-debug', 'badge-debug-bg', 'card-debug', 'card-debug-bg',
  'badge-info', 'badge-info-bg', 'card-info', 'card-info-bg',
  'badge-warn', 'badge-warn-bg', 'card-warn', 'card-warn-bg',
  'badge-error', 'badge-error-bg', 'card-error', 'card-error-bg',
  'badge-fatal', 'badge-fatal-bg', 'card-fatal', 'card-fatal-bg',
  'action-gradient', 'pri-lt'
];

function toolkitVars(base, mode, derivation) {
  const v = Rules.deriveVars(base, mode, derivation);
  /* fatal：把附录 primaryPressed 公式（mix(accent→ink/black)）作用于 danger——旧快照 fatal 比 error 深一档的等价派生；
   * fatal 底 = danger-pressed-bg（statusSoft + 4%，同按压档），公式常数仍全部来自附录。 */
  const fatal = Rules.resolveFormula(derivation.primaryPressed,
    { accent: base.danger, ink: base.ink, paper: base.paper, white: '#FFFFFF', black: '#000000' });
  return {
    /* 表面：paper=窗口/文档底，surface=面板侧栏（海拔 -1）；bg3/workbg=代码区与芯片底（旧快照 bg3==hov==workbg） */
    bg: base.paper,                    /* StudioBackgroundColor → 窗口舞台 */
    bg2: base.surface,                 /* StudioSurfaceColor → 面板/侧栏/工具栏 */
    bg3: v.hover,                      /* hover(ink@5/7%→surface) → 芯片/徽标/骨架屏底 */
    tx: base.ink,                      /* StudioTextColor */
    tx2: v.muted,                      /* muted(ink@64%→paper) 次要文字 */
    tx3: v.muted,                      /* 旧快照 tx2==tx3，保持同源 */
    bd: v['control-border'],           /* controlBorder(ink@20/26%→paper) 输入控件描边 */
    bd2: v.border,                     /* border(ink@12/16%→paper) 弱分隔线 */
    line: v.border,                    /* 同 border */
    hov: v.hover,                      /* hover(ink@5/7%→surface) */
    sel: v.selected,                   /* selected(accent@12/18%→paper) 选中项背景 */
    workbg: v.hover,                   /* 代码/控制台底，旧快照与 bg3 同层 */
    /* 品牌/交互：accent 唯一高饱和来源（§15 强调色唯一律） */
    pri: base.accent,                  /* StudioPrimaryColor / 链接/焦点色 */
    'pri-h': v['primary-hover'],       /* primaryHover mix(accent→paper/white,8%) */
    brand: base.accent,                /* 品牌位 */
    glow: base.accent,                 /* 焦点辉光/描边 */
    secondary: v['gradient-end'],      /* gradientEnd mix(accent→white/paper) 渐变第二停点 */
    'text-gradient-end': v['gradient-end'], /* StudioTextGradientEndColor */
    'button-bg': base.accent,          /* 主按钮底 = accent */
    'button-hover': v['primary-hover'],
    'button-pressed': v['primary-pressed'],
    'on-primary': v['on-accent'],      /* onAccent 自动判定（§3） */
    ok: v.success,                     /* success（statusDefaults 补齐） */
    warn: v.warning,
    err: v.danger,
    'badge-text': v['on-accent'],      /* 徽标文字 = onAccent */
    'card-top': base.ink,              /* 状态卡顶栏文字 = ink（与 Studio 令牌 StatusCardTopTextForeground 对齐） */
    /* 状态徽标/卡片（debug=accent 软底；info/warn/error=status + statusSoft；fatal 见上） */
    'badge-debug': base.accent, 'badge-debug-bg': v['accent-soft'], 'card-debug': base.ink, 'card-debug-bg': v['accent-soft'],
    'badge-info': v.success, 'badge-info-bg': v['success-soft'], 'card-info': v.success, 'card-info-bg': v['success-soft'],
    'badge-warn': v.warning, 'badge-warn-bg': v['warning-soft'], 'card-warn': v.warning, 'card-warn-bg': v['warning-soft'],
    'badge-error': v.danger, 'badge-error-bg': v['danger-soft'], 'card-error': v.danger, 'card-error-bg': v['danger-soft'],
    'badge-fatal': fatal, 'badge-fatal-bg': v['danger-pressed-bg'], 'card-fatal': fatal, 'card-fatal-bg': v['danger-pressed-bg'],
    'action-gradient': v['title-grad'], /* linear-gradient(135deg,accent,gradientEnd) 全界面唯一渐变 */
    'pri-lt': v['accent-soft']         /* accentSoft(accent@10/16%→paper) 软强调底 */
  };
}

function cssBlock(mode, paletteKey, vars) {
  const missing = ORDER.filter(name => vars[name] === undefined);
  if (missing.length) throw new Error('映射不完整，缺少变量: ' + missing.join(', '));
  const body = ORDER.map(name => '  --' + name + ':' + vars[name] + ';').join('\n');
  return ':root[data-theme=' + mode + ']' + (paletteKey ? '[data-palette=' + paletteKey + ']' : '')
    + ' {\ncolor-scheme:' + mode + ';\n' + body + '\n}';
}

/* 色卡数据：8 个独立圆点的颜色数组（paper/surface/ink/accent/danger/success/warning/info 浅色态）。
 * 渲染端输出 8 个 9px 小圆（.swatch-dots > i），每色一圈横向一行。 */
function swatchColors(base) {
  return [base.paper, base.surface, base.ink, base.accent, base.danger, base.success, base.warning, base.info];
}

/* 旧 Key → 新 Key 别名（规则 §2 替换声明：自用迁移）。含本原型旧 9 key 与 Vex 生成器同源别名。 */
const legacyAliases = {
  weft: 'teal', aquatic: 'teal', aqua: 'teal',
  ocean: 'indigo', blue: 'indigo', dusk: 'indigo', dotnet: 'indigo', 'night-sky': 'indigo', night: 'indigo', default: 'indigo',
  sand: 'amber', ochre: 'amber', desert: 'amber',
  rose: 'crimson', iris: 'violet', plum: 'violet', aurora: 'violet',
  olive: 'pine', ember: 'terracotta', contrast: 'graphite'
};

const issues = [];
const paletteCss = [];
const paletteEntries = [];
for (const [key, palette] of Object.entries(data.palettes)) {
  const entry = { key, label: palette.label, en: palette.en, note: palette.note, recommended: !!palette.recommended };
  for (const mode of ['light', 'dark']) {
    const base = baseOf(palette, mode);
    Rules.validate(base, mode, data.statusDefaults, data.derivation[mode])
      .forEach(message => issues.push(key + '/' + mode + ': ' + message));
    paletteCss.push(cssBlock(mode, key, toolkitVars(base, mode, data.derivation[mode])));
    if (mode === 'light') entry.swatch = swatchColors(base);
  }
  paletteEntries.push(entry);
}

const fallback = data.palettes[data.default];
if (!fallback) throw new Error('附录 A default 指向不存在的配色: ' + data.default);
const fallbackCss = ['light', 'dark']
  .map(mode => cssBlock(mode, null, toolkitVars(baseOf(fallback, mode), mode, data.derivation[mode])));

const badAlias = Object.entries(legacyAliases).filter(([, target]) => !data.palettes[target]);
if (badAlias.length) throw new Error('别名指向不存在的配色: ' + badAlias.map(([k, v]) => k + '->' + v).join(', '));

const tokensCss = '/* ' + stamp + ' */\n'
  + '/* 默认回退块 = ' + data.default + '（规则附录 A default）；8 套配色块以更高优先级覆盖。旧 key 别名映射见 assets/palette-data.js。 */\n'
  + '/* 变量语义映射表见 scripts/prototype/generate-palette-tokens.cjs。非配色变量（--sp* / --code 等）仍由 index.template.html 静态提供。 */\n'
  + fallbackCss.join('\n') + '\n' + paletteCss.join('\n') + '\n';

const dataJs = '// ' + stamp + '\n'
  + '"use strict";\n'
  + 'window.CodeWFPalettes = ' + JSON.stringify({
    defaultKey: data.default,
    ruleVersion: data.ruleVersion,
    source: 'CodeWF.Avalonia ' + version,
    list: paletteEntries,
    alias: legacyAliases
  }, null, 2) + ';\n';

if (issues.length) {
  console.error('对比度校验未通过（规则 §5）：');
  issues.forEach(message => console.error('  ✗ ' + message));
  if (!args.includes('--force')) { console.error('已阻止落盘；调整附录 A 取值或加 --force 强制生成。'); process.exit(1); }
  process.exitCode = 1;
} else {
  console.log('对比度校验通过：' + Object.keys(data.palettes).length + ' 套 × 明暗。');
}

if (flag('--check') >= 0) { console.log('--check：只校验，未写文件。'); process.exit(process.exitCode || 0); }

function write(file, value) {
  const target = path.join(root, file);
  if (!fs.existsSync(target) || fs.readFileSync(target, 'utf8') !== value) {
    fs.writeFileSync(target, value, 'utf8');
    console.log('已生成 ' + file);
  } else {
    console.log('未变化 ' + file);
  }
}
write('assets/palette-tokens.css', tokensCss);
write('assets/palette-data.js', dataJs);
console.log('默认配色: ' + data.default + ' · 版本戳: CodeWF.Avalonia ' + version);
