using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class OutcomesPageModel : PageViewModel,
    IRecipient<RunObserver.Pending>
{
    private readonly RunObserver _run;

    /// <inheritdoc/>
    public OutcomesPageModel(RunObserver run) : base("Outcomes")
    {
        _run = run;

        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => _run.Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList(),
            count: () => _run.Model.Outcomes.Count()
        );
        Track(Outcomes);
    }

    public override string Route => $"{nameof(Run)}/{_run.Id}/{Title}";
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }

    [ObservableProperty] private OutcomeObserver? _selectedOutcome;

    [ObservableProperty] private string? _filter;

    [ObservableProperty] private ResultState _state = ResultState.None;

    [ObservableProperty] private string? _evaluationFilter;

    [ObservableProperty] private bool _showResults;

    [ObservableProperty] private string _drawerOrientation = "Horizontal";

    public IEnumerable<ResultState> States =>
        [ResultState.None, .._run.Model.Outcomes.Select(x => x.Verification.Result).Distinct()];

    /// <summary>
    /// Toggles the visibility of results in the UI.
    /// </summary>
    [RelayCommand]
    private void ToggleResults()
    {
        ShowResults = !ShowResults;
    }

    /// <summary>
    /// Toggles the orientation of the results drawer view.
    /// </summary>
    [RelayCommand]
    private void ToggleOrientation()
    {
        DrawerOrientation = DrawerOrientation == "Vertical" ? "Horizontal" : "Vertical";
    }

    /// <summary>
    /// Command to update the local result state for filtering outcomes based on result. 
    /// </summary>
    /// <param name="state">The result state to set.</param>
    [RelayCommand]
    private void UpdateState(ResultState? state)
    {
        State = state ?? ResultState.None;
    }

    /// <summary>
    /// Sets all outcomes configured to a pending result state to indicate to the UI that these specs are awaiting a new result. 
    /// </summary>
    public void Receive(RunObserver.Pending message)
    {
        if (message.Run.Id != _run.Id) return;

        foreach (var outcome in Outcomes)
        {
            outcome.Result = ResultState.Pending;
        }
    }

    /// <summary>
    /// When the state or filter properties change apply the filters to the collections.
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(SelectedOutcome):
                ShowResults = true;
                FilterEvaluations(EvaluationFilter);
                break;
            case nameof(State):
                Filter = string.Empty;
                EvaluationFilter = string.Empty;
                ApplyFilter(State);
                break;
            case nameof(Filter):
                ApplyFilter(Filter);
                break;
            case nameof(EvaluationFilter):
                FilterEvaluations(EvaluationFilter);
                break;
        }
    }

    private void ApplyFilter(ResultState state)
    {
        Outcomes.Refresh();

        //Only apply when a state other than none is selected.
        if (state != ResultState.None)
        {
            Outcomes.AddFilter(o =>
            {
                o.Evaluations.Filter(e => e.Result == state);
                return o.Result == state || o.Evaluations.Count > 0;
            });
        }
    }

    private void ApplyFilter(string? filter)
    {
        Outcomes.AddFilter(o =>
        {
            o.Evaluations.Filter(e => e.Filter(filter));
            return o.Filter(filter) || o.Evaluations.Count > 0;
        });
    }

    private void FilterEvaluations(string? filter)
    {
        SelectedOutcome?.Evaluations.Filter(filter);
    }
}