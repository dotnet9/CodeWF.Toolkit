using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using CodeWF.Toolkit.Lang.Json;
using CodeWF.Toolkit.NetWrapper.Models;

namespace CodeWF.Toolkit.Network.Tests.NetWrapper;

public sealed class LocalizationTests
{
    [Fact]
    public void DescriptionAttributeResolvesAgainAfterCultureChanges()
    {
        var culture = CultureInfo.CurrentUICulture;
        var description = typeof(TcpResponseStatus).GetField(nameof(TcpResponseStatus.Success))!.GetCustomAttribute<DescriptionAttribute>()!;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal("Success", description.Description);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            Assert.Equal(new JsonLocalizer(typeof(TcpResponseStatus).Assembly).GetResource("Toolkit.NetWrapper.Status.Success", "zh-CN"), description.Description);
            Assert.NotEqual("Success", description.Description);
        }
        finally { CultureInfo.CurrentUICulture = culture; }
    }
}
