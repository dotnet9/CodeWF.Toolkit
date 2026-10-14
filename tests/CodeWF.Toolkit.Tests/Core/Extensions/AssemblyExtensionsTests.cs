using System.Reflection;
using System.Reflection.Emit;
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
        Assert.Equal(File.GetLastWriteTime(typeof(CodeWfAssemblyExtensions).Assembly.Location), compileTime.Value);
    }

    [Fact]
    public void CompileTime_PrefersExplicitMetadataForDeterministicBuilds()
    {
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToLocalTime(),
            typeof(AssemblyExtensionsTests).Assembly.CompileTime());
    }

    [Fact]
    public void CompileTime_DynamicAssemblyDoesNotBorrowEntryAssemblyTime()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("NoBuildTimestamp"), AssemblyBuilderAccess.Run);
        Assert.Null(assembly.CompileTime());
    }
}
