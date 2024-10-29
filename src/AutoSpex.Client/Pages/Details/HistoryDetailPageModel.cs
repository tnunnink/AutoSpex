using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class HistoryDetailPageModel() : DetailPageModel("History"), IRecipient<Observer.GetSelected>
{
    public override string Route => Title;
    public override string Icon => Title;
    public ObserverCollection<Run, RunObserver> Runs { get; } = [];
    public ObservableCollection<RunObserver> Selected { get; } = [];

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    public int Total => Runs.Count;
    public int Passed => Runs.Count(x => x.Result == ResultState.Passed);
    public int Failed => Runs.Count(x => x.Result == ResultState.Failed);
    public int Errored => Runs.Count(x => x.Result == ResultState.Errored);
    public int Inconclusive => Runs.Count(x => x.Result == ResultState.Inconclusive);

    public override async Task Load()
    {
        var runs = await Mediator.Send(new ListRuns());
        Runs.Bind(runs.ToList(), r => new RunObserver(r));
        RegisterDisposable(Runs);
    }

    [RelayCommand]
    private void ApplyFilter(ResultState? state)
    {
        FilterState = state ?? ResultState.None;
    }
    
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not RunObserver observer) return;
        if (!Runs.Any(s => s.Is(observer))) return;

        foreach (var item in Selected)
        {
            message.Reply(item);
        }
    }

    public override void Receive(Observer.Deleted message)
    {
        if (message.Observer is not RunObserver observer) return;
        Runs.Remove(observer);
    }

    protected override void FilterChanged(string? filter)
    {
        Runs.Filter(filter);
    }
}