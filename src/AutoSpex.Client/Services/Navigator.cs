using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Navigator(IMessenger messenger) : IDisposable
{
    /// <summary>
    /// The locally maintained set of open page references.
    /// </summary>
    private readonly Dictionary<string, PageViewModel> _openPages = [];

    /// <summary>
    /// A default instance of the <see cref="Navigator"/> service which uses the <see cref="WeakReferenceMessenger"/>
    /// as the internal messenger for sending the <see cref="NavigationRequest"/> message.
    /// </summary>
    public static Navigator Default => new(WeakReferenceMessenger.Default);

    /// <summary>
    /// Performs navigation of the <see cref="HomePageModel"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the async function with the page result.</returns>
    public Task<HomePageModel> NavigateHome()
    {
        return OpenPage(Container.Resolve<HomePageModel>);
    }

    /// <summary>
    /// Performs navigation of the <see cref="PageViewModel"/> specified using the generic type parameter. This method
    /// will use the application <see cref="Container"/> to resolve the page specified.
    /// </summary>
    /// <param name="action">The optional navigation action to perform.</param>
    /// <typeparam name="TPage">The page view model type to navigate.</typeparam>
    /// <returns>The Task representing the async function with the page view model result.</returns>
    public Task<TPage> Navigate<TPage>(NavigationAction action = NavigationAction.Replace) where TPage : PageViewModel
    {
        return OpenPage(Container.Resolve<TPage>);
    }

    public Task<TPage> Navigate<TPage>(Func<TPage> factory, NavigationAction action = NavigationAction.Replace)
        where TPage : PageViewModel
    {
        return OpenPage(factory);
    }

    public Task<PageViewModel> Navigate<TObserver>(TObserver observer,
        NavigationAction action = NavigationAction.Replace)
        where TObserver : ITrackable
    {
        return OpenPage(GetPageResolver(observer), action);
    }

    /// <summary>
    /// Closes the provided <see cref="PageViewModel"/> by issuing the close cation navigation request message. 
    /// </summary>
    /// <param name="page">The page to close.</param>
    public void Close(PageViewModel page) => ClosePage(page);

    /// <summary>
    /// Disposes/Deactivates all pages current open in this instance of the <see cref="Navigator"/> service.
    /// </summary>
    public void Dispose()
    {
        foreach (var page in _openPages.Values)
        {
            page.IsActive = false;
        }

        _openPages.Clear();
    }

    /// <summary>
    /// Opens a page using the provided page factory and navigation action. This will resolve the page and use the open
    /// instance if found (allowing the created instance to be garbage collected). It will then create the
    /// <see cref="NavigationRequest"/> message and send that out. The owning pages responsible for displaying this
    /// page should receive and render the page. Once rendered this service will await the Activation/Loading of the
    /// page to initialize it's state. Finally it will clean up inactive pages from memory and then return the active
    /// page to the caller of the navigation request.
    /// </summary>
    private async Task<TPage> OpenPage<TPage>(Func<TPage> factory, NavigationAction action = NavigationAction.Open)
        where TPage : PageViewModel
    {
        var page = ResolvePage(factory);
        var request = new NavigationRequest(page, action);
        messenger.Send(request);
        await ActivatePage(page);
        CleanUp();
        return page;
    }

    private async Task<PageViewModel> OpenPage(Func<PageViewModel> factory,
        NavigationAction action = NavigationAction.Open)
    {
        var page = ResolvePage(factory);
        var request = new NavigationRequest(page, action);
        messenger.Send(request);
        await ActivatePage(page);
        CleanUp();
        return page;
    }

    private void ClosePage(PageViewModel page)
    {
        var request = new NavigationRequest(page, NavigationAction.Close);
        messenger.Send(request);
        _openPages.Remove(page.Route);
        page.IsActive = false;
    }

    private TPage ResolvePage<TPage>(Func<TPage> factory) where TPage : PageViewModel
    {
        var page = factory();

        if (!_openPages.TryAdd(page.Route, page))
        {
            page = (TPage) _openPages[page.Route];
        }

        return page;
    }

    private static async Task ActivatePage(PageViewModel page)
    {
        if (page.IsActive) return;
        page.IsActive = true;
        await page.Load();
    }

    private void CleanUp()
    {
        var deactivated = _openPages.Where(p => !p.Value.IsActive).Select(p => p.Key).ToList();
        foreach (var page in deactivated)
        {
            _openPages.Remove(page);
        }
    }

    private static Func<PageViewModel> GetPageResolver(ITrackable observer)
    {
        return observer switch
        {
            ProjectObserver project => () => new ProjectPageModel(project),
            NodeObserver node when node.NodeType == NodeType.Collection => () => new CollectionPageModel(node),
            NodeObserver node when node.NodeType == NodeType.Folder => () => new FolderPageModel(node),
            NodeObserver node when node.NodeType == NodeType.Spec => () => new SpecPageModel(node),
            SourceObserver source => () => new SourcePageModel(source.Id),
            _ => throw new NotSupportedException($"The observer type {observer.GetType()} does not support navigation")
        };
    }
}

/// <summary>
/// A common message to be sent via the application <see cref="IMessenger"/> to navigate (open/close) a specific page
/// in the application. 
/// </summary>
/// <param name="page">The page to navigate.</param>
/// <param name="action">The navigation action to perform.</param>
public class NavigationRequest(PageViewModel page, NavigationAction action = NavigationAction.Open)
{
    /// <summary>
    /// The instance of the <see cref="PageViewModel"/> to navigate for this request.
    /// </summary>
    public PageViewModel Page { get; } = page ?? throw new ArgumentNullException(nameof(page));

    /// <summary>
    /// The <see cref="NavigationAction"/> to perform for this request.
    /// </summary>
    public NavigationAction Action { get; } = action;
}

/// <summary>
/// An enumeration of navigation actions to perform.
/// </summary>
public enum NavigationAction
{
    Open,
    Close,
    Replace
}