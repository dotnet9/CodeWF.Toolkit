using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Toolkit.Lang;

/// <summary>每次访问时从所属库的内嵌 JSON 中查询当前语言的说明。</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class LocalizedDescriptionAttribute(Type resourceOwner, string key) : DescriptionAttribute(key)
{
    private static readonly ConditionalWeakTable<Assembly, JsonLocalizer> Localizers = new();

    /// <inheritdoc />
    public override string Description => Localizers.GetValue(resourceOwner.Assembly, assembly => new JsonLocalizer(assembly)).GetResource(DescriptionValue);
}
