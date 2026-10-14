using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.Helpers;
using Xunit;

namespace CodeWF.Toolkit.Tests.Files;

public class StringExtensionsTest
{
    [Fact(Skip = "手动桌面验收：会打开系统浏览器，不在常规测试中执行。")]
    public void Test_OpenBrowserForVisitSite_Success()
    {
        "https://codewf.com".OpenBrowserForVisitSite();
    }
}
