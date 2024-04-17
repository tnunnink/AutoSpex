using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using AutoSpex.Persistence.Variables;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

public partial class RunPageModel(RunObserver run) : PageViewModel
{
    private readonly List<Spec> _specs = [];
    private L5X? _content;
    private CancellationTokenSource? _cancellation;
    
    [ObservableProperty] private RunObserver _run = run;

    [ObservableProperty] private bool _running;
    
    [ObservableProperty] private int _total;

    public ObservableCollection<Outcome> Outcomes { get; } = [];

    public ObservableCollection<EvaluationObserver> Evaluations { get; } = [];

    public override async Task Load()
    {
        await LoadSpecs();
        await LoadContent();
        await LoadVariables();
    }

    [RelayCommand]
    private async Task Execute()
    {
        //Indicate a run to the UI
        _cancellation = new CancellationTokenSource();
        Running = true;

        /*//Configure a progress tracker which will receive outcomes as they are completed.
        //Use these to update the evaluations.
        var progress = new Progress<Outcome>();
        progress.ProgressChanged += (_, outcome) =>
        {
            var evaluations = outcome.Evaluations.Select(x => new EvaluationObserver(x));
            Evaluations.AddRange(evaluations);
        };
        */

        /*var run = await RunSpec(source, Spec);
        if (run is not null)
        {
            var evaluations = run.Model.Outcomes.SelectMany(o => o.Evaluations).Select(x => new EvaluationObserver(x));
            Evaluations.AddRange(evaluations);
        }

        LastRun = run;*/
        Running = false;
    }

    [RelayCommand]
    private Task Cancel()
    {
        throw new NotImplementedException();
    }

    private async Task LoadSpecs()
    {
        _specs.Clear();

        var ids = Node.CheckedSpecs.Select(n => n.NodeId);
        var result = await Mediator.Send(new GetSpecsIn(ids));
        if (result.IsFailed) return;

        _specs.AddRange(result.Value);
        Total = _specs.Count;
    }

    private async Task LoadContent()
    {
        var result = await Mediator.Send(new GetSourceContent(Source.Id));
        if (result.IsFailed) return;
        _content = result.Value;
    }

    private Task LoadVariables()
    {
        return Mediator.Send(new ResolveVariables(_specs));
    }
}