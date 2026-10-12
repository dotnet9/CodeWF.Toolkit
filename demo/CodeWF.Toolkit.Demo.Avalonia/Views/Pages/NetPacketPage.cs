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

    private const string CSharpSnippet = """
        using CodeWF.Toolkit.NetWeaver;
        using CodeWF.Toolkit.NetWeaver.Base;

        [NetHead(0x01, 1)]
        public sealed class SamplePacket : INetObject
        {
            [NetFieldOrder(0)]
            public int Id { get; set; }

            [NetFieldOrder(1)]
            public float Score { get; set; }

            [NetFieldOrder(2)]
            public string Title { get; set; } = string.Empty;
        }

        var packet = new SamplePacket { Id = 42, Score = 1.0f, Title = "hello" };

        var buffer = packet.Serialize(systemId: 1);     // 包头 [NetHead(0x01, 1)] + 字段
        var restored = buffer.Deserialize<SamplePacket>();

        Console.WriteLine($"{buffer.Length} 字节 → Id={restored.Id}, Score={restored.Score}, Title={restored.Title}");

        // [NetIgnoreMember] 跳过成员；位域场景用 NetFieldOffsetAttribute 精确布局
        // 裁剪 / AOT 下通过 DynamicDependency 保留反射成员
        """;

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
        _output = Ui.Code("等待序列化", output: true);

        _fieldList = new ListBox
        {
            ItemsSource = _fieldRows,
            Height = 130,
            FontFamily = new FontFamily("Consolas,Cascadia Mono,Menlo,monospace"),
            FontSize = 11.5,
        };

        DrawFields();

        var fields = Ui.Card("协议字段（真实序列化）",
            _fieldList,
            Ui.Row(_newField, Ui.Button("添加字段", AddField), Ui.Button("清空字段", () => _fieldRows.Clear())),
            Ui.Row(Ui.Button("Serialize", Serialize, primary: true), Ui.Button("Deserialize", Deserialize)),
            _output);

        var head = Ui.Card("包头",
            Ui.Field("systemId", _systemId),
            Ui.Field("version", _version),
            Ui.Field("字节序", Ui.Key("大端（Big-Endian）")),
            Ui.Field("固定头长", Ui.Key($"{SerializeHelper.PacketHeadLen} 字节")),
            Ui.Hint("包头由 [NetHead(systemId, version)] 声明，序列化时按 INetObject 的类型元数据写入；反序列化按同一元数据校验版本。"));

        var declaration = Ui.Card("协议声明",
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
            Ui.Code("""
                [NetIgnoreMember]           跳过该成员
                NetFieldOffsetAttribute     位域 / 精确偏移布局
                SerializeObject(obj, type)  非泛型重载（配合 DynamicDependency）
                PacketHeadLen               固定包头长度
                """, output: true),
            Ui.Hint("字段类型映射：int → 4 字节、float → 4 字节（IEEE754 大端）、string → 长度前缀 + UTF-8。"));

        Content = Ui.Page(
            Ui.Heading("NetWeaver：特性驱动的二进制序列化"),
            Ui.Sub("属性即协议：[NetHead(systemId, version)] 声明包头，[NetFieldOrder(n)] 声明字段顺序；下方按钮执行真实序列化与反序列化。"),
            Ui.TwoColumns(fields, head),
            declaration);
    }

    public override string TitleKey => DemoKeys.ItemNetPacket;

    public override string DescriptionKey => DemoKeys.DescNetPacket;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() => DemoProperty.Finish(
        DemoProperty.Category("pkt.head", "包头", "与 [NetHead] 对应的声明值（演示用）。",
            DemoProperty.Number("pkt.systemid", "systemId", 1, 0, 255, 1, null, "写入包头的系统标识。"),
            DemoProperty.Number("pkt.version", "version", 1, 0, 255, 1, null, "写入包头的协议版本。"),
            DemoProperty.Bool("pkt.bigendian", "大端序", true, "NetWeaver 固定使用大端序。"),
            DemoProperty.Bool("pkt.offset", "位域布局", false, "使用 NetFieldOffsetAttribute 精确布局。")));

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
            _output.Text = "字段名不能为空。";
            return;
        }

        _fieldRows.Add($"[演示] {name,-6} string = (仅 UI 演示，真实协议需改 C# 声明)");
        _output.Text = $"已添加占位字段 {name}：协议字段由编译期特性声明，运行时不能真正改变包结构。";
        Logger.Warn($"NetWeaver 演示：新增占位字段 {name}");
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

        _output.Text = $"Serialize(systemId: {systemId}) → {buffer.Length} 字节\n"
                       + $"包头 [NetHead(0x01, {_version.Text})] · {SerializeHelper.PacketHeadLen} 字节\n\n"
                       + hex;
        Logger.Info($"NetWeaver 序列化：{buffer.Length} 字节（systemId={systemId}）");
    }

    private void Deserialize()
    {
        var packet = new SamplePacket { Id = 42, Score = 1.0f, Title = "hello" };
        var buffer = packet.Serialize(1);
        var restored = buffer.Deserialize<SamplePacket>();

        var builder = new StringBuilder();
        builder.AppendLine($"Deserialize<SamplePacket>() ← {buffer.Length} 字节");
        builder.AppendLine($"  Id    = {restored.Id}");
        builder.AppendLine($"  Score = {restored.Score}");
        builder.AppendLine($"  Title = \"{restored.Title}\"");
        builder.AppendLine();
        builder.Append(restored.Id == packet.Id
                       && Math.Abs(restored.Score - packet.Score) < 0.001f
                       && restored.Title == packet.Title
            ? "往返校验：一致"
            : "往返校验：不一致！");

        _output.Text = builder.ToString();
        Logger.Info("NetWeaver 反序列化：往返校验完成");
    }

    private static string PropertyTypeName(Type type) => type == typeof(int) ? "int"
        : type == typeof(float) ? "float"
        : type == typeof(double) ? "double"
        : type == typeof(string) ? "string"
        : type.Name;
}
