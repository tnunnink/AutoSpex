﻿using System;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using JetBrains.Annotations;

namespace AutoSpex.Client;

[UsedImplicitly]
public partial class Shell : Window
{
    public Shell()
    {
        InitializeComponent();
        var shell = Container.Resolve<ShellViewModel>();
        shell.IsActive = true;
        DataContext = shell;
        
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

        Loaded += OnLoaded;
    }
    
    /// <summary>
    /// This is to override the Chrome hints that Actipro is configuring so that we can get a window drop shaddow.
    /// </summary>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
    }
}