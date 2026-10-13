using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;

namespace CodeWF.Toolkit.Demo.Avalonia.Views;

/// <summary>
/// 外壳浮层（命令面板 + 通知中心）的输入处理：
/// 打开时聚焦搜索框、↑↓ 移动选择、Enter 执行、Esc 关闭，点击遮罩空白处关闭。
/// </summary>
public partial class ShellOverlay : UserControl
{
    public ShellOverlay()
    {
        InitializeComponent();

        CommandMask.PointerPressed += (_, e) =>
        {
            if (ViewModel is { } viewModel && e.Source == CommandMask)
            {
                viewModel.CommandPaletteOpen = false;
            }
        };

        CommandBox.PointerPressed += (_, e) => e.Handled = true;
        AddHandler(KeyDownEvent, OnOverlayKeyDown, RoutingStrategies.Tunnel);
        DataContextChanged += (_, _) => HookViewModel();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void HookViewModel()
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.CommandPaletteOpen) && viewModel.CommandPaletteOpen)
            {
                // 等布局把焦点交给输入框（可见性刚变更时直接 Focus 会被吞掉）。
                Dispatcher.UIThread.Post(() => CommandInput.Focus(), DispatcherPriority.Input);
            }
        };
    }

    private void OnOverlayKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Escape when viewModel.CommandPaletteOpen:
                viewModel.CommandPaletteOpen = false;
                e.Handled = true;
                break;
            case Key.Up when viewModel.CommandPaletteOpen:
                viewModel.MoveCommandSelection(-1);
                e.Handled = true;
                break;
            case Key.Down when viewModel.CommandPaletteOpen:
                viewModel.MoveCommandSelection(1);
                e.Handled = true;
                break;
            case Key.Enter when viewModel.CommandPaletteOpen:
                viewModel.RunSelectedCommand();
                e.Handled = true;
                break;
        }
    }
}
