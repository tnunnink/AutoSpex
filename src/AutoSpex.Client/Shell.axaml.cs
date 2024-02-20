using System;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages.Home;
using AutoSpex.Client.Pages.Projects;
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
    private readonly Navigator _navigator;

    private bool _dialogOpen;
    private PageViewModel? _currentPage;
    private ICommand? _navigateHomeCommand;
    private ICommand? _navigateProjectCommand;

    public static readonly DirectProperty<Shell, bool> DialogOpenProperty =
        AvaloniaProperty.RegisterDirect<Shell, bool>(
            nameof(DialogOpen), o => o.DialogOpen, (o, v) => o.DialogOpen = v);

    public static readonly DirectProperty<Shell, PageViewModel?> CurrentPageProperty =
        AvaloniaProperty.RegisterDirect<Shell, PageViewModel?>(
            nameof(CurrentPage), o => o.CurrentPage, (o, v) => o.CurrentPage = v);

    public static readonly DirectProperty<Shell, ICommand?> NavigateHomeCommandProperty =
        AvaloniaProperty.RegisterDirect<Shell, ICommand?>(
            nameof(NavigateHomeCommand), o => o.NavigateHomeCommand, (o, v) => o.NavigateHomeCommand = v);

    public static readonly DirectProperty<Shell, ICommand?> NavigateProjectCommandProperty =
        AvaloniaProperty.RegisterDirect<Shell, ICommand?>(
            nameof(NavigateProjectCommand), o => o.NavigateProjectCommand, (o, v) => o.NavigateProjectCommand = v);

    public Shell()
    {
        InitializeComponent();

        _navigator = new Navigator(WeakReferenceMessenger.Default);
    }

    public Shell(IMessenger messenger, Navigator navigator)
    {
        InitializeComponent();

        _navigator = navigator;
        DataContext = this;
        NavigateHomeCommand = new RelayCommand(NavigateHome);
        NavigateProjectCommand = new RelayCommand<ProjectObserver?>(NavigateProject);

        messenger.RegisterAll(this);

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

    public ICommand? NavigateProjectCommand
    {
        get => _navigateProjectCommand;
        set => SetAndRaise(NavigateProjectCommandProperty, ref _navigateProjectCommand, value);
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
    }

    private void NavigateHome()
    {
        if (CurrentPage is not null && CurrentPage.Route.Equals("HomePageModel")) return;
        _navigator.NavigateHome();
    }

    private void NavigateProject(ProjectObserver? project)
    {
        if (project is null) return;
        if (CurrentPage is not null && CurrentPage.Route.Equals(project.Uri.LocalPath)) return;
        _navigator.Navigate(project);
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is HomePageModel or ProjectPageModel)
        {
            CurrentPage = message.Page;
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