using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Tests.Files.Models;
using Xunit;

namespace CodeWF.Toolkit.Tests.Files;

public class EnumDescriptionTest
{
    [Fact]
    public void Test_GetEnumDescription_Success()
    {
        var usage = PowerUsage.High;
        var alarmStatus = AlarmStatus.OverLimit | AlarmStatus.UserChanged;

        var usageDescription = usage.GetDescription();
        var alarmStatusDescription = alarmStatus.GetDescription();

        Assert.Equal("高", usageDescription);
        Assert.Equal("超限,切换用户", alarmStatusDescription);

        alarmStatus = AlarmStatus.Normal;
        alarmStatusDescription = alarmStatus.GetDescription();
        Assert.Equal("正常", alarmStatusDescription);
    }
}