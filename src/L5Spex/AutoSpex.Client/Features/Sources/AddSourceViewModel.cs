using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Sources;

[UsedImplicitly]
public partial class AddSourceViewModel : ViewModelBase
{
    private readonly IStoragePicker _picker;

    public AddSourceViewModel(IStoragePicker picker)
    {
        _picker = picker;
    }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required]
    private string _name = string.Empty;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required]
    private Uri? _path;
    
    [RelayCommand]
    private async Task SelectSource()
    {
        var file = await _picker.PickSource();
        if (file is null) return;
        
        Path = file.Path;
        
        if (string.IsNullOrEmpty(Name))
            Name = file.Name;
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window window)
    {
        if (Path is null) return;
        window.Close(new {Path, Name});
    }

    private bool CanAdd() => !HasErrors;

    [RelayCommand]
    private static void Cancel(Window window)
    {
        window.Close();
    }
    
    public static ValidationResult? ValidateSourceFile(string value, ValidationContext context)
    {
        /*return value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            ? new ValidationResult($"The name {value} is not a valid file name.")
            : ValidationResult.Success;*/
        throw new NotImplementedException();
    }
}