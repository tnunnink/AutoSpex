using System;
using System.ComponentModel;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace AutoSpex.Client.Shared;

[ObservableRecipient]
public abstract partial class PageViewModel : ObservableValidator, IChangeTracking, IEquatable<PageViewModel>
{
    //Instead of DI I'm resolving dependencies for the pages directly.
    //I am choosing to do this to simplify the construction and since I am never going to do mocking,
    //but rather using the real implementations to preform integration testing for my application pages.
    protected readonly Shell Shell = Container.Resolve<Shell>();
    protected readonly IMediator Mediator = Container.Resolve<IMediator>();
    protected readonly Navigator Navigator = Container.Resolve<Navigator>();

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
    /// An indication that the page contains data which has been updated and not yet saved.
    /// </summary>
    public virtual bool IsChanged => false;

    /// <summary>
    /// A method that accepts changes to the current state of the page prior to saving.
    /// </summary>
    public virtual void AcceptChanges()
    {
    }

    /// <summary>
    /// A command to initiate the loading of data from external resources in order to populate this page with it's
    /// required content.
    /// </summary>
    /// <returns>The <see cref="Task"/> for loading the data.</returns>
    /// <remarks>Since most pages will needed to perform some initialization by sending a request to fetch data via
    /// our <see cref="Mediator"/> we will provide this command by default with no implementation. Deriving pages can
    /// implement this load <see cref="RelayCommand"/> which can be trigger from the UI on the event loaded using
    /// interactions.</remarks>
    [RelayCommand]
    public virtual Task Load() => Task.CompletedTask;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public virtual Task Save() => Task.CompletedTask;

    protected virtual bool CanSave() => true;

    [RelayCommand(CanExecute = nameof(CanClose))]
    public virtual void Close()
    {
        Navigator.Remove(this);
        IsActive = false;
    }

    protected virtual bool CanClose() => true;

    [RelayCommand]
    public virtual Task ForceClose() => Task.CompletedTask;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is PageViewModel model && Equals(Route, model.Route);
    }

    public bool Equals(PageViewModel? other)
    {
        return other is not null && Equals(Route, other.Route);
    }

    public override int GetHashCode() => Route.GetHashCode();
}