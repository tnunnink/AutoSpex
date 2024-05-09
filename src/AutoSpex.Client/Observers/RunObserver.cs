using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : NamedObserver<Run>
{
    private CancellationTokenSource? _cancellation;

    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            () => Model.Outcomes.Select(n => new OutcomeObserver(n)).ToList(),
            (_, m) => Model.AddOutcome(m),
            (_, m) => Model.AddOutcome(m),
            (_, m) => Model.Remove(m.OutcomeId),
            () => Model.Clear());

        Total = Model.Outcomes.Count();
        Passed = Model.Outcomes.Count(o => o.Result == ResultState.Passed);
        Failed = Model.Outcomes.Count(o => o.Result == ResultState.Failed);
        Errored = Model.Outcomes.Count(o => o.Result == ResultState.Error);
        Duration = Model.Outcomes.Sum(o => o.Duration);
        Average = Ran > 0 ? Duration / Ran : 0;
    }

    public override Guid Id => Model.RunId;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public SourceObserver? Source => Model.Source is not null ? new SourceObserver(Model.Source) : default;
    public ResultState Result => Model.Result;
    public string LastRan => $"Ran on {Model.RanOn:G} by {Model.RanBy}";
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }

    [ObservableProperty] private bool _runOnLoad;
    [ObservableProperty] private bool _running;
    [ObservableProperty] private int _total;
    [ObservableProperty] private int _ran;
    [ObservableProperty] private int _passed;
    [ObservableProperty] private int _failed;
    [ObservableProperty] private int _errored;
    [ObservableProperty] private long _duration;
    [ObservableProperty] private long _average;

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);

    protected override Task<Result> RenameModel(string name) => Mediator.Send(new RenameRun(this));

    protected override async Task Navigate()
    {
        //Load full run to navigate into details.
        var load = await Mediator.Send(new GetRun(Id));
        if (load.IsFailed) return;
        await Navigator.Navigate(new RunObserver(load.Value));
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    public async Task Execute()
    {
        if (Model.SourceId == Guid.Empty) return;

        //Indicate a run to the UI
        _cancellation = new CancellationTokenSource();
        Ran = 0;
        Running = true;

        var content = await LoadContent(_cancellation.Token);
        if (content.IsFailed) return;

        //Configure a progress tracker which will receive outcomes as they are completed.
        var progress = new Progress<Outcome>();
        progress.ProgressChanged += OnOutcomesProcessed;

        //Execute the run
        await ExecuteRun(content.Value, progress);

        //Reset
        Running = false;
    }

    private bool CanExecute() => Model.SourceId != Guid.Empty && Outcomes.Any();

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _cancellation?.Cancel();
    }

    private bool CanCancel() => Running;

    /// <summary>
    /// Load the source content for this run from the database.
    /// </summary>
    private async Task<Result<L5X>> LoadContent(CancellationToken token)
    {
        var result = await Mediator.Send(new GetSourceContent(Model.SourceId), token);
        return result.IsFailed ? result : result.Value;
    }

    /// <summary>
    /// Executes this run using the provided specs and L5X content.
    /// </summary>
    /// <param name="content">The L5X content to run the specs against.</param>
    /// <param name="progress">The progress object for tracking the execution progress. (optional)</param>
    /// <returns>The Run object that encapsulates the execution outcome.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input source is null.</exception>
    /// <remarks>
    /// Any spec that is not already added to the run will be skipped. Only specs that are configured via
    /// <see cref="Outcomes"/> will be processed. 
    /// </remarks>
    private async Task ExecuteRun(L5X content, IProgress<Outcome>? progress = default)
    {
        foreach (var outcome in Outcomes)
        {
            await outcome.Process(content);
            progress?.Report(outcome.Model);
        }

        Model.UpdateResult();
        Refresh();
    }

    /// <summary>
    /// Updates the user interface by incrementing counts, duration, and average after each outcome is run.
    /// </summary>
    private void OnOutcomesProcessed(object? sender, Outcome outcome)
    {
        Ran++;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (outcome.Result)
        {
            case ResultState.Passed:
                Passed++;
                break;
            case ResultState.Failed:
                Failed++;
                break;
            case ResultState.Error:
                Errored++;
                break;
        }

        Duration += outcome.Duration;
        Average = Ran > 0 ? Duration / Ran : 0;
    }
}