using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class RunnerPageModel : PageViewModel
{
    private CancellationTokenSource? _cancellation;
    public override string Title => "Runner";
    public override string Icon => "Runner";

    [ObservableProperty] private RunObserver? _run;
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; } = [];

    [RelayCommand]
    private void ClosePage()
    {
        Navigator.Send(new NavigationRequest(this, NavigationAction.Close));
    }

    /// <summary>
    /// Command to start execution of the provided run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        if (Run is null) return;
        
        _cancellation = new CancellationTokenSource();
        
        //Update the UI in preparation for this run. Resets all outcomes and stats.
        PrepareRun(Run);
        
        //Run all specs against all Source.
        await ExecuteRun(Run, _cancellation.Token).ConfigureAwait(true);
        
        //Mark the run as completed to update/refresh the UI.
        CompleteRun(Run);
    }

    private bool CanStart() => Run is not null;
    
    /// <summary>
    /// Command to cancel execution of the current run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _cancellation?.Cancel();

    private bool CanStop() => Run?.Result == ResultState.Pending;


    /// <summary>
    /// Executes the provided <see cref="RunObserver"/>.
    /// This will first fully load all specs configured on the run.
    /// We then iterate each source, load it, and run the specs against it.
    /// As specs are run, the outcomes are posted to the <see cref="Outcomes"/> collection for display on the page.
    /// </summary>
    private async Task ExecuteRun(RunObserver run, CancellationToken token)
    {
        var specs = (await LoadSpecs(run, token)).ToList();

        var sourceIds = run.Model.Sources.Select(n => n.NodeId).ToList();
        foreach (var id in sourceIds)
        {
            await RunSource(id, specs, token);
        }
    }

    /// <summary>
    /// Runs the source against all provided specifications.
    /// This method will lead the source from the database and then run each spec, updating the page as processing ocurrs.
    /// </summary>
    private async Task RunSource(Guid id, List<Spec> specs, CancellationToken token)
    {
        var source = await LoadSource(id, token);
        if (source is null) return;

        foreach (var spec in specs)
        {
            var outcome = await spec.Run(source, token);
            var observer = new OutcomeObserver(outcome);
            Dispatcher.UIThread.Invoke(() => ProcessOutcome(observer));
        }
    }

    /// <summary>
    /// Load all specs configured for the provided <see cref="RunObserver"/>.
    /// </summary>
    private async Task<IEnumerable<Spec>> LoadSpecs(RunObserver run, CancellationToken token)
    {
        var ids = run.Model.Specs.Select(n => n.NodeId);
        var result = await Mediator.Send(new LoadSpecs(ids), token);
        return result.IsSuccess ? result.Value : Enumerable.Empty<Spec>();
    }

    /// <summary>
    /// Load all sources
    /// </summary>
    private async Task<Source?> LoadSource(Guid sourceId, CancellationToken token)
    {
        var result = await Mediator.Send(new GetSource(sourceId), token);
        return result.IsSuccess ? result.Value : default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="run"></param>
    private void PrepareRun(RunObserver run)
    {
        Outcomes.Clear();
        run.Outcomes.Clear();
        run.Result = ResultState.Pending;
        run.Refresh();
    }

    /// <summary>
    /// Updates the user interface by incrementing counts, duration, and average after each outcome is run.
    /// </summary>
    private void ProcessOutcome(OutcomeObserver outcome)
    {
        Outcomes.Add(outcome);
        Run?.Outcomes.Add(outcome);
        OnPropertyChanged(nameof(Run.Ran));
        OnPropertyChanged(nameof(Run.Passed));
        OnPropertyChanged(nameof(Run.Failed));
        OnPropertyChanged(nameof(Run.Error));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="run"></param>
    private void CompleteRun(RunObserver run)
    {
        run.Model.Complete();
        run.Refresh();
    }
}