using System.ComponentModel.DataAnnotations;
using System.IO;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Client.Validation;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

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

    [RelayCommand]
    private async Task SelectLocation()
    {
        var path = await Shell.StorageProvider.SelectFolderUri("Select Project Location");
        if (path is null) return;
        Location = path.LocalPath;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create(Window dialog)
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Location)) return;

        var path = Path.Combine(Location, $"{Name}{Constant.SpexExtension}");
        
        if (File.Exists(path))
        {
            await Prompter.PromptError("File already exists", "The file already exists man. Pick another...");
            return;
        }

        var project = new Project(path);
        var result = await Mediator.Send(new CreateProject(project));

        if (result.IsSuccess)
        {
            var observer = new ProjectObserver(project);
            Messenger.Send(new ProjectObserver.Created(observer));
            dialog.Close(observer);
        }

        dialog.Close(null);
    }

    private bool CanCreate() => !HasErrors && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Location);
}