using System.Reflection;
using CodeWF.Toolkit.Core.Extensions;
using CodeWfAssemblyExtensions = CodeWF.Toolkit.Core.Extensions.AssemblyExtensions;

namespace CodeWF.Toolkit.Tests.Core.Extensions;

public class AssemblyExtensionsTests
{
    [Fact]
    public void CompileTime_NullAssembly_ReturnsNull()
    {
        Assembly? assembly = null;

        Assert.Null(assembly.CompileTime());
    }

    [Fact]
    public void CompileTime_UsesTheProvidedAssembly()
    {
        var compileTime = typeof(CodeWfAssemblyExtensions).Assembly.CompileTime();

        Assert.NotNull(compileTime);
    }
}
