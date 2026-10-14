using System.Diagnostics;
using System.Runtime.InteropServices;
using CodeWF.Toolkit.Core.UpdateChecking;

namespace CodeWF.Toolkit.Tests.Core;

public sealed class UpdateInstallerTests
{
    [Theory]
    [InlineData("Windows", ".exe", "", true)]
    [InlineData("Linux", ".deb", "xdg-open", false)]
    [InlineData("OSX", ".dmg", "open", false)]
    [InlineData("OSX", ".pkg", "open", false)]
    public async Task DispatchesInstallerWithoutStartingRealProcesses(string os, string extension, string command, bool exits)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        await File.WriteAllTextAsync(path, "fake installer", TestContext.Current.CancellationToken);
        ProcessStartInfo? launched = null;
        var installer = new UpdateInstaller(Platform(os), info => { launched = info; return true; });
        try
        {
            var result = await installer.InstallAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(UpdateInstallKind.Launched, result.Kind);
            Assert.Equal(exits, result.ShouldExitApplication);
            Assert.NotNull(launched);
            Assert.Equal(command == "" ? path : command, launched.FileName);
            if (command != "") Assert.Contains(path, launched.ArgumentList);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Windows", ".exe")]
    [InlineData("Linux", ".deb")]
    [InlineData("OSX", ".dmg")]
    [InlineData("Windows", ".zip")]
    [InlineData("Linux", ".zip")]
    [InlineData("OSX", ".zip")]
    public async Task FailedProcessStartReturnsManualAndNeverRequestsExit(string os, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        await File.WriteAllTextAsync(path, "fake package", TestContext.Current.CancellationToken);
        try
        {
            var result = await new UpdateInstaller(Platform(os), _ => false).InstallAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(UpdateInstallKind.Manual, result.Kind);
            Assert.False(result.ShouldExitApplication);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Windows")]
    [InlineData("Linux")]
    [InlineData("OSX")]
    public async Task PortablePackageOpensContainingDirectory(string os)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        await File.WriteAllTextAsync(path, "fake archive", TestContext.Current.CancellationToken);
        try
        {
            var result = await new UpdateInstaller(Platform(os), _ => true).InstallAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(UpdateInstallKind.OpenedFolder, result.Kind);
            Assert.False(result.ShouldExitApplication);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task MissingPackageDoesNotStartAnyProcess()
    {
        var result = await new UpdateInstaller(OSPlatform.Windows, _ => throw new InvalidOperationException()).InstallAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe"), TestContext.Current.CancellationToken);
        Assert.Equal(UpdateInstallKind.Unsupported, result.Kind);
    }

    private static OSPlatform Platform(string os) => os switch { "Windows" => OSPlatform.Windows, "OSX" => OSPlatform.OSX, _ => OSPlatform.Linux };
}
