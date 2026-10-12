using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Core.FileExtensions;using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Files.Helpers;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>压缩与配置页：7z（SharpCompress）真实打包/解包往返 + App.config appSettings 读写。</summary>
public sealed class FilesArchivePage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Files" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.Core.FileExtensions;
        using CodeWF.Toolkit.Files.Helpers;

        // 7z：文件 / 文件夹 / 远程 URL（http 开头自动下载后打入）
        var compressor = new SevenZipCompressor(httpClient);
        compressor.Zip("docs/导出结果", "archive.7z", rootdir: "docs");
        compressor.Decompress("archive.7z", "out/restored");

        // App.config：传统 appSettings 读写（也有 Configuration 对象的扩展方法重载）
        AppConfigHelper.Set("App.Language", "zh-CN");
        var ok = AppConfigHelper.TryGet<string>("App.Language", out var lang);
        AppConfigHelper.SetOrAdd("Log.MinimumLevel", "Information");
        var level = AppConfigHelper.GetOrAdd("Log.MinimumLevel", "Information");
        """;

    private readonly SelectableTextBlock _zipOutput;
    private readonly SelectableTextBlock _configTable;
    private readonly TextBox _configKey;
    private readonly TextBox _configValue;
    private readonly string _configPath;
    private readonly string _sandboxDirectory;

    public FilesArchivePage()
    {
        _sandboxDirectory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "archive");
        _configPath = Path.Combine(_sandboxDirectory, "App.config");

        _zipOutput = Ui.Code("等待操作（所有文件写入 %TEMP%\\CodeWFToolkitDemo\\archive）", output: true);

        var archive = Ui.Card("7z 压缩（SharpCompress）",
            Ui.Row(
                Ui.Button("准备样本文件", PrepareSamples),
                Ui.Button("Compress → archive.7z", Compress, primary: true),
                Ui.Button("Decompress → restored", Decompress),
                Ui.Button("清空输出", () => _zipOutput.Text = string.Empty)),
            _zipOutput,
            Ui.Code("""
                public interface ISevenZipCompressor
                {
                    void Zip(IEnumerable<string> files, string zipFile, string rootdir = ");
                    void Zip(string dir, string zipFile, string rootdir = ");
                    void Decompress(string compressedFile, string dir = "", bool ignoreEmptyDir = true);
                }
                """),
            Ui.Hint("实现基于 SharpCompress，LZMA 压缩、UTF-8 条目名；zip 内条目按公共根目录计算相对路径，跨盘符会直接拒绝。"));

        _configKey = Ui.Input("App.Language", 150);
        _configValue = Ui.Input("zh-CN");
        _configTable = Ui.Code(DescribeConfig(), output: true);

        var config = Ui.Card("App.config appSettings",
            _configTable,
            Ui.Row(Ui.Key("Key"), _configKey, Ui.Key("Value"), _configValue, Ui.Button("写入", WriteConfig, primary: true)),
            Ui.Code("""
                AppConfigHelper.Set("App.Language", "zh-CN");
                var ok = AppConfigHelper.TryGet<string>("App.Language", out var lang);
                AppConfigHelper.SetOrAdd("Log.MinimumLevel", "Information");
                AppConfigHelper.GetOrAdd("Log.MinimumLevel", "Information");
                """),
            Ui.Hint("Set / GetOrAdd 使用轻量 XML 写入（裁剪友好）；Configuration 对象上的重载标注了 RequiresUnreferencedCode。"));

        Content = Ui.Page(
            Ui.Heading("7z 压缩与 App.config"),
            Ui.Sub("SevenZipCompressor 基于 SharpCompress，AppConfigHelper 读写传统 appSettings，下方都是真实写盘操作。"),
            archive,
            config);
    }

    public override string TitleKey => DemoKeys.ItemFilesArchive;

    public override string DescriptionKey => DemoKeys.DescFilesArchive;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("sz.opt", "压缩选项", "演示参数（写出文件名固定）。",
            DemoProperty.ReadOnly("sz.algorithm", "压缩算法", "LZMA（ZipWriterOptions）"),
            DemoProperty.Bool("sz.utf8", "UTF-8 条目名", true, "ArchiveEncoding.Default = UTF8。"),
            DemoProperty.Number("sz.samples", "样本文件数", 3, 1, 20, 1, "个", "准备样本文件时生成的数量。")),
        DemoProperty.Category("cfg.opt", "配置项", "App.config 读写演示。",
            DemoProperty.ReadOnly("cfg.path", "文件位置", Path.Combine("%TEMP%", "CodeWFToolkitDemo", "archive", "App.config")),
            DemoProperty.Bool("cfg.bom", "UTF-8 BOM", false, "写入 appSettings 时是否带 BOM。")));

    private void PrepareSamples()
    {
        Directory.CreateDirectory(Path.Combine(_sandboxDirectory, "docs", "导出结果"));
        File.WriteAllText(Path.Combine(_sandboxDirectory, "docs", "说明文档.md"), "# CodeWF.Toolkit 演示文档\n");
        File.WriteAllText(Path.Combine(_sandboxDirectory, "docs", "导出结果", "students.csv"), "Name,Age,City\n张三,20,成都\n");
        File.WriteAllText(Path.Combine(_sandboxDirectory, "原型.html"), "<!-- design/CodeWF.Toolkit.Demo原型.html -->\n");
        Append($"已准备样本：{_sandboxDirectory}");
        Log("7z 演示：样本文件已准备");
    }

    private void Compress()
    {
        var source = Path.Combine(_sandboxDirectory, "docs");
        if (!Directory.Exists(source))
        {
            PrepareSamples();
        }

        var zipFile = Path.Combine(_sandboxDirectory, "archive.7z");
        if (File.Exists(zipFile))
        {
            File.Delete(zipFile);
        }

        try
        {
            var entryCount = Directory.GetFiles(source, "*", SearchOption.AllDirectories).Length;
            var compressor = new SevenZipCompressor(new HttpClient());
            compressor.Zip(source, zipFile, "docs");

            var size = new FileInfo(zipFile).Length;
            Append($"SevenZipCompressor.Zip(\"{source}\", \"archive.7z\", rootdir: \"docs\")");
            Append($"完成：archive.7z（{size} 字节，{entryCount} 个条目，LZMA / UTF-8）");
            Log($"7z 压缩完成：{entryCount} 个条目，{size} 字节");
        }
        catch (Exception ex)
        {
            Append("压缩失败：" + ex.Message);
            Logger.Error("7z 压缩失败", ex);
        }
    }

    private void Decompress()
    {
        var zipFile = Path.Combine(_sandboxDirectory, "archive.7z");
        if (!File.Exists(zipFile))
        {
            Append("请先执行 Compress。");
            return;
        }

        var destination = Path.Combine(_sandboxDirectory, "restored");
        try
        {
            var compressor = new SevenZipCompressor(new HttpClient());
            compressor.Decompress(zipFile, destination);

            var files = Directory.GetFiles(destination, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(destination, path))
                .OrderBy(path => path)
                .ToList();
            Append($"SevenZipCompressor.Decompress(\"archive.7z\", \"{destination}\")");
            Append("还原文件：\n  " + string.Join("\n  ", files));
            Log($"7z 解压完成：还原 {files.Count} 个文件");
        }
        catch (Exception ex)
        {
            Append("解压失败：" + ex.Message);
            Logger.Error("7z 解压失败", ex);
        }
    }

    private void WriteConfig()
    {
        var key = (_configKey.Text ?? string.Empty).Trim();
        if (key.Length == 0)
        {
            Append("请填写 Key。");
            return;
        }

        var value = _configValue.Text ?? string.Empty;
        try
        {
            Directory.CreateDirectory(_sandboxDirectory);
            AppConfigHelper.Set(_configPath, key, value);
            var found = AppConfigHelper.TryGet<string>(_configPath, key, out var read);
            Append($"AppConfigHelper.Set(\"{_configPath}\", \"{key}\", \"{value}\")");
            Append($"TryGet → {found}，读回 {key} = {read}");
            _configTable.Text = DescribeConfig();
            Log($"App.config 写入：{key} = {value}");
        }
        catch (Exception ex)
        {
            Append("写入失败：" + ex.Message);
            Logger.Error("App.config 写入失败", ex);
        }
    }

    private string DescribeConfig()
    {
        if (!File.Exists(_configPath))
        {
            return $"（尚未创建）{_configPath}";
        }

        var lines = new List<string> { _configPath, string.Empty };
        foreach (var key in new[] { "App.Language", "App.Theme", "Log.MinimumLevel" })
        {
            lines.Add($"{key,-20} = {(AppConfigHelper.TryGet<string>(_configPath, key, out var value) ? value : "(未设置)")}");
        }

        return string.Join('\n', lines);
    }

    private void Append(string line)
    {
        var current = _zipOutput.Text;
        _zipOutput.Text = string.IsNullOrEmpty(current) || current.StartsWith("等待操作", StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }

    private static void Log(string message) => Logger.Info(message);
}
