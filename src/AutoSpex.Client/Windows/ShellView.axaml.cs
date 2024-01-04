using System;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using JetBrains.Annotations;

namespace AutoSpex.Client.Windows;

[UsedImplicitly]
public partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<ShellViewModel>();
        
        //This is a work around to solve the window covering the task bar when maximizing while we are using custom title bar 
        this.GetPropertyChangedObservable(WindowStateProperty).AddClassHandler<Visual>((t, args) =>
        {
            if (!OperatingSystem.IsWindows()) return;
            if (args.GetNewValue<WindowState>() != WindowState.Maximized) return;
            
            var screen = Screens.ScreenFromWindow(this);
            if (screen is null) return;
            
            if (!(screen.WorkingArea.Height < ClientSize.Height * screen.Scaling)) return;
           
            ClientSize = screen.WorkingArea.Size.ToSize(screen.Scaling);

            if (Position is {X: >= 0, Y: >= 0}) return;
            
            Position = screen.WorkingArea.Position;
            WindowHelper.FixAfterMaximizing(TryGetPlatformHandle().Handle, screen);
        });
        
        Loaded += OnLoaded;
        Closing += OnClosing;
        Opened += OnOpened;
#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <summary>
    /// This is to override the Chrome hints that Actipro is configuring so that we can get a window drop shaddow.
    /// </summary>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
    }

    /// <summary>
    /// Configures the default shell positions if they are not already set so we know we will get something
    /// when the project is launched.
    /// </summary>
    private void OnOpened(object? sender, EventArgs e)
    {
        var position = DefaultPosition(800, 1400);
        Settings.App.Add(nameof(Settings.ShellX), position.X);
        Settings.App.Add(nameof(Settings.ShellY), position.Y);
        
        Height = Settings.App.ShellHeight;
        Width = Settings.App.ShellWidth;
        WindowState = Settings.App.ShellState;
        Position = new PixelPoint(Settings.App.ShellX, Settings.App.ShellY);
        Focusable = true;
    }

    /// <summary>
    /// Persist whatever shell positions are currently set so the next time the user launches a project it
    /// restores to the last location and size.
    /// </summary>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        Settings.App.Save(s =>
        {
            s.ShellHeight = Height;
            s.ShellWidth = Width;
            s.ShellState = WindowState;
            s.ShellX = Position.X;
            s.ShellY = Position.Y;
        });
    }
    

    private PixelPoint DefaultPosition(double height, double width)
    {
        var screen = Screens.ScreenFromWindow(this);
        var bounds = screen?.Bounds;

        var screenWidth = bounds?.Width ?? 1920;
        var screenHeight = bounds?.Height ?? 1080;

        var x = (int)(screenWidth - width) / 2;
        var y = (int)(screenHeight - height) / 2;

        return new PixelPoint(x, y);
    }
}