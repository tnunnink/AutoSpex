using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class RunResultsPageModel(RunObserver run) : PageViewModel
{
    public override string Route => $"Run/{Run.Id}/{Title}";
    public override string Title => "Results";

    [ObservableProperty] private RunObserver _run = run;

    [ObservableProperty] private string? _filter;
    public ObservableCollection<OutcomeObserver> Outcomes { get; } = [];
    public ObservableCollection<OutcomeObserver> Selected { get; } = [];

    protected override void OnActivated()
    {
        base.OnActivated();
        UpdateOutcomes();
    }

    /// <inheritdoc />
    public override async Task<Result> Save() => await Mediator.Send(new SaveRun(Run));

    /// <summary>
    /// Handles adding the provided node to this <see cref="Run"/>. This will add all descendant nodes if it is a
    /// container node.
    /// </summary>
    /// <param name="observer">The node to add.</param>
    public void AddNode(NodeObserver observer)
    {
        //Adds to underlying outcomes which contains both sources and specs.
        Run.AddNode(observer);
        UpdateOutcomes(Filter);
    }

    /// <summary>
    /// When the filter text changes, update the <see cref="Outcomes"/> collection.
    /// </summary>
    partial void OnFilterChanged(string? value)
    {
        UpdateOutcomes(value);
    }

    /// <summary>
    /// Updates the public <see cref="Outcomes"/> collection with either specs or sources based on the feature and filter text. 
    /// </summary>
    private void UpdateOutcomes(string? filter = default)
    {
        var filtered = Run.Outcomes.Where(n => n.Filter(filter));
        Outcomes.Refresh(filtered);
    }
}