using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class FilterEntryPageModel(Element element) : PageViewModel
{
    [ObservableProperty] private Element _element = element;

    [ObservableProperty] private Property? _property;

    [ObservableProperty] private string? _propertyName;

    [ObservableProperty] private Operation? _operation;
    public ObservableCollection<ArgumentObserver> Arguments { get; } = [];

    [RelayCommand]
    private void AddFilter(Window dialog)
    {
    }
    
    
}