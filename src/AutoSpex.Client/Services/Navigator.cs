using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Navigator(IMessenger messenger)
{
    private PageViewModel? _currentPage;

    public Task NavigateHome()
    {
        return PerformNavigation(Container.Resolve<HomePageModel>, nameof(ShellViewModel));
    }

    public Task NavigateProject(ProjectObserver project)
    {
        return PerformNavigation(() => new ProjectPageModel(project), nameof(ShellViewModel));
    }

    public Task Navigate<TPageModel>(params KeyValuePair<string, object>[] parameters) where TPageModel : PageViewModel
    {
        var dictionary = parameters.Length > 0 ? parameters.ToDictionary(p => p.Key, p => p.Value) : default;
        return PerformNavigation(Container.Resolve<TPageModel>, nameof(ShellViewModel), dictionary);
    }

    public Task Navigate(Func<PageViewModel> factory, params KeyValuePair<string, object>[] parameters)
    {
        var dictionary = parameters.Length > 0 ? parameters.ToDictionary(p => p.Key, p => p.Value) : default;
        return PerformNavigation(factory, nameof(ShellViewModel), dictionary);
    }

    public Task NavigateTo<TPage, TOwner>(params KeyValuePair<string, object>[] parameters) where TPage : PageViewModel
    {
        var dictionary = parameters.Length > 0 ? parameters.ToDictionary(p => p.Key, p => p.Value) : default;
        return PerformNavigation(Container.Resolve<TPage>, typeof(TOwner).Name, dictionary);
    }

    public Task NavigateTo<TOwner>(Func<PageViewModel> factory, params KeyValuePair<string, object>[] parameters)
    {
        var dictionary = parameters.Length > 0 ? parameters.ToDictionary(p => p.Key, p => p.Value) : default;
        return PerformNavigation(factory, typeof(TOwner).Name, dictionary);
    }

    private async Task PerformNavigation(Func<PageViewModel> factory, string owner,
        Dictionary<string, object>? parameters = null)
    {
        if (_currentPage is not null && _currentPage.IsChanged)
        {
            //todo prompt
            return;
        }

        if (_currentPage is not null)
            _currentPage.IsActive = false;

        var page = factory();

        var request = new NavigationRequest(page, parameters);
        var result = await messenger.Send(request, owner);

        if (result.IsSuccess)
        {
            _currentPage = page;
            _currentPage.IsActive = true;
        }
    }
}

public class NavigationRequest(PageViewModel page, Dictionary<string, object>? parameters = null)
    : AsyncRequestMessage<Result>
{
    public PageViewModel Page { get; } = page ?? throw new ArgumentNullException(nameof(page));

    public Dictionary<string, object> Parameters { get; } = parameters ?? [];
}