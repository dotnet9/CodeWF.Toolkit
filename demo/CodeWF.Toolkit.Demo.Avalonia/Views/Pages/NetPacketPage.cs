using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWeaver;
using CodeWF.Toolkit.NetWeaver.Base;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>NetWeaver 页：特性驱动的二进制序列化，字段表可增删，序列化/反序列化都是真实往返。</summary>
public sealed class NetPacketPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.NetWeaver" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private static string CSharpSnippet => L.Instance.GetResource("Demo.Text.NetPacketPage.CSharpSnippet001");

    private readonly ObservableCollection<string> _fieldRows = [];
    private readonly ListBox _fieldList;
    private readonly TextBox _systemId;
    private readonly TextBox _version;
    private readonly TextBox _newField;
    private readonly SelectableTextBlock _output;

    public NetPacketPage()
    {
        _systemId = Ui.Input("1", 110);
        _version = Ui.Input("1", 110);
        _newField = Ui.Input("Remark", 160);
        _output = Ui.Code("@Demo.Text.NetPacketPage.Content002", output: true);

        _fieldList = new ListBox
        {
            ItemsSource = _fieldRows,
            Height = 130,
            FontFamily = new FontFamily("Consolas,Cascadia Mono,Menlo,monospace"),
            FontSize = 11.5,
        };

        DrawFields();

        var fields = Ui.Card("@Demo.Text.NetPacketPage.fields003",
            _fieldList,
            Ui.Row(_newField, Ui.Button("@Demo.Text.NetPacketPage.fields004", AddField), Ui.Button("@Demo.Text.NetPacketPage.fields005", () => _fieldRows.Clear())),
            Ui.Row(Ui.Button("Serialize", Serialize, primary: true), Ui.Button("Deserialize", Deserialize)),
            _output);

        var head = Ui.Card("@Demo.Text.NetPacketPage.head006",
            Ui.Field("systemId", _systemId),
            Ui.Field("version", _version),
            Ui.Field(L.Instance.GetResource("Demo.Text.NetPacketPage.head007"), Ui.Key("@Demo.Text.NetPacketPage.head008")),
            Ui.Field(L.Instance.GetResource("Demo.Text.NetPacketPage.head009"), Ui.Key(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.head010"), SerializeHelper.PacketHeadLen))),
            Ui.Hint("@Demo.Text.NetPacketPage.head011"));

        var declaration = Ui.Card("@Demo.Text.NetPacketPage.declaration012",
            Ui.Code("""
                [NetHead(0x01, 1)]
                public sealed class SamplePacket : INetObject
                {
                    [NetFieldOrder(0)] public int Id { get; set; }
                    [NetFieldOrder(1)] public float Score { get; set; }
                    [NetFieldOrder(2)] public string Title { get; set; } = string.Empty;
                }

                var buffer = packet.Serialize(systemId: 0);
                var restored = buffer.Deserialize<SamplePacket>();
                """),
            Ui.Code("@Demo.Text.NetPacketPage.declaration013", output: true),
            Ui.Hint("@Demo.Text.NetPacketPage.declaration014"));

        Content = Ui.Page(
            Ui.Heading("@Demo.Text.NetPacketPage.Content015"),
            Ui.Sub("@Demo.Text.NetPacketPage.Content016"),
            Ui.TwoColumns(fields, head),
            declaration);
    }

    public override string TitleKey => DemoKeys.ItemNetPacket;

    public override string DescriptionKey => DemoKeys.DescNetPacket;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("pkt.head", L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties017"), L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties018"),
            DemoProperty.Number("pkt.systemid", "systemId", 1, 0, 255, 1, null, L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties019")),
            DemoProperty.Number("pkt.version", "version", 1, 0, 255, 1, null, L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties020")),
            DemoProperty.Bool("pkt.bigendian", L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties021"), true, L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties022")),
            DemoProperty.Bool("pkt.offset", L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties023"), false, L.Instance.GetResource("Demo.Text.NetPacketPage.BuildProperties024"))));

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case "pkt.systemid":
                _systemId.Text = ((int)DemoProperty.Num(change, 1)).ToString();
                break;
            case "pkt.version":
                _version.Text = ((int)DemoProperty.Num(change, 1)).ToString();
                break;
        }
    }

    private void DrawFields()
    {
        var declaration = new SamplePacket();
        _fieldRows.Clear();
        foreach (var property in declaration.GetType().GetProperties())
        {
            if (!Attribute.IsDefined(property, typeof(NetFieldOrderAttribute)))
            {
                continue;
            }

            var order = ((NetFieldOrderAttribute?)Attribute.GetCustomAttribute(property, typeof(NetFieldOrderAttribute)))
                ?.Order ?? 0;
            var value = property.GetValue(declaration);
            _fieldRows.Add($"[{order}] {property.Name,-8} {PropertyTypeName(property.PropertyType),-8} = {value}");
        }
    }

    private void AddField()
    {
        var name = (_newField.Text ?? string.Empty).Trim();
        if (name.Length == 0)
        {
            _output.Text = L.Instance.GetResource("Demo.Text.NetPacketPage.AddField025");
            return;
        }

        _fieldRows.Add(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.AddField026"), name));
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.AddField027"), name);
        Logger.Warn(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.AddField028"), name));
    }

    private void Serialize()
    {
        var packet = new SamplePacket
        {
            Id = 42,
            Score = 1.0f,
            Title = "hello",
        };

        var systemId = long.TryParse(_systemId.Text, out var parsed) ? parsed : 1;
        var buffer = packet.Serialize(systemId);
        var hex = string.Join(' ', buffer.Select(b => "0x" + b.ToString("X2")));

        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.Serialize029"), systemId, buffer.Length)
                       + string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.Serialize030"), _version.Text, SerializeHelper.PacketHeadLen)
                       + hex;
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.Serialize031"), buffer.Length, systemId));
    }

    private void Deserialize()
    {
        var packet = new SamplePacket { Id = 42, Score = 1.0f, Title = "hello" };
        var buffer = packet.Serialize(1);
        var restored = buffer.Deserialize<SamplePacket>();

        var builder = new StringBuilder();
        builder.AppendLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.NetPacketPage.Deserialize032"), buffer.Length));
        builder.AppendLine($"  Id    = {restored.Id}");
        builder.AppendLine($"  Score = {restored.Score}");
        builder.AppendLine($"  Title = \"{restored.Title}\"");
        builder.AppendLine();
        builder.Append(restored.Id == packet.Id
                       && Math.Abs(restored.Score - packet.Score) < 0.001f
                       && restored.Title == packet.Title
            ? L.Instance.GetResource("Demo.Text.NetPacketPage.Deserialize033")
            : L.Instance.GetResource("Demo.Text.NetPacketPage.Deserialize034"));

        _output.Text = builder.ToString();
        Logger.Info(L.Instance.GetResource("Demo.Text.NetPacketPage.Deserialize035"));
    }

    private static string PropertyTypeName(Type type) => type == typeof(int) ? "int"
        : type == typeof(float) ? "float"
        : type == typeof(double) ? "double"
        : type == typeof(string) ? "string"
        : type.Name;
}
