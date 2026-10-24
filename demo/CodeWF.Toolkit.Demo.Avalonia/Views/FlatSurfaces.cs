using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Toolkit.Demo.Avalonia.Views;

/// <summary>
/// 平面化表面覆盖层（评审反馈）：把 FlatLight/FlatDark 字典的表面画刷直接写入应用级资源。
/// Toolkit Demo 通过 NuGet 包引用 Controls 主题，令牌随包版本走；本覆盖层让表面
/// 立即与 CodeWF.Avalonia Demo 的纯白/纯黑观感一致，语义色与强调色不受影响。
/// 直接键值写入优先于合并字典，包内令牌切换不会冲掉覆盖。
/// </summary>
public static class FlatSurfaces
{
    private static bool _dark;

    public static void Apply(bool dark)
    {
        var app = Application.Current;
        if (app is null || dark == _dark && app.Resources.ContainsKey("StudioBackground"))
        {
            return;
        }

        _dark = dark;
        var uri = dark
            ? new Uri("avares://CodeWF.Toolkit.Demo.Avalonia/Views/FlatDark.axaml")
            : new Uri("avares://CodeWF.Toolkit.Demo.Avalonia/Views/FlatLight.axaml");
        var dictionary = (ResourceDictionary)AvaloniaXamlLoader.Load(uri);
        foreach (var key in dictionary.Keys)
        {
            app.Resources[key] = dictionary[key];
        }
    }
}
