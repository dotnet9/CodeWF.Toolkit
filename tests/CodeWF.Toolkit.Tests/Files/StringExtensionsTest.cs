using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.Helpers;
using Xunit;

namespace CodeWF.Toolkit.Tests.Files;

public class StringExtensionsTest
{
    [Fact]
    public void Test_OpenBrowserForVisitSite_Success()
    {
        "https://codewf.com".OpenBrowserForVisitSite();
    }
}