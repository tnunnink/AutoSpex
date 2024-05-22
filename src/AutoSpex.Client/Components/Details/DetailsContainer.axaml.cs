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
    private ICommand? _addCollectionCommand;
    private ICommand? _addSpecCommand;
    private ICommand? _addSourceCommand;
    private ICommand? _addRunnerCommand;
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

    public static readonly DirectProperty<DetailsContainer, ICommand?> AddCollectionCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(AddCollectionCommand), o => o.AddCollectionCommand, (o, v) => o.AddCollectionCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> AddSpecCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(AddSpecCommand), o => o.AddSpecCommand, (o, v) => o.AddSpecCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> AddSourceCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(AddSourceCommand), o => o.AddSourceCommand, (o, v) => o.AddSourceCommand = v);

    public static readonly DirectProperty<DetailsContainer, ICommand?> AddRunnerCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(AddRunnerCommand), o => o.AddRunnerCommand, (o, v) => o.AddRunnerCommand = v);

    public ObservableCollection<DetailPageModel> PagesSource => (ItemsSource as ObservableCollection<DetailPageModel>)!;

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public ICommand? AddCollectionCommand
    {
        get => _addCollectionCommand;
        set => SetAndRaise(AddCollectionCommandProperty, ref _addCollectionCommand, value);
    }

    public ICommand? AddSpecCommand
    {
        get => _addSpecCommand;
        set => SetAndRaise(AddSpecCommandProperty, ref _addSpecCommand, value);
    }

    public ICommand? AddSourceCommand
    {
        get => _addSourceCommand;
        set => SetAndRaise(AddSourceCommandProperty, ref _addSourceCommand, value);
    }

    public ICommand? AddRunnerCommand
    {
        get => _addRunnerCommand;
        set => SetAndRaise(AddRunnerCommandProperty, ref _addRunnerCommand, value);
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
        return new DetailTab();
    }

    private async Task CloseTab(DetailPageModel? page)
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