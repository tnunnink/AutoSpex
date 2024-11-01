using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Shared;

/// <summary>
/// A view model class that is configured from an observer or page to allow dynamically building a context menu.
/// </summary>
public class MenuActionItem : ObservableObject
{
    public string? Header { get; set; }
    public object? Icon { get; set; }
    public string? Classes { get; set; }
    public KeyGesture? Gesture { get; set; }
    public ICommand? Command { get; set; }
    public object? CommandParameter { get; set; }
    public bool IsVisible => DetermineVisibility.Invoke();
    public Func<bool> DetermineVisibility { get; init; } = () => true;
    public IList<MenuActionItem> Items { get; set; } = [];
    public static MenuActionItem Separator => new() { Header = "-" };
}