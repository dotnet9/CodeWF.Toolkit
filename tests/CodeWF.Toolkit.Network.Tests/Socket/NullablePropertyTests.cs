using CodeWF.Toolkit.EventBus.Socket.Extensions;

namespace CodeWF.Toolkit.Network.Tests.Socket;

public sealed class NullablePropertyTests
{
    [Fact]
    public void PropertyReadsNullableScalarsEnumsAndFallbacks()
    {
        var source = new { Number = "42", Day = "friday", Missing = (string?)null, Invalid = "not-a-number" };
        Assert.Equal(42, source.Property<int?>(nameof(source.Number)));
        Assert.Equal(DayOfWeek.Friday, source.Property<DayOfWeek?>(nameof(source.Day)));
        Assert.Null(source.Property<int?>(nameof(source.Missing)));
        Assert.Equal(7, source.Property<int?>(nameof(source.Invalid), 7));
        Assert.Null(source.Property<int?>("Absent"));
    }
}
