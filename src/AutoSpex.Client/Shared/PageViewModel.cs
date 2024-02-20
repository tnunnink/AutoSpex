using System;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace AutoSpex.Client.Shared;

public abstract partial class PageViewModel : TrackableViewModel, IEquatable<PageViewModel>
{
    /// <summary>
    /// A resource identifier that identifies this page relative to any other. The default implementation is this
    /// type name but derived classes can override to indicate how a given object should be identified.
    /// </summary>
    public virtual string Route => GetType().Name;

    /// <summary>
    /// The name of the page that can be used to display in a tab or other control.
    /// </summary>
    public virtual string Title => "Title";

    /// <summary>
    /// The string key of the icon that this page corresponds to.
    /// </summary>
    public virtual string Icon => "None";

    /// <summary>
    /// A command to initiate the loading of data from external resources in order to populate this page with it's
    /// required content.
    /// </summary>
    /// <returns>The <see cref="Task"/> for loading the data.</returns>
    /// <remarks>Since most pages will needed to perform some initialization by sending a request to fetch data via
    /// our <see cref="Mediator"/> we will provide this command by default with no implementation. Deriving pages can
    /// implement this load <see cref="RelayCommand"/> which will be awaited as the pages are navigated to using out
    /// application navigator service.
    /// </remarks>
    [RelayCommand]
    public virtual Task Load() => Task.CompletedTask;

    /// <summary>
    /// A command to initiate a save of the current state of the page.
    /// </summary>
    /// <returns>The <see cref="Task"/> for saving the data</returns>
    /// <remarks>Some pages will need the ability to save changes to the database by sending some command through the
    /// <see cref="Mediator"/> object. This command by default has no implementation. Deriving classes will implement
    /// this as needed.</remarks>
    [RelayCommand(CanExecute = nameof(CanSave))]
    protected virtual Task Save() => Task.CompletedTask;

    /// <summary>
    /// Indicates whether the page can be saved or not. By default this looks at whether there are changes using the
    /// <see cref="IsChanged"/> property. Derived classes can override this implementation as needed.
    /// </summary>
    /// <returns><c>true</c> if the page can be saved, Otherwise, <c>false</c>.</returns>
    protected virtual bool CanSave() => IsChanged;

    [RelayCommand(CanExecute = nameof(CanClose))]
    public virtual async Task<bool> Close()
    {
        var prompt = Settings.App.PromptSaveChanges;

        if (!prompt || !IsChanged)
        {
            Navigator.Close(this);
            return true;
        }

        var answer = await UserPromptBuilder.Configure().SaveChangesPrompt(Title, prompt, SaveCommand).Show();
        await Settings.App.SaveAsync(s => s.PromptSaveChanges = prompt);
        if (answer != MessageBoxResult.Yes) return false;

        Navigator.Close(this);
        return true;
    }

    protected virtual bool CanClose() => !IsChanged;

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
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Task Navigate() => Navigator.Navigate(() => this);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is PageViewModel model && Equals(Route, model.Route);
    }

    /// <inheritdoc />
    public bool Equals(PageViewModel? other)
    {
        return other is not null && Equals(Route, other.Route);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Route.GetHashCode();

    public static bool operator ==(PageViewModel first, PageViewModel second) => Equals(first, second);
    public static bool operator !=(PageViewModel first, PageViewModel second) => !Equals(first, second);
}