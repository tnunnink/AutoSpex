using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

[PublicAPI]
public class DetailsContainer : TabControl
{
    private ICommand? _addCommand;
    private ICommand? _closeTabCommand;
    private ICommand? _closeAllTabsCommand;
    private ICommand? _closeOtherTabsCommand;
    private ICommand? _closeRightTabsCommand;
    private ICommand? _closeLeftTabsCommand;
    private ICommand? _forceCloseTabCommand;
    private ICommand? _forceCloseAllTabsCommand;

    public DetailsContainer()
    {
        CloseTabCommand = new AsyncRelayCommand<DetailPageModel>(CloseTab);
        CloseAllTabsCommand = new AsyncRelayCommand(CloseAllTabs);
        CloseOtherTabsCommand = new AsyncRelayCommand<DetailPageModel>(CloseOtherTabs);
        CloseRightTabsCommand = new AsyncRelayCommand<DetailPageModel>(CloseRightTabs);
        CloseLeftTabsCommand = new AsyncRelayCommand<DetailPageModel>(CloseLeftTabs);
        ForceCloseTabCommand = new RelayCommand<DetailPageModel>(ForceCloseTab);
        ForceCloseAllTabsCommand = new RelayCommand(ForceCloseAllTabs);
    }

    public static readonly DirectProperty<DetailsContainer, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    /*public static readonly DirectProperty<DetailsContainer, ICommand?> CloseTabCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(CloseTabCommand), o => o.CloseTabCommand, (o, v) => o.CloseTabCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> CloseAllTabsCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(CloseAllTabsCommand), o => o.CloseAllTabsCommand, (o, v) => o.CloseAllTabsCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> CloseOtherTabsCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(CloseOtherTabsCommand), o => o.CloseOtherTabsCommand, (o, v) => o.CloseOtherTabsCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> CloseRightTabsCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(CloseRightTabsCommand), o => o.CloseRightTabsCommand, (o, v) => o.CloseRightTabsCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> CloseLeftTabsCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(CloseLeftTabsCommand), o => o.CloseLeftTabsCommand, (o, v) => o.CloseLeftTabsCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> ForceCloseTabCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(ForceCloseTabCommand), o => o.ForceCloseTabCommand, (o, v) => o.ForceCloseTabCommand = v);*/

    /*public static readonly DirectProperty<DetailsContainer, ICommand?> ForceCloseAllTabsCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(ForceCloseAllTabsCommand), o => o.ForceCloseAllTabsCommand, (o, v) => o.ForceCloseAllTabsCommand = v);*/

    public ObservableCollection<DetailPageModel> PagesSource => (ItemsSource as ObservableCollection<DetailPageModel>)!;

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public ICommand? CloseTabCommand { get; }

    public ICommand? CloseAllTabsCommand { get; }

    public ICommand? CloseOtherTabsCommand { get; }

    public ICommand? CloseRightTabsCommand { get; }

    public ICommand? CloseLeftTabsCommand { get; }

    public ICommand? ForceCloseTabCommand { get; }

    public ICommand? ForceCloseAllTabsCommand { get; }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DetailsTab();
    }

    private async Task CloseTab(PageViewModel? page)
    {
        if (page is null) return;
        await page.Close();
    }

    private async Task CloseAllTabs()
    {
        var pages = PagesSource.ToList();

        foreach (var page in pages)
        {
            await page.Close();
        }

        pages.Clear();
    }

    private async Task CloseOtherTabs(PageViewModel? page)
    {
        if (page is null) return;

        var pages = PagesSource.Where(p => p != page).ToList();

        foreach (var closable in pages)
        {
            await closable.Close();
        }

        pages.Clear();
    }

    private async Task CloseRightTabs(DetailPageModel? page)
    {
        if (page is null) return;

        var pages = PagesSource.ToList();
        var start = pages.IndexOf(page) + 1;

        for (var i = start; i < pages.Count; i++)
        {
            var closable = pages[i];
            await closable.Close();
        }
    }

    private async Task CloseLeftTabs(DetailPageModel? page)
    {
        if (page is null) return;

        var pages = PagesSource.ToList();
        var start = pages.IndexOf(page) - 1;

        for (var i = start; i >= 0; i--)
        {
            var closable = pages[i];
            await closable.Close();
        }
    }

    private void ForceCloseTab(DetailPageModel? page)
    {
        if (page is null) return;
        page.ForceCloseCommand.Execute(null);
        PagesSource.Remove(page);
    }

    private void ForceCloseAllTabs()
    {
        var pages = PagesSource.ToList();

        foreach (var page in pages)
        {
            page.ForceCloseCommand.Execute(null);
        }

        pages.Clear();
    }
}