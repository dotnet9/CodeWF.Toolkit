using CodeWF.Toolkit.Core.Extensions;

namespace CodeWF.Toolkit.Tests.Core;

public sealed class NullableConversionTests
{
    [Fact]
    public void ConvertToSupportsNullableScalarsAndEnums()
    {
        Assert.Equal(42, "42".ConvertTo<int?>());
        Assert.Equal(12.5, "12.5".ConvertTo<double?>());
        Assert.True("true".ConvertTo<bool?>());
        Assert.Equal(DayOfWeek.Friday, "Friday".ConvertTo<DayOfWeek?>());
        Assert.Equal(42, "42".ConvertTo<int>());
        Assert.True(typeof(int?).IsNumeric());
        Assert.True(typeof(decimal?).IsNumeric());
        Assert.False(typeof(bool?).IsNumeric());
    }

    [Fact]
    public void MissingValuesPreserveNullableDefaults()
    {
        IConvertible? missing = null;
        Assert.Null(missing.ConvertTo<int?>());
        Assert.Null(missing.ConvertTo<string>());
        Assert.Null(DBNull.Value.ConvertTo<DayOfWeek?>());
        Assert.Equal(0, missing.ConvertTo<int>());
    }

    [Fact]
    public void NullableValuesWorkWithCurrentCompilerFeatures()
    {
        NullableState?[] states = [new() { Text = "  value  " }, null];
        Assert.Equal("value", states[0]?.Text);
        states[0]?.Text = null;
        states[1]?.Text = "ignored";
        Assert.Null(states[0]?.Text);
    }

    private sealed class NullableState
    {
        public string? Text { get; set => field = value?.Trim(); }
    }
}
