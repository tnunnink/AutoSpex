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

namespace AutoSpex.Client.Pages;

public partial class HistoryPageModel(Observer observer) : PageViewModel("History"),
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Deleted>,
    IRecipient<RunObserver.Finished>
{
    public override string Route => $"{observer.Entity}/{observer.Id}/{Title}";
    public override string Icon => "IconLineClockRotate";
    public ObserverCollection<Run, RunObserver> Runs { get; } = [];
    public ObservableCollection<RunObserver> Selected { get; } = [];
    public ObservableCollection<ResultState> States { get; } = [];
    public ObservableCollection<SourceObserver> Sources { get; } = [];

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    [ObservableProperty] private SourceObserver _filterSource = Source.Empty("All Sources");

    /// <inheritdoc />
    public override async Task Load()
    {
        var runs = await Mediator.Send(new ListRunsFor(observer.Id));
        Runs.Bind(runs.ToList(), r => new RunObserver(r));
        RegisterDisposable(Runs);
        RefreshFilterSelections();
    }

    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not RunObserver run) return;
        if (!Runs.Has(run)) return;

        foreach (var item in Selected)
            message.Reply(item);
    }

    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not RunObserver run) return;
        if (!Runs.Has(run)) return;
        Selected.Clear();
        Runs.Remove(run);
        RefreshFilterSelections();
    }

    public void Receive(RunObserver.Finished message)
    {
        if (observer.Id != message.Run.Node.Id && observer.Id != message.Run.Source.Id) return;
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