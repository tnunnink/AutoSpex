using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Shared;

public partial class MenuItemViewModel : ObservableObject
{
    private MenuItemViewModel()
    {
    }

    public static MenuItemViewModel Separator => new() {Header = "-"};
    
    public MenuItemViewModel(string header, ICommand command,
        object? commandParameter = default,
        KeyGesture? inputGesture = default,
        IList<MenuItemViewModel>? items = default)
    {
        Header = header;
        Command = command;
        CommandParameter = commandParameter;
        InputGesture = inputGesture;
        Items = items ?? new List<MenuItemViewModel>();
    }

    public MenuItemViewModel(string header, string icon, ICommand command,
        object? commandParameter = default,
        KeyGesture? inputGesture = default,
        IList<MenuItemViewModel>? items = default)
    {
        Header = header;
        Icon = icon;
        Command = command;
        CommandParameter = commandParameter;
        InputGesture = inputGesture;
        Items = items ?? new List<MenuItemViewModel>();
    }

    [ObservableProperty] private string? _header;
    [ObservableProperty] private string? _icon;
    [ObservableProperty] private KeyGesture? _inputGesture;
    [ObservableProperty] private ICommand? _command;
    [ObservableProperty] private object? _commandParameter;
    [ObservableProperty] private bool? _isEnabled = true;
    public IList<MenuItemViewModel> Items { get; set; } = [];
}