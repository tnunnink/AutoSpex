using System.Collections.ObjectModel;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class NavigationMenus : TemplatedControl
{
    private ObservableCollection<PageViewModel> _pages = [];
    private PageViewModel? _selectedPage;
    
    public static readonly DirectProperty<NavigationMenus, ObservableCollection<PageViewModel>> PagesProperty =
        AvaloniaProperty.RegisterDirect<NavigationMenus, ObservableCollection<PageViewModel>>(
            nameof(Pages), o => o.Pages, (o, v) => o.Pages = v);
    
    public static readonly DirectProperty<NavigationMenus, PageViewModel?> SelectedPageProperty =
        AvaloniaProperty.RegisterDirect<NavigationMenus, PageViewModel?>(
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