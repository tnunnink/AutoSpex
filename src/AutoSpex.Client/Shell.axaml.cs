using System;
using System.Globalization;
using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client;

[UsedImplicitly]
public partial class Shell : Window, IRecipient<LaunchProjectMessage>
{
    private double _clientHeight;
    private double _clientWidth;
    private int _clientX;
    private int _clientY;


    public Shell(IMessenger messenger)
    {
        InitializeComponent();
        Resized += OnResized;
        PositionChanged += OnPositionChanged;
        Closing += OnClosing;

#if DEBUG
        this.AttachDevTools();
#endif

        messenger.RegisterAll(this);
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        _clientX = e.Point.X;
        _clientY = e.Point.Y;
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            App.Settings.Set(Setting.WindowHeight, _clientHeight.ToString(CultureInfo.InvariantCulture));
            App.Settings.Set(Setting.WindowWidth, _clientWidth.ToString(CultureInfo.InvariantCulture));
            App.Settings.Set(Setting.WindowX, _clientX.ToString());
            App.Settings.Set(Setting.WindowY, _clientY.ToString());
            App.Settings.Set(Setting.WindowState, WindowState.ToString());
            await App.Settings.Save().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private void OnResized(object? sender, WindowResizedEventArgs e)
    {
        _clientHeight = e.ClientSize.Height;
        _clientWidth = e.ClientSize.Width;
    }

    public void Receive(LaunchProjectMessage message)
    {
        var height = App.Settings.Find(Setting.WindowHeight);
        var width = App.Settings.Find(Setting.WindowWidth);
        
        Height = height is not null ? double.Parse(height) : 1000;
        Width = width is not null ? double.Parse(width) : 1600;

        var x = DetermineX();
        var y = DetermineY();
        
        Position = new PixelPoint(x, y);
    }

    private int DetermineX()
    {
        var x = App.Settings.Find(Setting.WindowX)?.Parse<int>();
        if (x is not null) return x.Value;
        
        var bounds = Screens.Primary?.Bounds;
        var screenWidth = bounds?.Width ?? 0;
        var windowWidth = Width;
        return (int)(screenWidth - windowWidth) / 2;
    }
    
    private int DetermineY()
    {
        var y = App.Settings.Find(Setting.WindowY)?.Parse<int>();
        if (y is not null) return y.Value;
        
        var bounds = Screens.Primary?.Bounds;
        var screenHeight = bounds?.Height ?? 0;
        var windowHeight = Height;
        return (int)(screenHeight - windowHeight) / 2;
    }
}