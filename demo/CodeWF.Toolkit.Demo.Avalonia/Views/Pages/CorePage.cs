using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.FileExtensions;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>Core 页：扩展方法（可实时试用）+ GitHub Release 更新检查。</summary>
public sealed class CorePage : DemoPageBase
{
    private const string XamlSnippet = """
        <!-- Core 页本身是代码构建的演示页；这里演示的是演示台对 Toolkit 的引用方式 -->
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Core" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Core.Extensions;
        using CodeWF.Toolkit.Core.FileExtensions;
        using CodeWF.Toolkit.Core.UpdateChecking;

        // 字符串 / 数值 / 时间扩展
        var text = "CodeWF.Toolkit 是 .NET 扩展集合";
        var cut = text.Length > 18 ? text[..18] + "…" : text;      // 截断
        var size = ((double)3_221_225_472).FormatBytes();           // 3.00 GB
        var money = 1234.ToChineseNumber();                         // 一千二百三十四

        // 正则快捷判断（* → 通配，? → 单字符，# → 数字，@ → 字母）
        var matched = "CodeWF.Toolkit".IsMatch("CodeWF*");

        // 文件编码探测与安全读写（带重试）
        var encoding = FileHelper.GetFileEncodeType("app.log");
        await FileHelper.SafeWriteAllTextAsync("app.log", "hello", encoding);

        // 应用数据目录：%LOCALAPPDATA%\CodeWF.Toolkit，旧位置自动一次性迁移
        var dataDir = AppDataMigrator.ResolveAndMigrate("CodeWF.Toolkit", "config.json", AppContext.BaseDirectory);

        // GitHub Release 更新检查：网页端点优先（不占 API 配额），命中缓存 tag 时连资产请求都省掉
        var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
        var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
        if (!result.Succeeded) { /* result.Error */ }
        else if (result.Update is null) { /* 已是最新 */ }
        else { /* result.Update.Version / Tag / Notes / AssetUrl */ }
        """;

    private readonly TextBox _input;
    private readonly SelectableTextBlock _output;
    private readonly TextBox _updateRepo;
    private readonly TextBox _updateCurrent;
    private readonly ToggleSwitch _updateEtag;
    private readonly SelectableTextBlock _updateOutput;
    private readonly Button _updateButton;

    private PropertyItem? _truncateItem;
    private int _truncateLength = 18;

    public CorePage()
    {
        _input = Ui.Input("CodeWF.Toolkit 是 .NET 扩展集合");

        _output = Ui.Code("点击上方按钮查看结果", output: true);

        var tools = Ui.Card("字符串 / 数值扩展（真实调用）",
            _input,
            Ui.Row(
                Ui.Button("截断 18", RunTruncate),
                Ui.Button("转标识符", RunSlug),
                Ui.Button("字节单位", RunBytes),
                Ui.Button("正则判断", RunMatch)),
            _output,
            Ui.Hint("截断长度取右侧属性面板的“截断长度”；字节单位为 DoubleExtensions.FormatBytes。"),
            Ui.Code("""
                StringExtensions   正则快捷判断（IsMatch）、Join
                DoubleExtensions   字节 → 可读单位 FormatBytes
                NumberExtension    数字 → 中文数字 / 中文金额
                EnumExtension      EnumDescriptionAttribute 取描述
                """));

        _updateRepo = Ui.Input("dotnet9/CodeWF.Toolkit");
        _updateCurrent = Ui.Input("1.0.0.0");
        _updateEtag = Ui.Switch(true);
        _updateOutput = Ui.Code("尚未检查", output: true);
        _updateButton = Ui.Button("检查更新", async () => await CheckUpdateAsync(), primary: true);

        var update = Ui.Card("更新检查 UpdateChecker",
            Ui.Field("仓库", _updateRepo),
            Ui.Field("当前版本", _updateCurrent),
            Ui.Row(_updateButton, _updateEtag, Ui.Key("启用 ETag 缓存")),
            _updateOutput,
            Ui.Hint("默认路径不碰 api.github.com 配额：先走 releases/latest 的 302 落点，tag 与缓存一致时直接用缓存做版本比较；网页端点不可用才退回 API（ETag 条件请求命中 304 时不计配额）。"),
            Ui.Code("""
                var checker = new UpdateChecker("dotnet9", "CodeWF.Toolkit");
                var result = await checker.CheckAsync(new Version(1, 0, 0, 0));
                """));

        var data = Ui.Card("应用数据目录与编码探测",
            Ui.Code("""
                // %LOCALAPPDATA%\CodeWF.Toolkit（Linux/macOS 取各自标准值）
                var dataDir = AppDataMigrator.ResolveAndMigrate(
                    "CodeWF.Toolkit", "config.json", AppContext.BaseDirectory);
                // 旧 %APPDATA% 目录存在、新位置没有数据时一次性搬运，旧目录保留作备份
                """),
            Ui.Code("""
                FileHelper.GetFileEncodeType      编码探测（BOM / 无 BOM）
                FileHelper.SafeWriteAllTextAsync  带重试的安全读写
                FileHelper.OpenFolderAndSelectFile 打开目录并选中文件
                AppDataMigrator.ResolveAndMigrate 数据目录 + 一次性迁移
                IpHelper.GetAllIpV4               本机 IPv4 枚举
                """, output: true));

        Content = Ui.Page(
            Ui.Heading("Core：扩展方法与基础设施"),
            Ui.Sub("零第三方依赖。左侧是可实时试用的扩展方法与更新检查，右侧属性面板可调截断长度与检查选项。"),
            Ui.TwoColumns(tools, update),
            data);

        _updateRepo.Width = 180;
        _updateCurrent.Width = 120;
    }

