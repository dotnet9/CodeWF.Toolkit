using System;
using System.Reflection;
using CodeWF.Toolkit.Core.Extensions;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>
/// 应用版本的唯一来源：程序集的 <c>AssemblyInformationalVersion</c>。
/// 发布时由 CI 用 <c>-p:Version=</c> 注入，关于窗口、更新比较、安装包校验共用它，
/// 避免界面里再写死一个版本号。
/// </summary>
public static class UpdateVersion
{
    /// <summary>
    /// 展示用版本号（如 <c>1.5.3</c>）。SourceLink 会在 informational version 后追加
    /// <c>+提交号</c>，展示时去掉；拿不到程序集时返回 <c>0.0.0</c>。
    /// </summary>
    public static string Display(Assembly? assembly = null)
    {
        string? informational = (assembly ?? Assembly.GetEntryAssembly()).InformationalVersion();
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return plus > 0 ? informational[..plus] : informational;
        }

        return (assembly ?? Assembly.GetEntryAssembly()).Version() ?? "0.0.0";
    }

    /// <summary>把展示版本解析成补齐到四段的可比较版本；解析失败返回 <c>0.0.0.0</c>。</summary>
    public static Version Current(Assembly? assembly = null)
        => VersionUtil.Parse(Display(assembly)) ?? new Version(0, 0, 0, 0);
}
