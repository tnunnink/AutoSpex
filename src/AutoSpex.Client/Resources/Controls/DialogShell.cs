using System;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;

namespace AutoSpex.Client.Resources.Controls;

public class DialogShell : Window
{
    public DialogShell()
    {
        WindowState = WindowState.Normal;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
        ExtendClientAreaTitleBarHeightHint = 0;
        SystemDecorations = SystemDecorations.None;
        Background = new SolidColorBrush(Colors.Transparent);
        BorderBrush = new SolidColorBrush(Colors.Transparent);
        IsTabStop = false;
        CanResize = false;
        ShowInTaskbar = false;

        //This is a work around to solve the window covering the task bar when maximizing while we are using custom title bar 
        this.GetPropertyChangedObservable(WindowStateProperty).AddClassHandler<Visual>((_, args) =>
        {
            if (!OperatingSystem.IsWindows()) return;
            if (args.GetNewValue<WindowState>() != WindowState.Maximized) return;

            var screen = Screens.ScreenFromWindow(this);
            if (screen is null) return;

            if (!(screen.WorkingArea.Height < ClientSize.Height * screen.Scaling)) return;

            ClientSize = screen.WorkingArea.Size.ToSize(screen.Scaling);

            if (Position is {X: >= 0, Y: >= 0}) return;

            Position = screen.WorkingArea.Position;
            WindowHelper.FixAfterMaximizing(TryGetPlatformHandle()!.Handle, screen);
        });
    }
}