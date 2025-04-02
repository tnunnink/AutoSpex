using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class NewRepoPageModel : PageViewModel
{
    private readonly List<string> _names = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [CustomValidation(typeof(NewRepoPageModel), nameof(ValidateRepoName))]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string? _name;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
//todo need to add those custom folder path validations    [CustomValidation(typeof(NewRepoPageModel), nameof(ValidateRepoName))]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string? _location;

    public override async Task Load()
    {
        var repos = await Mediator.Send(new ListRepos());
        _names.AddRange(repos.Select(r => r.Name));
    }

    [RelayCommand]
    private async Task Browse()
    {
        var location = await Shell.StorageProvider.SelectLocation("Select Repository Location");
        if (location is null) return;
        Location = location;
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window window)
    {
        if (string.IsNullOrEmpty(Name)) return;
        if (string.IsNullOrEmpty(Location)) return;

        var repo = new Repo(Location, Name);

        window.Close(repo);
    }

    private bool CanAdd() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Location) && !IsErrored;

    /// <summary>
    /// Custom validation for the source name which must be unique in the application.
    /// </summary>
    public static ValidationResult? ValidateRepoName(object? value, ValidationContext context)
    {
        if (value is not string text)
            throw new InvalidOperationException("Expecting text name input.");

        if (context.ObjectInstance is not NewRepoPageModel page)
            throw new InvalidOperationException("Expecting NewSourcePageModel object instance.");

        return page._names.Contains(text)
            ? new ValidationResult("Repository name already used. Name must be unique.")
            : ValidationResult.Success;
    }
}