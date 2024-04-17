using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using FluentResults;

namespace AutoSpex.Client.Observers;

public class RunObserver(Run model) : NamedObserver<Run>(model)
{
    public RunObserver() : base(new Run())
    {
        
    }
    public override Guid Id => Model.RunId;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public List<Spec> Specs { get; } = [];
    
    
    public ResultState Result => Model.Result;
    public string RanOnText => $"Ran on {Model.RanOn:G} by {Model.RanBy}";
    public int Ran => Model.Outcomes.Count;
    public int Passed => Model.Outcomes.Count(o => o.Result == ResultState.Passed);
    public int Failed => Model.Outcomes.Count(o => o.Result == ResultState.Failed);

    /// <summary>
    /// Gets the number of outcomes that resulted in an error.
    /// </summary>
    /// <value>The number of outcomes that resulted in an error.</value>
    public int Errored => Model.Outcomes.Count(o => o.Result == ResultState.Error);

    /// <summary>
    /// The total duration in milliseconds that all sepcs took to run.
    /// </summary>
    public long Duration => Model.Outcomes.Sum(o => o.Duration);

    /// <summary>
    /// The average time in milliseconds each spec has taken to run.
    /// </summary>
    public long Average => Duration / Ran;
    
    protected override Task<Result> RenameModel(string name) => Mediator.Send(new RenameRun(this));

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}