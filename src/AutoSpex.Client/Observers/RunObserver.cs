using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : Observer<Run>
{
    private CancellationTokenSource? _cancellation;

    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Result = Model.Result;
        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList()
        );
        Sources = new ObserverCollection<Source, SourceObserver>(Model.Environment.Sources, s => new SourceObserver(s));
        Sources.ItemPropertyChanged += OnSourcesItemPropertyChanged;
    }

    public override Guid Id => Model.RunId;
    public override string Name => Model.Name;
    public override string Icon => "Run";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result;

    [ObservableProperty] private bool _isVirtual;
    public DateTime RanOn => Model.RanOn;
    public string RanBy => Model.RanBy;
    public int Ran => Outcomes.Count(x => x.Result > ResultState.Pending);
    public float Progress => Outcomes.Count > 0 ? (float)Ran / Outcomes.Count * 100 : 0;
    public int Passed => Outcomes.Count(x => x.Result == ResultState.Passed);
    public int Failed => Outcomes.Count(x => x.Result == ResultState.Failed);
    public int Errored => Outcomes.Count(x => x.Result == ResultState.Error);
    public long Duration => Outcomes.Sum(x => x.Duration);
    public bool HasResult => Result > ResultState.Pending;
    public bool HasManySources => Sources.Count > 1;
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }
    public ObservableCollection<OutcomeObserver> Selected { get; } = [];
    public ObserverCollection<Source, SourceObserver> Sources { get; }

    public IEnumerable<ResultState> ResultFilters { get; } =
        [ResultState.None, ResultState.Passed, ResultState.Failed, ResultState.Error];

    [ObservableProperty] private ResultState _resultFilter = ResultState.None;


    /// <inheritdoc />
    /// <remarks>We want to load the entire run first.</remarks>
    protected override async Task Navigate()
    {
        var result = await Mediator.Send(new GetRun(Id));
        if (result.IsFailed) return;
        await Navigator.Navigate(new RunObserver(result.Value));
    }

    /// <summary>
    /// Adds the provided node (or its descendants) to this run configuration.
    /// </summary>
    /// <param name="node">The node to add.</param>
    [RelayCommand]
    private void AddNode(NodeObserver node)
    {
        //Getting a local node to use as the different observer instance to respect different UI properties.
        var observer = new NodeObserver(node);
        //We have to add to the model since it handles adding only descendants and not the container node itself (ObserverCollection doesn't know not to).
        //So after each add we just need to Sync the collection of the underlying model to our ObserverCollection. (Sync will trigger change notification which we want)
        Model.AddNode(observer);
        Outcomes.Sync();
        RunCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Updates the local result filter property to allow the UI to update the displayed list of outcomes based on the
    /// selected result.
    /// </summary>
    /// <param name="result">The result state to filter to.</param>
    [RelayCommand]
    private void ApplyResultFilter(ResultState result)
    {
        ResultFilter = result;
    }

    /// <summary>
    /// Command to execute this run by retrieving, resolving, and evaluating all configured spec/source pairs and
    /// producing new outcome results.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task Run()
    {
        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;

        MarkPending();

        try
        {
            var specs = await LoadSpecs(token);
            await Model.ExecuteAsync(specs, OnSpecRunning, OnSpecCompleted, token);
        }
        catch (OperationCanceledException)
        {
        }

        Result = Model.Result;
        Refresh();
    }

    /// <summary>
    /// Indicates that this run can be executed.
    /// </summary>
    private bool CanRun() => Model.Outcomes.Any();

    /// <summary>
    /// Command to cancel execution of this run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cancellation?.Cancel();

    /// <summary>
    /// Indicates that the run can be cancelled.
    /// </summary>
    private bool CanCancel() => Result == ResultState.Pending;

    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var outcome in Outcomes)
        {
            outcome.IsExpanded = true;
        }
    }

    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var outcome in Outcomes)
        {
            outcome.IsExpanded = false;
        }
    }

    /// <summary>
    /// Load all specs configured for the provided <see cref="RunObserver"/>.
    /// </summary>
    private async Task<IEnumerable<Spec>> LoadSpecs(CancellationToken token)
    {
        var ids = Model.Outcomes.Select(n => n.SpecId);
        var result = await Mediator.Send(new LoadSpecs(ids), token);
        return result.IsSuccess ? result.Value : Enumerable.Empty<Spec>();
    }

    private void OnSpecRunning(Outcome outcome)
    {
        var message = new OutcomeObserver.Running(outcome.SpecId);
        Messenger.Send(message);
    }

    /// <summary>
    /// Sends a message that the provided outcome has completed running and its state is updated with the result.
    /// </summary>
    /// <param name="outcome">The outcome that just ran.</param>
    private void OnSpecCompleted(Outcome outcome)
    {
        var message = new OutcomeObserver.Complete(outcome);
        Messenger.Send(message);
        OnPropertyChanged(nameof(Ran));
        OnPropertyChanged(nameof(Progress));
    }

    /// <summary>
    /// Sets this run and all outcomes configured to a pending result state to indicate to the UI that these specs
    /// are awaiting a new result. 
    /// </summary>
    private void MarkPending()
    {
        Result = ResultState.Pending;

        foreach (var outcome in Outcomes)
        {
            outcome.Result = ResultState.Pending;
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(FilterText) or nameof(ResultFilter))
        {
            FilterOutcomes();
        }
    }

    /// <summary>
    /// When a source observer item is checked from this run, refresh the outcomes and result collection to only
    /// show the checked sources.
    /// </summary>
    private void OnSourcesItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsChecked)) return;

        var sources = Sources.Where(x => x.IsChecked).ToList();

        foreach (var outcome in Outcomes)
            outcome.SelectedSources = sources;
    }

    /// <summary>
    /// Gets the collection of outcomes to display for this run based on the specified result and source filters.
    /// </summary>
    private void FilterOutcomes()
    {
        var filter = FilterText;
        var state = ResultFilter;
        Outcomes.Filter(o => o.Filter(filter) && (state == ResultState.None || o.Result == state));
    }

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}