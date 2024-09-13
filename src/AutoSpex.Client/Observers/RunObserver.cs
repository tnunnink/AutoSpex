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
    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
    }

    public override Guid Id => Model.RunId;
    public override string Name => Model.Name;
    public override string Icon => nameof(Run);
    public ResultState Result => Model.Result;
    public DateTime RanOn => Model.RanOn;
    public string RanBy => Model.RanBy;
    public int Passed => Model.Outcomes.Count(x => x.Result == ResultState.Passed);
    public int Failed => Model.Outcomes.Count(x => x.Result == ResultState.Failed);
    public int Errored => Model.Outcomes.Count(x => x.Result == ResultState.Errored);
    public long Duration => Model.Outcomes.Sum(x => x.Duration);
    public bool HasResult => Model.Result > ResultState.Pending;
    
    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}