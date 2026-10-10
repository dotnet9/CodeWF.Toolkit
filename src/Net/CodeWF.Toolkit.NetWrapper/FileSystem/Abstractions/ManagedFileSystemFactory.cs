using CodeWF.Toolkit.NetWrapper.Services;

namespace CodeWF.Toolkit.NetWrapper.Abstractions;

/// <summary>
///     文件系统实现工厂。
/// </summary>
public static class ManagedFileSystemFactory
{
    public static IManagedFileSystem CreateDefault() => new PhysicalManagedFileSystem();
}
