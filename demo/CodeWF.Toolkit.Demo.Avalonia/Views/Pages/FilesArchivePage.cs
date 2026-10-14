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

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Content.FilesArchive.Source.CSharp");

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

        _zipOutput = Ui.Code("@Demo.Content.FilesArchive.Content.ReadyOutputTEMPCodeWFToolkitDemoArchive", output: true);

        var archive = Ui.Card("@Demo.Content.FilesArchive.archive.ArchiveCompressionSharpCompress",
            Ui.Row(
                Ui.Button("@Demo.Content.FilesArchive.archive.PrepareSampleFiles", PrepareSamples),
                Ui.Button("Compress → archive.7z", Compress, primary: true),
                Ui.Button("Decompress → restored", Decompress),
                Ui.Button("@Demo.Content.FilesArchive.archive.ClearOutput", () => _zipOutput.Text = string.Empty)),
            _zipOutput,
            Ui.Code("""
                public interface ISevenZipCompressor
                {
                    void Zip(IEnumerable<string> files, string zipFile, string rootdir = ");
                    void Zip(string dir, string zipFile, string rootdir = ");
                    void Decompress(string compressedFile, string dir = "", bool ignoreEmptyDir = true);
                }
                """),
            Ui.Hint("@Demo.Content.FilesArchive.archive.SharpCompressZIPCompressionWithLZMAAndUTF8"));

        _configKey = Ui.Input("App.Language", 150);
        _configValue = Ui.Input("zh-CN");
        _configTable = Ui.Code(DescribeConfig(), output: true);

        var config = Ui.Card("App.config appSettings",
            _configTable,
            Ui.Row(Ui.Key("Key"), _configKey, Ui.Key("Value"), _configValue, Ui.Button("@Demo.Content.FilesArchive.config.Write", WriteConfig, primary: true)),
            Ui.Code("""
                AppConfigHelper.Set("App.Language", "zh-CN");
                var ok = AppConfigHelper.TryGet<string>("App.Language", out var lang);
                AppConfigHelper.SetOrAdd("Log.MinimumLevel", "Information");
                AppConfigHelper.GetOrAdd("Log.MinimumLevel", "Information");
                """),
            Ui.Hint("@Demo.Content.FilesArchive.config.SetGetOrAddUseLightweightXMLWritesSuitableFor"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Content.FilesArchive.Content.ArchivesAndAppConfig"),
            Ui.Sub("@Demo.Content.FilesArchive.Content.SevenZipCompressorUsesSharpCompressAndAppConfigHelperHandlesAppSettingsThe"),
            archive,
            config);
    }

    public override string TitleKey => DemoKeys.FilesArchiveExampleTitle;

    public override string DescriptionKey => DemoKeys.DescFilesArchive;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("sz.opt", L.Instance.GetResource("Demo.Content.FilesArchive.Properties.CompressionOptions"), L.Instance.GetResource("Demo.Content.FilesArchive.Properties.DemoParametersOutputFilenamesAreFixed"),
            DemoProperty.ReadOnly("sz.algorithm", L.Instance.GetResource("Demo.Content.FilesArchive.Properties.CompressionAlgorithm"), "LZMA（ZipWriterOptions）"),
            DemoProperty.Bool("sz.utf8", L.Instance.GetResource("Demo.Content.FilesArchive.Properties.UTF8EntryNames"), true, "ArchiveEncoding.Default = UTF8。"),
            DemoProperty.Number("sz.samples", L.Instance.GetResource("Demo.Content.FilesArchive.Properties.SampleFileCount"), 3, 1, 20, 1, L.Instance.GetResource("Demo.Content.FilesArchive.Properties.Items"), L.Instance.GetResource("Demo.Content.FilesArchive.Properties.NumberOfSampleFilesToCreate"))),
        DemoProperty.Category("cfg.opt", L.Instance.GetResource("Demo.Content.FilesArchive.Properties.ConfigurationEntry"), L.Instance.GetResource("Demo.Content.FilesArchive.Properties.AppConfigReadWriteDemo"),
            DemoProperty.ReadOnly("cfg.path", L.Instance.GetResource("Demo.Content.FilesArchive.Properties.FileLocation"), Path.Combine("%TEMP%", "CodeWFToolkitDemo", "archive", "App.config")),
            DemoProperty.Bool("cfg.bom", "UTF-8 BOM", false, L.Instance.GetResource("Demo.Content.FilesArchive.Properties.IncludeABOMWhenWritingAppSettings"))));

    private void PrepareSamples()
    {
        Directory.CreateDirectory(Path.Combine(_sandboxDirectory, "docs", L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.ExportResults")));
        File.WriteAllText(Path.Combine(_sandboxDirectory, "docs", L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.ReadmeMd")), L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.CodeWFToolkitSampleDocument"));
        File.WriteAllText(Path.Combine(_sandboxDirectory, "docs", L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.ExportResults"), "students.csv"), L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.NameAgeCityAlice20Chengdu"));
        File.WriteAllText(Path.Combine(_sandboxDirectory, L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.PrototypeHtml")), L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.ToolkitDesignReference"));
        Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.SampleFilesPreparedFormat"), _sandboxDirectory));
        Log(L.Instance.GetResource("Demo.Content.FilesArchive.PrepareSamples.ArchiveDemoSampleFilesPrepared"));
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
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.Compress.CompleteArchive7zBytesEntriesLZMAUTF8Format"), size, entryCount));
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.Compress.ArchiveCompleteEntriesBytesFormat"), entryCount, size));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Content.FilesArchive.Compress.CompressionFailed") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Content.FilesArchive.Compress.ArchiveCompressionFailed"), ex);
        }
    }

    private void Decompress()
    {
        var zipFile = Path.Combine(_sandboxDirectory, "archive.7z");
        if (!File.Exists(zipFile))
        {
            Append(L.Instance.GetResource("Demo.Content.FilesArchive.Decompress.RunCompressFirst"));
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
            Append(L.Instance.GetResource("Demo.Content.FilesArchive.Decompress.RestoredFiles") + string.Join("\n  ", files));
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.Decompress.ArchiveExtractedFilesRestoredFormat"), files.Count));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Content.FilesArchive.Decompress.ExtractionFailed") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Content.FilesArchive.Decompress.ArchiveExtractionFailed"), ex);
        }
    }

    private void WriteConfig()
    {
        var key = (_configKey.Text ?? string.Empty).Trim();
        if (key.Length == 0)
        {
            Append(L.Instance.GetResource("Demo.Content.FilesArchive.WriteConfig.EnterAKey"));
            return;
        }

        var value = _configValue.Text ?? string.Empty;
        try
        {
            Directory.CreateDirectory(_sandboxDirectory);
            AppConfigHelper.Set(_configPath, key, value);
            var found = AppConfigHelper.TryGet<string>(_configPath, key, out var read);
            Append($"AppConfigHelper.Set(\"{_configPath}\", \"{key}\", \"{value}\")");
            Append(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.WriteConfig.TryGetReadFormat"), found, key, read));
            _configTable.Text = DescribeConfig();
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.WriteConfig.AppConfigWrittenFormat"), key, value));
        }
        catch (Exception ex)
        {
            Append(L.Instance.GetResource("Demo.Content.FilesArchive.WriteConfig.WriteFailed") + ex.Message);
            Logger.Error(L.Instance.GetResource("Demo.Content.FilesArchive.WriteConfig.AppConfigWriteFailed"), ex);
        }
    }

    private string DescribeConfig()
    {
        if (!File.Exists(_configPath))
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.FilesArchive.DescribeConfig.NotCreatedYetFormat"), _configPath);
        }

        var lines = new List<string> { _configPath, string.Empty };
        foreach (var key in new[] { "App.Language", "App.Theme", "Log.MinimumLevel" })
        {
            lines.Add($"{key,-20} = {(AppConfigHelper.TryGet<string>(_configPath, key, out var value) ? value : L.Instance.GetResource("Demo.Content.FilesArchive.DescribeConfig.NotSet"))}");
        }

        return string.Join('\n', lines);
    }

    private void Append(string line)
    {
        var current = _zipOutput.Text;
        _zipOutput.Text = string.IsNullOrEmpty(current) || current.StartsWith(L.Instance.GetResource("Demo.Content.FilesArchive.Append.Ready"), StringComparison.Ordinal)
            ? line
            : current + "\n" + line;
    }

    private static void Log(string message) => Logger.Info(message);
}
