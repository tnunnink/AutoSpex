using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class RunConfigPageModel(RunObserver run) : PageViewModel
{
    public override string Route => $"Run/{Run.Id}/{Title}";
    public override string Title => "Config";

    [ObservableProperty] private RunObserver _run = run;

    [ObservableProperty] private string? _sourceFilter;

    [ObservableProperty] private string? _specFilter;

    public ObservableCollection<NodeObserver> Sources { get; } = [];
    public ObservableCollection<NodeObserver> SelectedSources { get; } = [];
    public ObservableCollection<NodeObserver> Specs { get; } = [];
    public ObservableCollection<NodeObserver> SelectedSpecs { get; } = [];

    protected override void OnActivated()
    {
        base.OnActivated();
        UpdateSources();
        UpdateSpecs();
    }

    /// <summary>
    /// Handles adding the provided node to this <see cref="Run"/>. This will add all descendant nodes if it is a
    /// container node.
    /// </summary>
    /// <param name="observer">The node to add.</param>
    public void AddNode(NodeObserver observer)
    {
        //Adds to underlying nodes which contains both sources and specs.
        Run.AddNode(observer);

        if (observer.Feature == NodeType.Source)
            UpdateSources(SourceFilter);

        if (observer.Feature == NodeType.Spec)
            UpdateSpecs(SpecFilter);
    }

    /// <summary>
    /// When the filter text changes, update the <see cref="Sources"/> collection.
    /// </summary>
    partial void OnSourceFilterChanged(string? value)
    {
        UpdateSources(value);
    }

    /// <summary>
    /// When the filter text changes, update the <see cref="Specs"/> collection.
    /// </summary>
    partial void OnSpecFilterChanged(string? value)
    {
        UpdateSpecs(value);
    }

    /// <summary>
    /// Updates the public <see cref="Specs"/> collection with either specs or sources based on the feature and filter text. 
    /// </summary>
    private void UpdateSources(string? filter = default)
    {
        var nodes = Run.Nodes.Where(n => n.Type == NodeType.Source && n.Filter(filter));
        Sources.Refresh(nodes);
    }

    /// <summary>
    /// Updates the public <see cref="Specs"/> collection with either specs or sources based on the feature and filter text. 
    /// </summary>
    private void UpdateSpecs(string? filter = default)
    {
        var nodes = Run.Nodes.Where(n => n.Type == NodeType.Spec && n.Filter(filter));
        Specs.Refresh(nodes);
    }
}