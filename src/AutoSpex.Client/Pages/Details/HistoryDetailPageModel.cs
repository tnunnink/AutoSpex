using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class HistoryDetailPageModel() : DetailPageModel("History")
{
    public ObserverCollection<Run, RunObserver> Runs { get; } = [];

    public ObservableCollection<RunObserver> Selected { get; } = [];

    public override async Task Load()
    {
        var runs = await Mediator.Send(new ListRuns());
        Runs.Bind(runs.ToList(), r => new RunObserver(r));
        RegisterDisposable(Runs);
    }

    protected override void FilterChanged(string? filter)
    {
        Runs.Filter(filter);
    }
}