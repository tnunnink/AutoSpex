using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class OutcomeObserver : Observer<Outcome>
{
    /// <inheritdoc/>
    public OutcomeObserver(Outcome model) : base(model)
    {
        Evaluations = new ObservableCollection<EvaluationObserver>(
            Model.Evaluations.Select(e => new EvaluationObserver(e)));
    }

    public override Guid Id => Model.OutcomeId;
    public string SpecName => Model.SpecName;
    public string SourceName => Model.SourceName;
    public string SourceNameFormatted => $"({Model.SourceName})";
    public ResultState Result => Model.Result;
    public long Duration => Model.Duration;
    public string DurationFormatted => $"[{Model.Duration} ms]";
    public ObservableCollection<EvaluationObserver> Evaluations { get; }
}