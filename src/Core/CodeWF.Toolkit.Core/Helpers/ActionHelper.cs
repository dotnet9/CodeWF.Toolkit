using System;
using System.Threading.Tasks;

namespace CodeWF.Toolkit.Core.Helpers;

public static class ActionHelper
{
    public static bool CheckOvertime(Action checkAction, int overtimeMilliseconds = 3000)
    {
        var task = Task.Run(checkAction);
        return task.Wait(TimeSpan.FromMilliseconds(overtimeMilliseconds));
    }
}
