using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages.Projects;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Validation;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Home;

[UsedImplicitly]
public partial class CreateProjectPageModel : PageViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [Required]
    [PathValidCharacters]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [Required]
    [PathValidCharacters]
    private string _location = string.Empty;

    [ObservableProperty] private string _summary = string.Empty;

    [ObservableProperty] private bool _exists;

    [RelayCommand]
    private async Task SelectLocation()
    {
        var path = await Shell.StorageProvider.SelectFolderUri("Select Project Location");
        if (path is null) return;
        Location = path.LocalPath;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Location))
            return;

        var path = Path.Combine(Location, $"{Name}{Constant.SpexExtension}");

        if (!Uri.TryCreate(path, UriKind.Absolute, out var uri)) return; //todo prompt user.
        if (File.Exists(uri.LocalPath)) return; //todo prompt user.

        var project = new Project(path);
        var result = await Mediator.Send(new CreateProject(project));

        if (result.IsSuccess)
        {
            var observer = new ProjectObserver(project);
            await Navigator.Navigate(() => new ProjectPageModel(observer));
        }
    }

    private bool CanCreate() => !HasErrors && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Location);

    [RelayCommand]
    private Task Cancel()
    {
        return Navigator.Navigate<HomeProjectPageModel>();
    }
}