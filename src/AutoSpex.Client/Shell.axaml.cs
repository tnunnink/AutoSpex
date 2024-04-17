using System;
using System.Windows.Input;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
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

    public static readonly DirectProperty<Shell, bool> ProjectPageOpenProperty =
        AvaloniaProperty.RegisterDirect<Shell, bool>(
            nameof(ProjectPageOpen), o => o.ProjectPageOpen, (o, v) => o.ProjectPageOpen = v);

    public static readonly DirectProperty<Shell, PageViewModel?> CurrentPageProperty =
        AvaloniaProperty.RegisterDirect<Shell, PageViewModel?>(
            nameof(CurrentPage), o => o.CurrentPage, (o, v) => o.CurrentPage = v);

    public static readonly DirectProperty<Shell, ICommand?> NavigateHomeCommandProperty =
        AvaloniaProperty.RegisterDirect<Shell, ICommand?>(
            nameof(NavigateHomeCommand), o => o.NavigateHomeCommand, (o, v) => o.NavigateHomeCommand = v);

    #endregion

    private readonly Navigator? _navigator;
    private bool _dialogOpen;
    private bool _projectPageOpen;
    private PageViewModel? _currentPage;
    private ICommand? _navigateHomeCommand;

    public Shell()
    {
        InitializeComponent();
    }

    public Shell(IMessenger messenger, Navigator navigator)
    {
        InitializeComponent();
        DataContext = this;

        messenger.RegisterAll(this);
        _navigator = navigator;

        NavigateHomeCommand = new AsyncRelayCommand(NavigateHome);

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

    public PageViewModel? CurrentPage
    {
        get => _currentPage;
        set => SetAndRaise(CurrentPageProperty, ref _currentPage, value);
    }

    public ICommand? NavigateHomeCommand
    {
        get => _navigateHomeCommand;
        set => SetAndRaise(NavigateHomeCommandProperty, ref _navigateHomeCommand, value);
    }

    public bool DialogOpen
    {
        get => _dialogOpen;
        set => SetAndRaise(DialogOpenProperty, ref _dialogOpen, value);
    }

    public bool ProjectPageOpen
    {
        get => _projectPageOpen;
        set => SetAndRaise(ProjectPageOpenProperty, ref _projectPageOpen, value);
    }

    /// <summary>
    /// This is to override the Chrome hints that Actipro is configuring so that we can get a window drop shadow.
    /// </summary>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
    }

    /// <summary>
    /// Navigates the <see cref="HomePageModel"/> instance into the view of the application shell.
    /// </summary>
    private async Task NavigateHome()
    {
        if (_navigator is null) return;
        if (CurrentPage is not null && CurrentPage.Route.Equals(nameof(HomePageModel))) return;
        await _navigator.NavigateHome();
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is not HomePageModel and not ProjectPageModel) return;
        if (message.Action != NavigationAction.Open) return;
        CurrentPage = message.Page;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CurrentPageProperty)
        {
            ProjectPageOpen = change.GetNewValue<PageViewModel?>()?.Route.Equals(nameof(HomePageModel)) is false;
        }
    }

    private void DialogShadowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        foreach (var window in OwnedWindows)
        {
            window.Close();
        }
    }
}