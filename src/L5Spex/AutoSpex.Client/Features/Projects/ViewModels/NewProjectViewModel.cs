using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Projects;

[UsedImplicitly]
public partial class NewProjectViewModel : ViewModelBase
{
    private Uri? _uri;
    private readonly IStoragePicker _picker;

    public NewProjectViewModel(IStoragePicker picker)
    {
        _picker = picker;
    }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [Required]
    [CustomValidation(typeof(NewProjectViewModel), nameof(ValidateFileName))]
    private string _name = string.Empty;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [Required]
    [CustomValidation(typeof(NewProjectViewModel), nameof(ValidateDirectoryName))]
    private string _location = string.Empty;

    [ObservableProperty] private bool _exists;
    
    [RelayCommand]
    private async Task SelectLocation()
    {
        var folder = await _picker.PickFolder("Select Project Location");
        if (folder is null) return;
        Location = folder.Path.LocalPath;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create(Window window)
    {
        if (_uri is null) return;

        if (File.Exists(_uri.LocalPath))
        {
            //todo throw error
        }
        
        window.Close(_uri);
    }

    private bool CanCreate() => !HasErrors && _uri is not null && !Exists;

    [RelayCommand]
    private static void Cancel(Window window)
    {
        window.Close();
    }

    partial void OnNameChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(Location))
            return;

        var path = Path.Combine(Location, $"{Name}{Constant.SpexExtension}");

        _uri = Uri.TryCreate(path, UriKind.Absolute, out var uri) ? uri : default;

        Exists = _uri is not null && File.Exists(_uri.LocalPath);
    }
    
    partial void OnLocationChanged(string value)
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(value))
            return;

        var path = Path.Combine(Location, $"{Name}{Constant.SpexExtension}");

        _uri = Uri.TryCreate(path, UriKind.Absolute, out var uri) ? uri : default;
        
        Exists = _uri is not null && File.Exists(_uri.LocalPath);
    }
    
    public static ValidationResult? ValidateFileName(string value, ValidationContext context)
    {
        return value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            ? new ValidationResult($"The name {value} is not a valid file name.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidateDirectoryName(string value, ValidationContext context)
    {
        return value.IndexOfAny(Path.GetInvalidPathChars()) >= 0
            ? new ValidationResult($"The path {value} is not a valid directory.")
            : ValidationResult.Success;
    }
}