using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Core.FileExtensions;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
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

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.FilesArchivePage.CSharpSnippet001");

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

        _zipOutput = Ui.Code("@Demo.Text.FilesArchivePage.Content002", output: true);

        var archive = Ui.Card("@Demo.Text.FilesArchivePage.archive003",
            Ui.Row(
                Ui.Button("@Demo.Text.FilesArchivePage.archive004", PrepareSamples),
                Ui.Button("Compress → archive.7z", Compress, primary: true),
                Ui.Button("Decompress → restored", Decompress),
                Ui.Button("@Demo.Text.FilesArchivePage.archive005", () => _zipOutput.Text = string.Empty)),
            _zipOutput,
            Ui.Code("""
                public interface ISevenZipCompressor
                {
                    void Zip(IEnumerable<string> files, string zipFile, string rootdir = ");
                    void Zip(string dir, string zipFile, string rootdir = ");
                    void Decompress(string compressedFile, string dir = "", bool ignoreEmptyDir = true);
                }
                """),
            Ui.Hint("@Demo.Text.FilesArchivePage.archive006"));

        _configKey = Ui.Input("App.Language", 150);
        _configValue = Ui.Input("zh-CN");
        _configTable = Ui.Code(DescribeConfig(), output: true);

        var config = Ui.Card("App.config appSettings",
            _configTable,
            Ui.Row(Ui.Key("Key"), _configKey, Ui.Key("Value"), _configValue, Ui.Button("@Demo.Text.FilesArchivePage.config007", WriteConfig, primary: true)),
            Ui.Code("""
                AppConfigHelper.Set("App.Language", "zh-CN");
                var ok = AppConfigHelper.TryGet<string>("App.Language", out var lang);
                AppConfigHelper.SetOrAdd("Log.MinimumLevel", "Information");
                AppConfigHelper.GetOrAdd("Log.MinimumLevel", "Information");
                """),
            Ui.Hint("@Demo.Text.FilesArchivePage.config008"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.FilesArchivePage.Content009"),
            Ui.Sub("@Demo.Text.FilesArchivePage.Content010"),
            archive,
            config);
    }

    public override string TitleKey => DemoKeys.ItemFilesArchive;

    public override string DescriptionKey => DemoKeys.DescFilesArchive;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("sz.opt", L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties011"), L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties012"),
            DemoProperty.ReadOnly("sz.algorithm", L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties013"), "LZMA（ZipWriterOptions）"),
            DemoProperty.Bool("sz.utf8", L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties014"), true, "ArchiveEncoding.Default = UTF8。"),
            DemoProperty.Number("sz.samples", L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties015"), 3, 1, 20, 1, L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties016"), L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties017"))),
        DemoProperty.Category("cfg.opt", L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties018"), L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties019"),
            DemoProperty.ReadOnly("cfg.path", L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties020"), Path.Combine("%TEMP%", "CodeWFToolkitDemo", "archive", "App.config")),
            DemoProperty.Bool("cfg.bom", "UTF-8 BOM", false, L.Instance.GetResource("Demo.Text.FilesArchivePage.BuildProperties021"))));

    private void PrepareSamples()
    {
        Directory.CreateDirectory(Path.Combine(_sandboxDirectory, "docs", L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples022")));
        File.WriteAllText(Path.Combine(_sandboxDirectory, "docs", L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples023")), L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples024"));
        File.WriteAllText(Path.Combine(_sandboxDirectory, "docs", L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples025"), "students.csv"), L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples026"));
        File.WriteAllText(Path.Combine(_sandboxDirectory, L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples027")), L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples028"));
        Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples029"), _sandboxDirectory));
        Log(L.Instance.GetResource("Demo.Text.FilesArchivePage.PrepareSamples030"));
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
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.Compress031"), size, entryCount));
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.Compress032"), entryCount, size));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Text.FilesArchivePage.Compress033") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Text.FilesArchivePage.Compress034"), ex);
        }
    }

    private void Decompress()
    {
        var zipFile = Path.Combine(_sandboxDirectory, "archive.7z");
        if (!File.Exists(zipFile))
        {
            Append(L.Instance.GetResource("Demo.Text.FilesArchivePage.Decompress035"));
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
            Append(L.Instance.GetResource("Demo.Text.FilesArchivePage.Decompress036") + string.Join("\n  ", files));
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.Decompress037"), files.Count));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Text.FilesArchivePage.Decompress038") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Text.FilesArchivePage.Decompress039"), ex);
        }
    }

    private void WriteConfig()
    {
        var key = (_configKey.Text ?? string.Empty).Trim();
        if (key.Length == 0)
        {
            Append(L.Instance.GetResource("Demo.Text.FilesArchivePage.WriteConfig040"));
            return;
        }

        var value = _configValue.Text ?? string.Empty;
        try
        {
            Directory.CreateDirectory(_sandboxDirectory);
            AppConfigHelper.Set(_configPath, key, value);
            var found = AppConfigHelper.TryGet<string>(_configPath, key, out var read);
            Append($"AppConfigHelper.Set(\"{_configPath}\", \"{key}\", \"{value}\")");
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.WriteConfig041"), found, key, read));
            _configTable.Text = DescribeConfig();
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.WriteConfig042"), key, value));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Text.FilesArchivePage.WriteConfig043") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Text.FilesArchivePage.WriteConfig044"), ex);
        }
    }

    private string DescribeConfig()
    {
        if (!File.Exists(_configPath))
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.FilesArchivePage.DescribeConfig045"), _configPath);
        }

        var lines = new List<string> { _configPath, string.Empty };
        foreach (var key in new[] { "App.Language", "App.Theme", "Log.MinimumLevel" })
        {
            lines.Add($"{key,-20} = {(AppConfigHelper.TryGet<string>(_configPath, key, out var value) ? value : L.Instance.GetResource("Demo.Text.FilesArchivePage.DescribeConfig046"))}");
        }

        return string.Join('\n', lines);
    }

    private void Append(string line)
    {
        var current = _zipOutput.Text;
        _zipOutput.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource("Demo.Text.FilesArchivePage.Append047"), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }

    private static void Log(string message) => Logger.Info(message);
}
