using System;
using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Projects;

[UsedImplicitly]
public partial class ProjectPageModel(ProjectObserver project) : PageViewModel, 
    IRecipient<NavigationRequest<SpecsPageModel>>,
    IRecipient<NavigationRequest<SourcesPageModel>>,
    IRecipient<NavigationRequest<DetailsPageModel>>
{
    [ObservableProperty] private ProjectObserver _project = project ?? throw new ArgumentNullException(nameof(project));

    [ObservableProperty] private ObservableCollection<PageViewModel> _menus = [];

    [ObservableProperty] private PageViewModel? _selectedMenu;

    [ObservableProperty] private PageViewModel? _detailsPage;

    public override string Route => $"{nameof(ProjectPageModel)}{Project.Directory}/{Project.Name}";

    public override bool IsChanged => DetailsPage?.IsChanged ?? false;

    public override void AcceptChanges()
    {
        DetailsPage?.AcceptChanges();
    }

    public override async Task Load()
    {
        await Navigator.Navigate(() => new SpecsPageModel());
        await Navigator.Navigate(() => new SourcesPageModel());
        await Navigator.Navigate(() => new DetailsPageModel());
    }

    public void Receive(NavigationRequest<SpecsPageModel> message)
    {
        if (!Menus.Contains(message.Page))
            Menus.Add(message.Page);
        
        SelectedMenu ??= message.Page;
        
        message.Reply(Result.Ok());
    }
    
    public void Receive(NavigationRequest<SourcesPageModel> message)
    {
        if (!Menus.Contains(message.Page))
            Menus.Add(message.Page);

        message.Reply(Result.Ok());
    }
    
    public void Receive(NavigationRequest<DetailsPageModel> message)
    {
        DetailsPage = message.Page;
        message.Reply(Result.Ok());
    }
}