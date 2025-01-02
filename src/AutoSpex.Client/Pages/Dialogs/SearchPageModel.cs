using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SearchPageModel : PageViewModel
{
    private readonly ObserverCollection<Criterion, ReplaceObserver> _criteria = [];

    public SearchPageModel(NodeObserver? scope = default)
    {
        Scope = scope;
    }

    [ObservableProperty] private NodeObserver? _scope;

    [ObservableProperty] private bool _showReplace = true;

    [ObservableProperty] private string? _searchText;

    [ObservableProperty] private string? _replaceText;

    [ObservableProperty] private ReplaceObserver? _selected;
    public ObservableCollection<ReplaceObserver> Instances { get; } = [];
    public Task<NodeSelectorPageModel> NodeSelector => Navigator.Navigate(() => new NodeSelectorPageModel(UpdateScope));

    /// <inheritdoc />
    public override async Task Load()
    {
        var nodes = (await Mediator.Send(new LoadAllNodes())).ToList();
        var criteria = nodes.SelectMany(n => n.Spec.GetAllCriteria(), (n, c) => new ReplaceObserver(c, n));
        _criteria.BindReadOnly(criteria.ToList());
        RegisterDisposable(_criteria);
    }

    [RelayCommand]
    private void Search()
    {
        FilterInstances(SearchText, Scope);
    }

    [RelayCommand]
    private async Task Replace(ReplaceObserver? observer)
    {
        if (observer is null || string.IsNullOrEmpty(SearchText)) return;

        var search = SearchText;
        var replace = ReplaceText ?? string.Empty;
        var result = observer.Replace(search, replace);
        if (Notifier.ShowIfFailed(result)) return;

        var saved = await Mediator.Send(new SaveSpec(observer.Node));
        Notifier.ShowIfFailed(saved);

        Search();
    }

    [RelayCommand]
    private async Task ReplaceAll()
    {
        if (string.IsNullOrEmpty(SearchText)) return;
        

        foreach (var instance in Instances)
        {
            var search = SearchText;
            var replace = ReplaceText ?? string.Empty;
            var result = instance.Replace(search, replace);
            if (Notifier.ShowIfFailed(result)) return;
        }

        var nodes = Instances.Select(x => x.Node).DistinctBy(n => n.NodeId);
        var saved = await Mediator.Send(new SaveNodes(nodes));
        Notifier.ShowIfFailed(saved);

        Search();
    }

    [RelayCommand]
    private async Task OpenSpec(Window window)
    {
        if (Selected is null) return;
        var node = new NodeObserver(Selected.Node);
        await Navigator.Navigate(node);
        window.Close();
    }

    /// <summary>
    /// As the entered search text changes, trigger the query to update the instance collection with criterion that
    /// match the search text.
    /// </summary>
    partial void OnSearchTextChanged(string? value)
    {
        FilterInstances(value, Scope);
    }

    /// <summary>
    /// Update the selected node scope and retrigger the filtering of the instances.
    /// </summary>
    private void UpdateScope(NodeObserver? node)
    {
        Scope = node;
        FilterInstances(SearchText, Scope);
    }

    /// <summary>
    /// Filter the current criterion instance based on the provided scope and filter text.
    /// </summary>
    private void FilterInstances(string? filter, NodeObserver? scope)
    {
        //Only want to show results when the user enters text.
        if (string.IsNullOrEmpty(filter))
        {
            Instances.Clear();
            return;
        }

        _criteria.Filter(x => (scope is null || x.Node.IsDescendantOf(scope)) && x.Filter(filter));
        Instances.Refresh(_criteria);
    }
}