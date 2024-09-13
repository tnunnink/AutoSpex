using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using MediatR;

namespace AutoSpex.Client.Shared;

public abstract partial class PageViewModel() : TrackableViewModel, IEquatable<PageViewModel>
{
    protected PageViewModel(string title) : this()
    {
        Title = title;
    }
    
    /// <summary>
    /// A resource identifier that identifies this page relative to any other. The default implementation is this
    /// type name but derived classes can override to indicate how a given page should be identified.
    /// </summary>
    public virtual string Route => GetType().Name;

    /// <summary>
    /// The name of the page that can be used to display in a tab or other control.
    /// </summary>
    [ObservableProperty] private string _title = "Title";

    /// <summary>
    /// The string key of the icon that this page corresponds to.
    /// </summary>
    public virtual string Icon => "None";

    /// <summary>
    /// Gets or sets a value indicating whether the page should be kept alive in the navigation stack.
    /// </summary>
    /// <value>
    /// <c>true</c> if the page should be kept alive; otherwise, <c>false</c>.
    /// </value>
    public virtual bool KeepAlive => true;

    /// <summary>
    /// A command to initiate the loading of data from external resources in order to populate this page with it's
    /// required content.
    /// </summary>
    /// <returns>The <see cref="Task"/> for loading the data.</returns>
    /// <remarks>Since most pages will need to perform some initialization by sending a request to fetch data via
    /// our <see cref="Mediator"/> we will provide this command by default with no implementation. Deriving pages can
    /// implement this load <see cref="RelayCommand"/> which will be awaited as the pages are navigated to using out
    /// application navigator service.
    /// </remarks>
    [RelayCommand]
    public virtual Task Load() => Task.CompletedTask;

    /// <summary>
    /// A command to initiate a save of the current state of the page.
    /// </summary>
    /// <returns>The <see cref="Task"/> which can await the <see cref="Result"/> of the save command.</returns>
    /// <remarks>
    /// Some pages will need the ability to save changes to the database by sending some command through the
    /// <see cref="Mediator"/> service. This command by default has no implementation. Deriving classes will implement
    /// this as needed. Each derived class can await the base implementation to get the result before processing further.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanSave))]
    public virtual Task<Result> Save() => Task.FromResult(Result.Ok());

    /// <summary>
    /// Indicates whether the page can be saved or not. By default, this returns true if <c>IsChanged</c> is true
    /// and <c>IsErrored</c> is false, but deriving classes can override to specify different functionality.
    /// </summary>
    /// <returns><c>true</c> if the page can be saved, Otherwise, <c>false</c>.</returns>
    public virtual bool CanSave() => IsChanged && !IsErrored;

    /// <inheritdoc />
    protected override Task Navigate() => Navigator.Navigate(() => this);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is PageViewModel model && Equals(Route, model.Route);
    }

    /// <inheritdoc />
    public bool Equals(PageViewModel? other) => other is not null && Equals(Route, other.Route);

    /// <inheritdoc />
    public override int GetHashCode() => Route.GetHashCode();

    public static bool operator ==(PageViewModel first, PageViewModel second) => Equals(first, second);
    public static bool operator !=(PageViewModel first, PageViewModel second) => !Equals(first, second);
}