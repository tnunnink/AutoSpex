using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace AutoSpex.Client.Shared;

public abstract partial class PageViewModel : TrackableViewModel, IEquatable<PageViewModel>
{
    /// <summary>
    /// A resource identifier that identifies this page relative to any other. The default implementation is this
    /// type name but derived classes can override to indicate how a given page should be identified.
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
    /// 
    /// </summary>
    /// <returns></returns>
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