using System.Collections.ObjectModel;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;

namespace AutoSpex.Client.Components;

public class ProjectNavMenu : ContentControl
{
    private ObservableCollection<PageViewModel> _pages = [];
    private PageViewModel? _selectedPage;
    
    public static readonly DirectProperty<ProjectNavMenu, ObservableCollection<PageViewModel>> PagesProperty =
        AvaloniaProperty.RegisterDirect<ProjectNavMenu, ObservableCollection<PageViewModel>>(
            nameof(Pages), o => o.Pages, (o, v) => o.Pages = v);
    
    public static readonly DirectProperty<ProjectNavMenu, PageViewModel?> SelectedPageProperty =
        AvaloniaProperty.RegisterDirect<ProjectNavMenu, PageViewModel?>(
            nameof(SelectedPage), o => o.SelectedPage, (o, v) => o.SelectedPage = v);
    
    public ObservableCollection<PageViewModel> Pages
    {
        get => _pages;
        set => SetAndRaise(PagesProperty, ref _pages, value);
    }

    public PageViewModel? SelectedPage
    {
        get => _selectedPage;
        set => SetAndRaise(SelectedPageProperty, ref _selectedPage, value);
    }
}