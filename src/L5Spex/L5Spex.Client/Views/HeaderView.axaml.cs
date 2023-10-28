using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace L5Spex.Client.Views;

public partial class HeaderView : UserControl
{
    public HeaderView()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    [RelayCommand]
    private void CloseWindow()
    {
        if (VisualRoot is Window shell)
        {
            shell.Close();
        }
    }

    [RelayCommand]
    private void MaximizeWindow()
    {
        if (VisualRoot is Window shell)
        {
            shell.WindowState = shell.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
    }

    [RelayCommand]    
    private void MinimizeWindow()
    {
        if (VisualRoot is Window shell)
        {
            shell.WindowState = WindowState.Minimized;
        }
    }

    /*private async void SubscribeToWindowState()
    {
        var shell = (Window) VisualRoot;

        while (shell == null)
        {
            shell = (Window) VisualRoot;
            await Task.Delay(50);
        }

        shell.GetObservable(Window.WindowStateProperty).Subscribe(s =>
        {
            if (s != WindowState.Maximized)
            {
                maximizeIcon.Data =
                    Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                shell.Padding = new Thickness(0, 0, 0, 0);
                maximizeToolTip.Content = "Maximize";
            }

            if (s == WindowState.Maximized)
            {
                maximizeIcon.Data =
                    Geometry.Parse(
                        "M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                shell.Padding = new Thickness(7, 7, 7, 7);
                maximizeToolTip.Content = "Restore Down";

                // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                /*hostWindow.Padding = new Thickness(
                        hostWindow.OffScreenMargin.Left,
                        hostWindow.OffScreenMargin.Top,
                        hostWindow.OffScreenMargin.Right,
                        hostWindow.OffScreenMargin.Bottom);#1#
            }
        });
    }*/
}