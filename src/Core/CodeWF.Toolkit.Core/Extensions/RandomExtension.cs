using System;

namespace CodeWF.Toolkit.Core.Extensions;

public static class RandomExtension
{
    public static int GetInt(int start, int end)
    {
        return Random.Shared.Next(start, end);
    }
}
