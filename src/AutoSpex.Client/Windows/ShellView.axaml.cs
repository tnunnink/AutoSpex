using System;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;

namespace AutoSpex.Client.Windows;

[UsedImplicitly]
public partial class ShellView : Window
{
    public ShellView()
    {
        InitializeComponent();
        
        DataContext = App.Container.GetInstance<ShellViewModel>();
        
        Closing += OnClosing;
        Opened += OnOpened;
#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <summary>
    /// Configures the default shell positions if they are not already set so we know we will get something
    /// when the project is launched.
    /// </summary>
    private void OnOpened(object? sender, EventArgs e)
    {
        App.Settings.Add(Setting.ShellHeight, 800);
        App.Settings.Add(Setting.ShellWidth, 1400);
        App.Settings.Add(Setting.ShellState, WindowState.Normal);

        var position = DefaultPosition(800, 1600);
        App.Settings.Add(Setting.ShellX, position.X);
        App.Settings.Add(Setting.ShellY, position.Y);
        App.Settings.Save();
        
        Height = App.Settings.Get(Setting.ShellHeight, double.Parse);
        Width = App.Settings.Get(Setting.ShellWidth, double.Parse);
        WindowState = App.Settings.Get(Setting.ShellState, Enum.Parse<WindowState>);
        var x = App.Settings.Get(Setting.ShellX, int.Parse);
        var y = App.Settings.Get(Setting.ShellY, int.Parse);
        Position = new PixelPoint(x, y);
    }

    /// <summary>
    /// Persist whatever shell positions are currently set so the next time the user launches a project it
    /// restores to the last location and size.
    /// </summary>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        App.Settings.Set(Setting.ShellHeight, Height);
        App.Settings.Set(Setting.ShellWidth, Width);
        App.Settings.Set(Setting.ShellState, WindowState);
        App.Settings.Set(Setting.ShellX, Position.X);
        App.Settings.Set(Setting.ShellY, Position.Y);
        App.Settings.Save();
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