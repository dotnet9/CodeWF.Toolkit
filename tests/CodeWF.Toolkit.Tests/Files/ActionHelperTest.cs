using System.Diagnostics;
using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.Helpers;

namespace CodeWF.Toolkit.Tests.Files;

public class ActionHelperTest
{
    [Fact]
    public void Test_CheckNormalAction_Success()
    {
        var check = ActionHelper.CheckOvertime(DoSomething, 2000);
        Assert.True(check);
        return;

        void DoSomething()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }
    }

    [Fact]
    public void Test_CheckOvertimeAction_Success()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var check = ActionHelper.CheckOvertime(DoSomething, 100);
        Debug.WriteLine("Cancel running");
        cancellationTokenSource.Cancel();
        Assert.False(check);
        Debug.WriteLine("Check over");
        return;

        void DoSomething()
        {
            var beginTime = DateTime.Now;
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine($"{beginTime.GetDiffTime(DateTime.Now)}: Test overtime");
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}
