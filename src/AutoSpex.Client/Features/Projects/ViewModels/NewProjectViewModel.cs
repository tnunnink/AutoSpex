using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Projects;

[UsedImplicitly]
public partial class NewProjectViewModel : ViewModelBase, IModalDialogViewModel, ICloseable
{
    private readonly IDialogService _dialog;

    public NewProjectViewModel(IDialogService dialog)
    {
        _dialog = dialog;
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

    public Uri? Uri { get; private set; }
    
    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;
    

    [RelayCommand]
    private async Task SelectLocation()
    {
        var path = await _dialog.ShowSelectFolderDialog("Select Project Location");
        if (path is null) return;
        Location = path.LocalPath;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Location))
            return;

        var path = Path.Combine(Location, $"{Name}{Constant.SpexExtension}");

        Uri = Uri.TryCreate(path, UriKind.Absolute, out var uri) ? uri : default;

        Exists = Uri is not null && File.Exists(Uri.LocalPath);
        if (Exists) return;

        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private bool CanCreate() => !HasErrors && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Location);

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
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