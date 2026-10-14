using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Linq;

namespace CodeWF.Toolkit.Core.Extensions;

public static class AssemblyExtensions
{
    public static string? Title(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        return attributes.Length > 0 ? ((AssemblyTitleAttribute)attributes[0]).Title : default;
    }

    public static string? Description(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
        return attributes.Length > 0 ? ((AssemblyDescriptionAttribute)attributes[0]).Description : default;
    }

    public static string? Company(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
        return attributes.Length > 0 ? ((AssemblyCompanyAttribute)attributes[0]).Company : default;
    }

    public static string? Product(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        return attributes.Length > 0 ? ((AssemblyProductAttribute)attributes[0]).Product : default;
    }

    public static string? Version(this Assembly? assembly)
    {
        return assembly?.GetName().Version?.ToString();
    }

    public static string? InformationalVersion(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
        return attributes.Length > 0
            ? ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion
            : default;
    }

    public static string? FileVersion(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
        return attributes.Length > 0 ? ((AssemblyFileVersionAttribute)attributes[0]).Version : default;
    }

    public static string? Copyright(this Assembly? assembly)
    {
        if (assembly == default)
        {
            return default;
        }

        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length > 0 ? ((AssemblyCopyrightAttribute)attributes[0]).Copyright : default;
    }

    public static DateTime? CompileTime(this Assembly? assembly)
    {
        if (assembly is null)
        {
            return default;
        }

        var timestamp = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "CodeWF.BuildTimestamp")?.Value;
        if (DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var buildTime))
        {
            return buildTime.LocalDateTime;
        }

        var assemblyPath = GetAssemblyPath(assembly);
        if (assemblyPath is null)
        {
            return default;
        }

        return GetPeCompileTime(assemblyPath) ?? GetFileModificationTime(assemblyPath);
    }

    private static string? GetAssemblyPath(Assembly assembly)
    {
        try
        {
            var assemblyPath = assembly.Location;
            if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
            {
                return assemblyPath;
            }
        }
        catch
        {
            // 动态程序集及单文件发布可能没有程序集路径。
        }

        if (assembly != Assembly.GetEntryAssembly()) return default;

        try
        {
            var processPath = Process.GetCurrentProcess().MainModule?.FileName;
            return !string.IsNullOrWhiteSpace(processPath) && File.Exists(processPath)
                ? processPath
                : default;
        }
        catch
        {
            return default;
        }
    }

    private static DateTime? GetPeCompileTime(string assemblyPath)
    {
        try
        {
            using var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var pe = new PEReader(stream);
            // 确定性构建在该字段保存内容哈希，不能将其解释为真实编译时间。
            if (pe.ReadDebugDirectory().Any(entry => entry.Type == DebugDirectoryEntryType.Reproducible)) return default;
            var secondsSince1970 = unchecked((uint)pe.PEHeaders.CoffHeader.TimeDateStamp);
            return secondsSince1970 == 0
                ? default
                : DateTimeOffset.FromUnixTimeSeconds(secondsSince1970).LocalDateTime;
        }
        catch
        {
            return default;
        }
    }

    private static DateTime? GetFileModificationTime(string exePath)
    {
        try
        {
            var fileInfo = new FileInfo(exePath);
            return fileInfo.LastWriteTime;
        }
        catch
        {
            return default;
        }
    }
}
