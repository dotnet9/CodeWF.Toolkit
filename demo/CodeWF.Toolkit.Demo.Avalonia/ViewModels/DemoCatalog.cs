using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeWF.Toolkit.Demo.Avalonia.Views;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Demo.Avalonia.Views.Scenes;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels;

/// <summary>The prototype and native navigation share an embedded API catalog.</summary>
public static class DemoCatalog
{
    public static ObservableCollection<DemoGroupViewModel> CreateGroups()
    {
        using var stream = typeof(DemoCatalog).Assembly.GetManifestResourceStream("Demo.Catalog.json")
            ?? throw new InvalidOperationException("Demo catalog resource missing.");
        var groups = JsonSerializer.Deserialize(stream, DemoCatalogJsonContext.Default.CatalogGroupArray)
            ?? throw new InvalidOperationException("Demo catalog is empty.");
        return new(groups.Select(group =>
        {
            var items = group.Items.Select(entry => new DemoItemViewModel(entry.TitleKey,
                () => CreatePage(entry.TitleKey), entry.Api, entry.Package, entry.PrototypeId, entry.SectionKey));
            return new DemoGroupViewModel(group.TitleKey, group.TitleKey switch { "Demo.Group.Home" => "⌂", "Demo.Group.Extensions" => "⟨⟩", "Demo.Group.System" => "▣", "Demo.Group.Data" => "▦", "Demo.Group.Events" => "◉", "Demo.Group.Network" => "⇄", "Demo.Group.Logging" => "☰", _ => "▶" }, items);
        }));
    }

    private static DemoPageBase CreatePage(string key) => key switch
    {
        "Demo.Item.Welcome" => new WelcomePage(),
        "Demo.Item.About" => new AboutPage(),
        "Demo.Item.StringExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("StringExtensions")),
        "Demo.Item.RegexExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("RegexExtensions")),
        "Demo.Item.DoubleExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("DoubleExtensions")),
        "Demo.Item.NumberExtension" => new HelperDemoPage(HelperDemoCatalog.Create("NumberExtension")),
        "Demo.Item.DateTimeExtension" => new HelperDemoPage(HelperDemoCatalog.Create("DateTimeExtension")),
        "Demo.Item.TimeSpanExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("TimeSpanExtensions")),
        "Demo.Item.RandomExtension" => new HelperDemoPage(HelperDemoCatalog.Create("RandomExtension")),
        "Demo.Item.IConvertibleExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("IConvertibleExtensions")),
        "Demo.Item.ObjectExtension" => new HelperDemoPage(HelperDemoCatalog.Create("ObjectExtension")),
        "Demo.Item.LinqExtension" => new HelperDemoPage(HelperDemoCatalog.Create("LinqExtension")),
        "Demo.Item.EnumExtension" => new HelperDemoPage(HelperDemoCatalog.Create("EnumExtension")),
        "Demo.Item.AssemblyExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("AssemblyExtensions")),
        "Demo.Item.ValidationExtension" => new HelperDemoPage(HelperDemoCatalog.Create("ValidationExtension")),
        "Demo.Item.DateTimeConverter" => new HelperDemoPage(HelperDemoCatalog.Create("DateTimeConverter")),
        "Demo.Item.ActionHelper" => new HelperDemoPage(HelperDemoCatalog.Create("ActionHelper")),
        "Demo.Item.IpHelper" => new HelperDemoPage(HelperDemoCatalog.Create("IpHelper")),
        "Demo.Item.NativeLibraryHelper" => new HelperDemoPage(HelperDemoCatalog.Create("NativeLibraryHelper")),
        "Demo.Item.FileHelper" => new HelperDemoPage(HelperDemoCatalog.Create("FileHelper")),
        "Demo.Item.AppDataMigrator" => new HelperDemoPage(HelperDemoCatalog.Create("AppDataMigrator")),
        "Demo.Item.UpdateChecker" => new HelperDemoPage(HelperDemoCatalog.Create("UpdateChecker")),
        "Demo.Item.FileExtension" => new HelperDemoPage(HelperDemoCatalog.Create("FileExtension")),
        "Demo.Item.SevenZipCompressor" => new HelperDemoPage(HelperDemoCatalog.Create("SevenZipCompressor")),
        "Demo.Item.AppConfigHelper" => new HelperDemoPage(HelperDemoCatalog.Create("AppConfigHelper")),
        "Demo.Item.JsonExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("JsonExtensions")),
        "Demo.Item.YamlExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("YamlExtensions")),
        "Demo.Item.DataTableExtensions" => new DataTableDemoPage(),
        "Demo.Item.ImageIcon" => new ImageIconPage(),
        "Demo.Item.ImageQr" => new ImageQrPage(),
        "Demo.Item.Bus" => new EventBusPage(),
        "Demo.Item.PrismExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("PrismExtensions")),
        "Demo.Item.AspNetCoreExtensions" => new HelperDemoPage(HelperDemoCatalog.Create("AspNetCoreExtensions")),
        "Demo.Item.EventServer" => new TransportDemoPage("EventServer"),
        "Demo.Item.EventClient" => new TransportDemoPage("EventClient"),
        "Demo.Item.NetPacket" => new NetPacketPage(),
        "Demo.Item.TcpSocketServer" => new TransportDemoPage("TcpSocketServer"),
        "Demo.Item.TcpSocketClient" => new TransportDemoPage("TcpSocketClient"),
        "Demo.Item.UdpSocketServer" => new TransportDemoPage("UdpSocketServer"),
        "Demo.Item.UdpSocketClient" => new TransportDemoPage("UdpSocketClient"),
        "Demo.Item.NetHelper" => new HelperDemoPage(HelperDemoCatalog.Create("NetHelper")),
        "Demo.Item.TcpSocketClientFileSystemFeature" => new FileSystemDemoPage(false),
        "Demo.Item.TcpSocketServerFileSystemFeature" => new FileSystemDemoPage(true),
        "Demo.Item.LogCore" => new LoggingCorePage(),
        "Demo.Item.LogMicrosoft" => new LoggingMicrosoftPage(),
        "Demo.Item.LogEventFeed" => new HelperDemoPage(HelperDemoCatalog.Create("LogEventFeed")),
        "Demo.Item.CodeWFLogHealth" => new HelperDemoPage(HelperDemoCatalog.Create("CodeWFLogHealth")),
        "Demo.Item.RuntimeFlow" => new RuntimeFlowPage(),
        "Demo.Item.LabFlow" => new LabFlowPage(),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown catalog API.")
    };
}

internal sealed record CatalogGroup(string TitleKey, CatalogItem[] Items);
internal sealed record CatalogItem(string TitleKey, string Api, string Package, string PrototypeId, string SectionKey = "");

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CatalogGroup[]))]
internal partial class DemoCatalogJsonContext : JsonSerializerContext;
