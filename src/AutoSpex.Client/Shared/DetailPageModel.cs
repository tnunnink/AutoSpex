﻿using System.ComponentModel;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using MediatR;

namespace AutoSpex.Client.Shared;

public abstract partial class DetailPageModel : PageViewModel
{
    /// <summary>
    /// A command to initiate a save of the current state of the detail page.
    /// </summary>
    /// <returns>The <see cref="Task"/> which can await the <see cref="Result"/> of the save command.</returns>
    /// <remarks>
    /// Some pages will need the ability to save changes to the database by sending some command through the
    /// <see cref="Mediator"/> object. This command by default has no implementation. Deriving classes will implement
    /// this as needed. Each derived class can await the base implementation to get the result before processing further.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanSave))]
    protected virtual Task<Result> Save() => Task.FromResult(Result.Ok());

    /// <summary>
    /// Indicates whether the page can be saved or not. By default, this looks at whether there are changes using the
    /// <see cref="TrackableViewModel.IsChanged"/> property. Derived classes can override this implementation as needed.
    /// </summary>
    /// <returns><c>true</c> if the page can be saved, Otherwise, <c>false</c>.</returns>
    protected virtual bool CanSave() => IsChanged && !HasErrors;

    /// <summary>
    /// A command to close the current detail page.
    /// </summary>
    /// <returns>The <see cref="Task"/> which can await the flag indicating whether the page was closed or not.</returns>
    /// <remarks>
    /// This will first check for changes, and if any exists and the AlwaysDiscardChanges is not enabled, will
    /// prompt the user whether they want to save, cancel, or discard changes. If they select an option which results
    /// in the page closing, this method will return <c>true</c>, otherwise it will return <c>false</c>.
    /// </remarks>
    /// 
    [RelayCommand]
    public async Task<bool> Close()
    {
        var discard = Settings.App.AlwaysDiscardChanges;
        if (discard || !IsChanged)
        {
            Navigator.Close(this);
            return true;
        }

        var answer = await Prompter.PromptSave(Title);
        switch (answer)
        {
            case "Save":
                await Save();
                break;
            case "Cancel":
                return false;
        }

        Navigator.Close(this);
        return true;
    }

    /// <summary>
    /// A command to force close the page regardless of the state. This would mean discarding any current changes.
    /// This command simply uses the <see cref="Navigator"/> to issue the <see cref="Navigator.Close"/> which
    /// other pages should subscribe to if they are expected to close the pages they contain.
    /// </summary>
    [RelayCommand]
    protected void ForceClose()
    {
        Navigator.Close(this);
    }

    //We need to notify the SaveCommand when the IsChange property changes since that is what it is controlled on.
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsChanged))
            SaveCommand.NotifyCanExecuteChanged();
    }
}