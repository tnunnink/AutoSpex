using System.Collections.ObjectModel;
using AutoSpex.Client.Resources.Controls;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AutoSpex.Client.Components;

public class NavigationMenus : TemplatedControl
{
    public static readonly DirectProperty<NavigationMenus, ObservableCollection<PageViewModel>> PagesProperty =
        AvaloniaProperty.RegisterDirect<NavigationMenus, ObservableCollection<PageViewModel>>(
            nameof(Pages), o => o.Pages, (o, v) => o.Pages = v);

    public static readonly DirectProperty<NavigationMenus, PageViewModel?> SelectedPageProperty =
        AvaloniaProperty.RegisterDirect<NavigationMenus, PageViewModel?>(
            nameof(SelectedPage), o => o.SelectedPage, (o, v) => o.SelectedPage = v,
            defaultBindingMode: BindingMode.TwoWay);

    private ObservableCollection<PageViewModel> _pages = [];
    private PageViewModel? _selectedPage;
    private TabControl? _tabControl;

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterTabControl(e);
    }

    private void RegisterTabControl(TemplateAppliedEventArgs args)
    {
        _tabControl?.RemoveHandler(PointerPressedEvent, OnTabControlPointerPressed);
        _tabControl = args.NameScope.Get<TabControl>("TabControl");
        _tabControl.AddHandler(PointerPressedEvent, OnTabControlPointerPressed, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Shows or hides the parent drawer when the user clicks the selected tab item.
    /// </summary>
    private void OnTabControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control {DataContext: PageViewModel page} control) return;
        if (control.FindLogicalAncestorOfType<TabItem>() is null) return;

        var drawer = this.FindLogicalAncestorOfType<DrawerView>();
        if (drawer is null) return;

        //If the clicked page is the selected page then toggle the drawer.
        if (page.Equals(SelectedPage))
        {
            drawer.IsDrawerOpen = !drawer.IsDrawerOpen;
            return;
        }

        //Otherwise ensure it is open as we clicked a different tab.
        drawer.IsDrawerOpen = true;
    }
}