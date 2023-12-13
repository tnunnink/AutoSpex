using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Engine.Operations;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Specifications;

[UsedImplicitly]
public partial class AddFilterViewModel : ViewModelBase
{
    private readonly Element _element;

    public AddFilterViewModel(Element element)
    {
        _element = element;
    }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required]
    private string _propertyName = string.Empty;

    [ObservableProperty] private ObservableCollection<Property> _properties;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required]
    private Operation? _operation;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window window)
    {
        throw new NotImplementedException();
    }

    private bool CanAdd() => !HasErrors;

    [RelayCommand]
    private static void Cancel(Window window)
    {
        window.Close();
    }
}