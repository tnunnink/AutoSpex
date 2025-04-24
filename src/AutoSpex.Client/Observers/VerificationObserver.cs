using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class VerificationObserver : Observer<Verification>
{
    public VerificationObserver(Verification model) : base(model)
    {
        Candidate = new ValueObserver(Model.Candidate);
        RegisterDisposable(Candidate);

        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(
            refresh: () => Model.Evaluations.Select(e => new EvaluationObserver(e)).ToList(),
            count: () => Model.Evaluations.Count
        );
        RegisterDisposable(Evaluations);
    }

    public ResultState Result => Model.Result;
    public ValueObserver Candidate { get; }
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }
}