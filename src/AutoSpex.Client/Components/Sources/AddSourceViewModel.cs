using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components.Sources;

[UsedImplicitly]
public partial class AddSourceViewModel : PageViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required]
    private string _name = string.Empty;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required]
    private Uri? _uri;
    
    [RelayCommand]
    private async Task SelectSource()
    {
        var uri = await Shell.StorageProvider.SelectL5XUri();
        if (uri is null) return;

        Uri = uri;
        
        if (string.IsNullOrEmpty(Name))
            Name = System.IO.Path.GetFileNameWithoutExtension(uri.Segments.Last());
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window window)
    {
        if (Uri is null) return;
        window.Close(new {Uri, Name});
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