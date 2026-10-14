using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using CodeWF.Toolkit.Lang.Json;

namespace CodeWF.Toolkit.Lang;

/// <summary>A description resolved from the owning library's embedded JSON on each access.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class LocalizedDescriptionAttribute(Type resourceOwner, string key) : DescriptionAttribute(key)
{
    private static readonly ConditionalWeakTable<Assembly, JsonLocalizer> Localizers = new();

    /// <inheritdoc />
    public override string Description => Localizers.GetValue(resourceOwner.Assembly, assembly => new JsonLocalizer(assembly)).GetResource(DescriptionValue);
}
