using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class ProjectPageModel : PageViewModel
{
    public ProjectPageModel(ProjectObserver project)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));

        NavigationPages = [new SpecsPageModel(), new SourcesPageModel()];
        SelectedNavigationPage = NavigationPages.First();
        DetailsPage = new DetailsPageModel();
    }
    
    [ObservableProperty] private ProjectObserver _project;
    
    [ObservableProperty] private ObservableCollection<PageViewModel> _navigationPages;
    
    [ObservableProperty] private PageViewModel _selectedNavigationPage;
    
    [ObservableProperty] private PageViewModel _detailsPage;
    
    //todo source list
    //todo runner list
    //todo details page

    public override string Route => $"{nameof(ProjectPageModel)}{Project.Directory}/{Project.Name}";

    public override bool IsChanged => DetailsPage.IsChanged;

    public override void AcceptChanges()
    {
        DetailsPage.AcceptChanges();
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        //todo we want to create a watcher for the project file so that if it is inadvertently removed we can respond to that in the UI.
    }
}