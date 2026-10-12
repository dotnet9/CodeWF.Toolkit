using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>欢迎页：能力域总览 + 快速开始（安装命令）。</summary>
public sealed class WelcomePage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 演示台外壳：CodeWFWindow 标题栏 + Ribbon 页签 + Dock 工作区 + PropertyGrid + LogView -->
        <codewf:ACSemiTheme />
        <codewf:DockSemiTheme />
        <codewf:CodeWFLogTheme />

        <Application.RequestedThemeVariant>Default</Application.RequestedThemeVariant>
        <Application.Styles>
          <semi:SemiTheme Locale="zh-CN" />
        </Application.Styles>
        """;

    public const string CSharpSnippet = """
        // 启动引导：注册 Lang 资源（Json 后端，输出目录 I18n\）并初始化日志核心
        I18nManager.Instance.Register(
            new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n") },
            new CultureInfo("zh-CN"));

        Logger.TryInitialize(new LoggerOptions
        {
            File = new FileLogOptions { DirectoryPath = logDirectory },
        });

        // 6 色板 × 浅/深：切换应用级主题变体，ACSemiTheme / DockSemiTheme 已映射全部变体资源
        Application.Current.RequestedThemeVariant = SemiTheme.Aquatic;
        Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

        // 运行时切换语言，全部 {codewf:I18n} 文案即时刷新
        I18nManager.Instance.Culture = new CultureInfo("en-US");
        """;

    /// <summary>能力域总览（能力 / 包 / 要点），等宽对齐呈现。</summary>
    private const string CapabilityTable = """
        能力      包                              要点
        --------  ------------------------------  ------------------------------------------------------------
        Core      CodeWF.Toolkit.Core             字符串/日期/数值/枚举扩展、FileHelper 编码探测、
                                                AppDataMigrator、GitHub Release 更新检查（ETag 增量）
        EventBus  EventBus(+Prism/AspNetCore/     发布订阅、Command/Query、[EventHandler] 扫描；
                  Socket)                       TCP 形态带心跳、断线重连与离线补发
        Files     CodeWF.Toolkit.Files           Json/Yaml/Csv/Excel 读写、7z 压缩、App.config、
                                                带重试的安全文件读写
        Image     CodeWF.Toolkit.Image           Magick.NET 转换/缩放/水印、多尺寸 ico 合成与拆分、
                                                ZXing 二维码
        Logging   Logging(+.Microsoft)           静态 Logger、控制台 + 文件轮转、UI 事件流、
                                                Microsoft.Extensions.Logging 提供程序
        Net       NetWeaver / NetWrapper         特性驱动的二进制序列化、TCP/UDP 封装、命令分发、
                                                远程文件系统与断点续传
        """;

    private const string QuickStart = """
        dotnet add package CodeWF.Toolkit.Core
        dotnet add package CodeWF.Toolkit.EventBus
        dotnet add package CodeWF.Toolkit.Files
        dotnet add package CodeWF.Toolkit.Image
        dotnet add package CodeWF.Toolkit.Logging
        dotnet add package CodeWF.Toolkit.NetWrapper
        """;

    public WelcomePage()
    {
        Content = Ui.Page(
            Ui.Heading("CodeWF.Toolkit 演示台"),
            Ui.Sub(".NET 原生功能扩展与应用基础设施：11 个包 / 6 大能力域，覆盖核心扩展、事件总线、文件、图像、日志与网络通信。所有演示页都调用仓库里的真实 API。"),
            Ui.Card("能力域", Ui.Code(CapabilityTable),
                Ui.Hint("左侧导航树与 Ribbon 页签均可切换演示页；双击页签可折叠命令区。")),
            Ui.Card("快速开始", Ui.Code(QuickStart)),
            Ui.Hint("https://github.com/dotnet9/CodeWF.Toolkit"));
    }

    public override string TitleKey => DemoKeys.ItemWelcome;

    public override string DescriptionKey => DemoKeys.DescWelcome;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("tk.info", "程序集信息", "来自 AssemblyExtensions 的程序集元数据读取。",
            DemoProperty.ReadOnly("tk.version", "版本", DemoInfo.Version),
            DemoProperty.ReadOnly("tk.tfm", "目标框架", "net10.0"),
            DemoProperty.ReadOnly("tk.packages", "包数量", "11"),
            DemoProperty.ReadOnly("tk.repo", "仓库", "dotnet9/CodeWF.Toolkit")),
        DemoProperty.Category("tk.state", "演示台状态", "Shell 运行状态（只读占位，与原型一致）。",
            DemoProperty.ReadOnly("tk.state.shell", "外壳", "CodeWFWindow + Ribbon + Dock"),
            DemoProperty.ReadOnly("tk.state.palette", "色板", "6 套（默认/水生/沙漠/黄昏/夜空/高对比）")));
}

/// <summary>演示台自身的元信息（程序集版本等）。</summary>
internal static class DemoInfo
{
    public static string Version
    {
        get
        {
            var version = typeof(DemoInfo).Assembly.GetName().Version;
            return version is null ? "1.0.0" : version.ToString(3);
        }
    }
}
