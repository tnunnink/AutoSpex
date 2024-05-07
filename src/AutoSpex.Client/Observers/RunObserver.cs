using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using AutoSpex.Persistence.Variables;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class RunObserver(Run model) : NamedObserver<Run>(model)
{
    private readonly List<Spec> _specs = [];
    private L5X? _content;
    private CancellationTokenSource? _cancellation;
    
    public RunObserver() : this(new Run())
    {
    }
    
    public override Guid Id => Model.RunId;
    public Guid NodeId => Model.NodeId;
    public Guid SourceId => Model.SourceId;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public ObservableCollection<Outcome> Outcomes { get; } = [];
    
    public ResultState Result => Model.Result;
    public string RanOnText => $"Ran on {Model.RanOn:G} by {Model.RanBy}";

    [ObservableProperty] private bool _running;
    
    [ObservableProperty] private int _total;
    
    /*public int Ran => Model.Outcomes.Count;
    public int Passed => Model.Outcomes.Count(o => o.Result == ResultState.Passed);
    public int Failed => Model.Outcomes.Count(o => o.Result == ResultState.Failed);
    public int Errored => Model.Outcomes.Count(o => o.Result == ResultState.Error);
    public long Duration => Model.Outcomes.Sum(o => o.Duration);
    public long Average => Duration / Ran;*/
    
    protected override Task<Result> RenameModel(string name) => Mediator.Send(new RenameRun(this));
    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);

    
    [RelayCommand]
    private async Task Execute()
    {
        if (_specs.Count == 0) return;
        if (_content is null) return;
        
        //Indicate a run to the UI
        _cancellation = new CancellationTokenSource();
        Running = true;

        //Reload all specs, source content, and variables prior to each run.
        await LoadSpecs(_cancellation.Token);
        await LoadContent(_cancellation.Token);
        await LoadVariables(_cancellation.Token);
        
        //Configure a progress tracker which will receive outcomes as they are completed.
        var progress = new Progress<Outcome>();
        progress.ProgressChanged += PostOutcome;

        //Execute the run
        await Model.Execute(_specs, _content, progress);
        
        //Reset
        Running = false;
    }

    [RelayCommand]
    private Task Cancel()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Loads the specifications associated with the run.
    /// </summary>
    private async Task LoadSpecs(CancellationToken token)
    {
        _specs.Clear();
        
        var ids = Model.Outcomes.Select(x => x.SpecId);
        var result = await Mediator.Send(new GetSpecsIn(ids), token);
        if (result.IsFailed) return;
        
        _specs.AddRange(result.Value);
        Total = _specs.Count;
    }
    
    private async Task LoadContent(CancellationToken token)
    {
        var result = await Mediator.Send(new GetSourceContent(Model.SourceId), token);
        if (result.IsFailed) return;
        _content = result.Value;
    }

    private Task LoadVariables(CancellationToken token)
    {
        return Mediator.Send(new ResolveVariables(_specs), token);
    }
    
    private void PostOutcome(object? sender, Outcome outcome)
    {
        Outcomes.Add(outcome);
    }
}