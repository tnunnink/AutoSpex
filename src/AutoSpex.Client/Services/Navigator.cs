using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;
using JetBrains.Annotations;
using HomePageModel = AutoSpex.Client.Pages.Home.HomePageModel;
using ProjectPageModel = AutoSpex.Client.Pages.Projects.ProjectPageModel;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Navigator(IMessenger messenger) : IDisposable
{
    private readonly Dictionary<string, PageViewModel> _openPages = [];

    public Task NavigateHome()
    {
        return OpenPage(Container.Resolve<HomePageModel>);
    }

    public Task NavigateProject(ProjectObserver project)
    {
        return OpenPage(() => new ProjectPageModel(project));
    }

    public Task Navigate<TPage>(params KeyValuePair<string, object>[] parameters)
        where TPage : PageViewModel
    {
        var dictionary = parameters.Length > 0 ? parameters.ToDictionary(p => p.Key, p => p.Value) : default;
        return OpenPage(Container.Resolve<TPage>, dictionary);
    }

    public Task Navigate<TPage>(Func<TPage> factory, params KeyValuePair<string, object>[] parameters)
        where TPage : PageViewModel
    {
        var dictionary = parameters.Length > 0 ? parameters.ToDictionary(p => p.Key, p => p.Value) : default;
        return OpenPage(factory, dictionary);
    }

    public void Remove(PageViewModel page)
    {
        _openPages.Remove(page.Route);
    }

    public void Dispose()
    {
        foreach (var page in _openPages.Values)
        {
            page.IsActive = false;
        }

        _openPages.Clear();
    }

    private async Task OpenPage<TPage>(Func<TPage> factory, Dictionary<string, object>? parameters = null)
        where TPage : PageViewModel
    {
        var page = ResolvePage(factory);

        var request = new NavigationRequest<TPage>(page, parameters);

        try
        {
            await messenger.Send(request);
        }
        catch (Exception)
        {
            //todo send ui notification perhaps. definitely log. and yeah this should not happen if properly setup.
            Console.WriteLine(
                $"No owner is registered to receive navigation requests for page with route {page.Route}");
            throw;
        }

        await ActivatePage(page);
        CleanUp();
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
}

public class NavigationRequest<TPage>(TPage page, Dictionary<string, object>? parameters = null)
    : AsyncRequestMessage<Result> where TPage : PageViewModel
{
    public TPage Page { get; } = page ?? throw new ArgumentNullException(nameof(page));
    public Dictionary<string, object> Parameters { get; } = parameters ?? [];
}