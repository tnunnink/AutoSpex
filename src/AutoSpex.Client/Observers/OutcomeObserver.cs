using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class OutcomeObserver : Observer<Outcome>
{
    /// <inheritdoc/>
    public OutcomeObserver(Outcome model) : base(model)
    {
        Result = model.Result;
        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(Model.Evaluations,
            e => new EvaluationObserver(e));
    }

    public override Guid Id => Model.OutcomeId;
    public string SpecName => Model.Spec.Name;
    public string Duration => $"[{Model.Duration} ms]";
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }

    [ObservableProperty] private ResultState _result = ResultState.None;

    public async Task Process(L5X content)
    {
        //notifies UI to show pending result.
        Result = ResultState.Pending;

        //actually run the spec.
        await Model.Process(content);

        Result = Model.Result;
        OnPropertyChanged(nameof(SpecName));
        OnPropertyChanged(nameof(Duration));
        Evaluations.Refresh();
    }
}