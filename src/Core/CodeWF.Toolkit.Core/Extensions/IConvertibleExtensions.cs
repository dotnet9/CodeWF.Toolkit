using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace CodeWF.Toolkit.Core.Extensions;

public static class IConvertibleExtensions
{
    public static bool IsNumeric(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("TypeDescriptor uses reflection and runtime metadata. Use explicit conversions for trimming and NativeAOT.")]
    [return: MaybeNull]
    public static T ConvertTo<T>(this IConvertible? value)
    {
        if (value != null && value != DBNull.Value)
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (value.GetType() == type)
            {
                return (T)value;
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture)!);
            }

            if (type.IsNumeric())
            {
                return (T)value.ToType(type, new NumberFormatInfo());
            }

            var converter = TypeDescriptor.GetConverter(value);
            if (converter.CanConvertTo(type))
            {
                return (T)converter.ConvertTo(value, type)!;
            }

            converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return (T)converter.ConvertFrom(value)!;
            }

            return (T)Convert.ChangeType(value, type, CultureInfo.InvariantCulture)!;
        }

        return default!;
    }
}
