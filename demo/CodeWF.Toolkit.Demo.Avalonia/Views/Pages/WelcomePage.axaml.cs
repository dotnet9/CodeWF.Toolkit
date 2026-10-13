using Avalonia.Controls;
using Avalonia.Input.Platform;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

public partial class WelcomePage : DemoPageBase
{
    public Action<string>? Navigate { get; set; }
    public Action<string>? TogglePanel { get; set; }
    public WelcomeViewModel Overview { get; }

    public WelcomePage()
    {
        Overview = new WelcomeViewModel(key => Navigate?.Invoke(key), panel => TogglePanel?.Invoke(panel));
        InitializeComponent();
        DataContext = Overview;
        Overview.CopyToClipboard = text => TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard
            ? clipboard.SetTextAsync(text)
            : Task.FromException(new InvalidOperationException("Clipboard unavailable"));
    }

    public override string TitleKey => DemoKeys.ItemWelcome;
    public override string DescriptionKey => DemoKeys.DescWelcome;
    public override string XamlCode => "<codewf:StudioTheme />";
    public override string CSharpCode => "Application.Current.RequestedThemeVariant = StudioTheme.DotNetDark;";


}

internal static class DemoInfo
{
    public static string Version => typeof(DemoInfo).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
}
