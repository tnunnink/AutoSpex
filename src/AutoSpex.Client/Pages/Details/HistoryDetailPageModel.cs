using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class HistoryDetailPageModel() : DetailPageModel("History"),
    IRecipient<Observer.GetSelected>,
    IRecipient<RunObserver.Finished>
{
    public override string Route => Title;
    public override string Icon => Title;
    public ObserverCollection<Run, RunObserver> Runs { get; } = [];
    public ObservableCollection<RunObserver> Selected { get; } = [];
    public ObservableCollection<ResultState> States { get; } = [];
    public ObservableCollection<SourceObserver> Sources { get; } = [];

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    [ObservableProperty] private SourceObserver _filterSource = Source.Empty("All Sources");

    public override async Task Load()
    {
        var runs = await Mediator.Send(new ListRuns());
        Runs.Bind(runs.ToList(), r => new RunObserver(r));
        RegisterDisposable(Runs);
        RefreshFilterSelections();
    }

    /*[RelayCommand]
    private async Task ClearHistory()
    {
        var result = await Mediator.Send(new ClearRuns());
        Notifier.ShowIfFailed(result);
        return result;
    }*/

    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not RunObserver observer) return;
        if (!Runs.Any(s => s.Is(observer))) return;

        foreach (var item in Selected)
            message.Reply(item);
    }

    public override void Receive(Observer.Deleted message)
    {
        if (message.Observer is not RunObserver observer) return;
        Selected.Clear();
        Runs.Remove(observer);
        RefreshFilterSelections();
    }

    public void Receive(RunObserver.Finished message)
    {
        Selected.Clear();
        Runs.Insert(0, message.Run);
        RefreshFilterSelections();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Filter) or nameof(FilterState) or nameof(FilterSource))
        {
            ApplyFilters(Filter, FilterState, FilterSource);
        }
    }

    private void ApplyFilters(string? text, ResultState state, SourceObserver source)
    {
        Runs.Filter(r =>
        {
            var hasText = r.Filter(text);
            var hasState = state == ResultState.None || r.Result == state;
            var hasSource = source.Id == Guid.Empty || r.Source.Id == source.Id;
            return hasText && hasState && hasSource;
        });
    }

    private void RefreshFilterSelections()
    {
        States.Refresh(
            [ResultState.None, ..Runs.Select(r => r.Result).Distinct().OrderBy(r => r.Value)]
        );

        Sources.Refresh(
            [Source.Empty("All Sources"), ..Runs.Select(r => r.Source).DistinctBy(n => n.Id).OrderBy(s => s.Name)]
        );
    }
}