    public override string TitleKey => DemoKeys.ItemCore;

    public override string DescriptionKey => DemoKeys.DescCore;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        _truncateItem = DemoProperty.Number("core.truncate", "截断长度", 18, 4, 200, 1, "字符", "字符串截断演示使用的长度。");

        return DemoProperty.Finish(
            DemoProperty.Category("core.ext", "扩展方法", "Core 包的扩展演示参数。",
                DemoProperty.ReadOnly("core.namespace", "命名空间", "CodeWF.Toolkit.Core.Extensions"),
                _truncateItem,
                DemoProperty.Bool("core.regex", "启用正则扩展", true, "IsMatch 的通配式正则转换。")),
            DemoProperty.Category("core.update", "更新检查", "GitHub Release 更新检查选项。",
                DemoProperty.Bool("core.etag", "ETag 增量检查", true, "命中 304 时不计入 API 配额。"),
                DemoProperty.ReadOnly("core.endpoint", "检查端点", "网页端点优先，失败退回 API")));
    }

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (change.Item.Id == "core.truncate")
        {
            _truncateLength = (int)DemoProperty.Num(change, _truncateLength);
        }
        else if (change.Item.Id == "core.etag")
        {
            _updateEtag.IsChecked = DemoProperty.Flag(change);
        }
    }

    private void RunTruncate()
    {
        var text = _input.Text ?? string.Empty;
        var cut = text.Length > _truncateLength ? text[.._truncateLength] + "…" : text;
        _output.Text = $"原文：{text}\n截断 {_truncateLength}：{cut}";
        Log($"Core 扩展演示：截断 {_truncateLength} 字符");
    }

    private void RunSlug()
    {
        var text = _input.Text ?? string.Empty;
        var slug = System.Text.RegularExpressions.Regex.Replace(text, "[^A-Za-z0-9]+", "_").ToLowerInvariant().Trim('_');
        _output.Text = $"正则替换非字母数字并转小写：\n{slug}";
        Log("Core 扩展演示：转标识符");
    }

    private void RunBytes()
    {
        double[] values = [1024, 1_048_576, 3_221_225_472];
        var lines = values.Select(v => $"{v,13:N0} B  →  {v.FormatBytes()}");
        _output.Text = "DoubleExtensions.FormatBytes：\n" + string.Join('\n', lines);
        Log("Core 扩展演示：字节单位格式化");
    }

    private void RunMatch()
    {
        var text = _input.Text ?? string.Empty;
        var results = new (string Pattern, bool Matched)[]
        {
            ("CodeWF*", text.IsMatch("CodeWF*")),
            ("*Toolkit", text.IsMatch("*Toolkit")),
            ("*扩展*", text.IsMatch("*扩展*")),
            ("Toolkit", text.IsMatch("Toolkit")),
        };
        _output.Text = "RegexExtensions.IsMatch（* → 通配，? → 单字符，# → 数字，@ → 字母）：\n"
                       + string.Join('\n', results.Select(r => $"{r.Pattern,-12} → {r.Matched}"));
        Log("Core 扩展演示：正则判断");
    }

    private async Task CheckUpdateAsync()
    {
        var repo = (_updateRepo.Text ?? string.Empty).Trim().Trim('/');
        var parts = repo.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            _updateOutput.Text = "仓库格式应为 owner/repo。";
            return;
        }

        if (!Version.TryParse(_updateCurrent.Text, out var current))
        {
            current = new Version(1, 0, 0, 0);
        }

        _updateButton.IsEnabled = false;
        _updateOutput.Text = $"GET https://github.com/{parts[0]}/{parts[1]}/releases/latest\n"
                             + (_updateEtag.IsChecked == true ? "（ETag 增量：命中缓存则不消耗 API 配额）\n" : string.Empty)
                             + "检查中…";
        Log($"更新检查开始：{parts[0]}/{parts[1]}（当前 {current}）");

        try
        {
            var stateFile = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "update-state.json");
            var checker = new UpdateChecker(new HttpClient(), parts[0], parts[1],
                log: message => Log(message), stateFile: stateFile);
            var result = await checker.CheckAsync(current);

            if (!result.Succeeded)
            {
                _updateOutput.Text += $"\n\nUpdateCheckResult.Succeeded = false\nError = {result.Error}";
                Log($"更新检查失败：{result.Error}", "warn");
            }
            else if (result.Update is null)
            {
                _updateOutput.Text += $"\n\nUpdateCheckResult.Succeeded = true\nUpdate = null → 已是最新版本 {current}";
                Log("更新检查完成：已是最新版本");
            }
            else
            {
                var update = result.Update;
                _updateOutput.Text += "\n\nUpdateCheckResult.Succeeded = true\n"
                                      + $"Update = {{ Version: {update.Version}, Tag: {update.Tag} }}\n"
                                      + $"Asset: {update.AssetName ?? "(无)"}\n"
                                      + $"→ 对比当前 {current}：发现新版本 {update.Tag}";
                Log($"更新检查完成：发现新版本 {update.Tag}", "warn");
            }
        }
        finally
        {
            _updateButton.IsEnabled = true;
        }
    }

    private static void Log(string message, string level = "info")
    {
        if (level == "warn")
        {
            CodeWF.Toolkit.Logging.Logger.Warn(message);
        }
        else
        {
            CodeWF.Toolkit.Logging.Logger.Info(message);
        }
    }
}
