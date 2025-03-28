using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Shared;
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
    /// Performs navigation of the <see cref="PageViewModel"/> specified using the generic type parameter. This method
    /// will use the application <see cref="Registrar"/> to resolve the page specified.
    /// </summary>
    /// <param name="action">The optional navigation action to perform.</param>
    /// <typeparam name="TPage">The page view model type to navigate.</typeparam>
    /// <returns>The page that was created and navigated.</returns>
    public Task<TPage> Navigate<TPage>(NavigationAction action = NavigationAction.Open) where TPage : PageViewModel
    {
        return OpenPage(Registrar.Resolve<TPage>);
    }

    /// <summary>
    /// Performs navigation of the page type specified in the provided factory.
    /// </summary>
    /// <param name="factory">The function that creates the page to navigate.</param>
    /// <param name="action">The optional navigation action to perform.</param>
    /// <typeparam name="TPage">The page view model type to navigate.</typeparam>
    /// <returns>The page that was created and navigated.</returns>
    public Task<TPage> Navigate<TPage>(Func<TPage> factory, NavigationAction action = NavigationAction.Open)
        where TPage : PageViewModel
    {
        return OpenPage(factory);
    }

    /// <summary>
    /// Performs navigation to a page mapping the provided observer. This service will map internally the page
    /// corresponding to the provided observer.
    /// </summary>
    /// <param name="observer">The observer type to navigate.</param>
    /// <param name="action">The optional navigation action to perform.</param>
    /// <typeparam name="TObserver">The page view model type to navigate.</typeparam>
    /// <returns>The page that was created and navigated.</returns>
    public Task<PageViewModel> Navigate<TObserver>(TObserver observer, NavigationAction action = NavigationAction.Open)
        where TObserver : ITrackable
    {
        return OpenPage(GetPageResolver(observer), action);
    }

    /// <summary>
    /// Opens a page specified by the route and returns the page of type TPage if found or resolved.
    /// If the page is not found or resolved, an InvalidOperationException is thrown.
    /// </summary>
    /// <param name="route">The route to identify the page to open.</param>
    /// <typeparam name="TPage">The type of the page view model to open.</typeparam>
    /// <returns>The page of type TPage that was opened.</returns>
    public TPage Open<TPage>(string route) where TPage : PageViewModel
    {
        if (!_openPages.TryGetValue(route, out var match))
            throw new InvalidOperationException($"Could not find page '{route}'");

        if (match is not TPage page)
            throw new InvalidOperationException($"Page '{route}' is not of type {typeof(TPage)}");

        var request = new NavigationRequest(page);
        messenger.Send(request);

        return page;
    }

    /// <summary>
    /// Closes the provided <see cref="PageViewModel"/> by issuing the close cation navigation request message. 
    /// </summary>
    /// <param name="page">The page to close.</param>
    public void Close(PageViewModel? page) => ClosePage(page);

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
    /// <see cref="NavigationRequest"/> message and send that out. Any owning pages responsible for displaying this
    /// page should receive and render the page. Once rendered this service will await the Activation/Loading of the
    /// page to initialize it's state. Finally, it will clean up inactive pages from memory and then return the active
    /// page to the caller of the navigation request in case it is needed to perform further interaction.
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

    /// <summary>
    /// Opens a page using the provided page factory and navigation action. This will resolve the page and use the open
    /// instance if found (allowing the created instance to be garbage collected). It will then create the
    /// <see cref="NavigationRequest"/> message and send that out. Any owning pages responsible for displaying this
    /// page should receive and render the page. Once rendered this service will await the Activation/Loading of the
    /// page to initialize it's state. Finally, it will clean up inactive pages from memory and then return the active
    /// page to the caller of the navigation request in case it is needed to perform further interaction.
    /// </summary>
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


    /// <summary>
    /// Closes the specified <see cref="PageViewModel"/> by sending a navigation request with <see cref="NavigationAction.Close"/>.
    /// The method also removes the page from the open pages dictionary and disposes of the page.
    /// </summary>
    /// <param name="page">The page to be closed.</param>
    private void ClosePage(PageViewModel? page)
    {
        if (page is null) return;
        var request = new NavigationRequest(page, NavigationAction.Close);
        messenger.Send(request);
        _openPages.Remove(page.Route);
        page.Dispose();
    }

    /// <summary>
    /// We resolve the page by calling the provided factory method. We then look at the page route and determine if
    /// we already have an instance of this page open. If so we want to return that and discard the created page.
    /// Note that since we have not called load and our services are not injected then this is not costly. Also, we only
    /// consider pages that are configured with the KeepAlive configured to true. Otherwise, we always create and never
    /// maintain the instance of the page.
    /// </summary>
    private TPage ResolvePage<TPage>(Func<TPage> factory) where TPage : PageViewModel
    {
        var page = factory();

        if (!page.KeepAlive) return page;

        if (!_openPages.TryAdd(page.Route, page))
        {
            page = (TPage)_openPages[page.Route];
        }

        return page;
    }

    /// <summary>
    /// Activation involves setting the IsActive bit true, which calls the OnActivated method for the page, registering
    /// the message handlers, and performing any other custom activation logic. We then await the Load method for the page,
    /// which is where the page requests data from external sources to load it with the content to display. This is what
    /// makes navigation async, and with this method we don't need to worry about calling load in the page constructor.
    /// </summary>
    private static async Task ActivatePage(PageViewModel page)
    {
        switch (page)
        {
            case { IsActive: true, Reload: false }:
                return;
            case { IsActive: true, Reload: true }:
                page.Flush();
                break;
        }

        page.IsActive = true;
        await page.Load();
    }

    /// <summary>
    /// Removes any deactivated page instances from the open page cache.
    /// </summary>
    private void CleanUp()
    {
        var deactivated = _openPages.Where(p => !p.Value.IsActive).Select(p => p.Key).ToList();

        foreach (var page in deactivated)
        {
            _openPages.Remove(page);
        }
    }

    /// <summary>
    /// Resolves the detail page corresponding to the provided trackable observer type.
    /// </summary>
    private static Func<PageViewModel> GetPageResolver(ITrackable observer)
    {
        return observer switch
        {
            NodeObserver node => () => new NodeDetailPageModel(node),
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