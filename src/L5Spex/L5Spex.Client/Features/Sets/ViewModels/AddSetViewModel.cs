using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace L5Spex.Client.Features.Sets.ViewModels;

public partial class AddSetViewModel : ObservableValidator
{
    private readonly IMediator _mediator;
    private readonly TaskNotifier _load;
    private HashSet<string> _names;

    public AddSetViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Load = LoadNames();
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    [CustomValidation(typeof(AddSetViewModel), nameof(ValidateName))]
    private string _name;
    
    private Task Load
    {
        get => _load;
        init => SetPropertyAndNotifyOnCompletion(ref _load, value);
    }

    private bool CanCreate => !HasErrors && Load.IsCompletedSuccessfully;

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create(Window window)
    {
        /*var set = new Set(Name);
        using var connection = _database.Connect();
        await connection.InsertAsync(set);
        window.Close(set);*/
        throw new NotImplementedException();
    }

    [RelayCommand]
    private static void Close(Window window)
    {
        window.Close(null);
    }
    
    private async Task LoadNames()
    {
        /*var request = new GetSets.Request();
        using var connection = _database.Connect();
        var names = await connection.QueryAsync<string>("SELECT Name FROM [Set] ORDER BY Name DESC");
        _names = new HashSet<string>(names);*/
        throw new NotImplementedException();
    }
    
    private static ValidationResult ValidateName(string value, ValidationContext context)
    {
        var vm = (AddSetViewModel) context.ObjectInstance;
        return vm._names.Contains(value) ? new ValidationResult("Name already exists") : ValidationResult.Success;
    }
}