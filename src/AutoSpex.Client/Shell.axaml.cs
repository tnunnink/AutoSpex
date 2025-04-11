using System;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

// ReSharper disable UnusedParameter.Local

namespace AutoSpex.Client;

[UsedImplicitly]
public partial class Shell : Window, IRecipient<NavigationRequest>
{
    #region AvaloniaProperties

    public static readonly DirectProperty<Shell, bool> DialogOpenProperty =
        AvaloniaProperty.RegisterDirect<Shell, bool>(
            nameof(DialogOpen), o => o.DialogOpen, (o, v) => o.DialogOpen = v);

    public static readonly DirectProperty<Shell, AppPageModel?> AppPageProperty =
        AvaloniaProperty.RegisterDirect<Shell, AppPageModel?>(
            nameof(AppPage), o => o.AppPage, (o, v) => o.AppPage = v);

    #endregion

    private readonly Navigator? _navigator;
    private bool _dialogOpen;
    private AppPageModel? _appPage;

    public Shell()
    {
        InitializeComponent();
    }

    [UsedImplicitly]
    public Shell(IMessenger messenger, Navigator navigator)
    {
        InitializeComponent();
        DataContext = this;

        messenger.RegisterAll(this);
        _navigator = navigator;

        //This is a workaround to solve the window covering the task bar when maximizing while we are using custom title bar 
        this.GetPropertyChangedObservable(WindowStateProperty).AddClassHandler<Visual>((_, args) =>
        {
            if (!OperatingSystem.IsWindows()) return;
            if (args.GetNewValue<WindowState>() != WindowState.Maximized) return;

            var screen = Screens.ScreenFromWindow(this);
            if (screen is null) return;

            if (!(screen.WorkingArea.Height < ClientSize.Height * screen.Scaling)) return;

            ClientSize = screen.WorkingArea.Size.ToSize(screen.Scaling);

            if (Position is { X: >= 0, Y: >= 0 }) return;

            Position = screen.WorkingArea.Position;
            WindowHelper.FixAfterMaximizing(TryGetPlatformHandle()!.Handle, screen);
        });

        Loaded += OnLoaded;
    }

    public AppPageModel? AppPage
    {
        get => _appPage;
        set => SetAndRaise(AppPageProperty, ref _appPage, value);
    }

    public bool DialogOpen
    {
        get => _dialogOpen;
        set => SetAndRaise(DialogOpenProperty, ref _dialogOpen, value);
    }

    /// <summary>
    /// This is to override the Chrome hints that Actipro is configuring so that we can get a window drop shadow.
    /// </summary>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
        _navigator?.Navigate<AppPageModel>();
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is not AppPageModel page) return;
        if (message.Action != NavigationAction.Open) return;
        AppPage = page;
    }

    private void DialogShadowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        foreach (var window in OwnedWindows)
        {
            window.Close();
        }
    }
}