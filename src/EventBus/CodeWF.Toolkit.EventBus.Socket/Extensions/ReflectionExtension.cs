using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CodeWF.Toolkit.EventBus.Socket.Extensions;

public static class ReflectionExtension
{
    [return: MaybeNull]
    public static T Property<T>(this object obj, string propertyName, [AllowNull] T defaultValue = default!)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

        var propertyInfo = obj.GetType().GetProperty(propertyName);
        if (propertyInfo == null) return defaultValue;

        var value = propertyInfo.GetValue(obj);
        if (value == null) return defaultValue;

        if (value is T typedValue) return typedValue;

        try
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)(targetType.IsEnum
                ? Enum.Parse(targetType, Convert.ToString(value, CultureInfo.InvariantCulture)!, ignoreCase: true)
                : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
        }
        catch
        {
            return defaultValue;
        }
    }
}
