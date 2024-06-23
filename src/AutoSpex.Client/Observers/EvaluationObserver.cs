using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple observer class wrapping an <see cref="Evaluation"/> returned from running a spec/criterion.
/// This class helps with formatting and filtering the returned data.
/// </summary>
/// <param name="model">The <see cref="Evaluation"/> object to wrap.</param>
public class EvaluationObserver(Evaluation model) : Observer<Evaluation>(model)
{
    public ResultState Result => Model.Result;
    public string Candidate => Model.Candidate;
    public string Criteria => Model.Criteria;
    public string Expected => Model.Expected;
    public string Actual => Model.Actual;
    public string Error => Model.Error;

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter)
               || Candidate.PassesFilter(filter)
               || Criteria.PassesFilter(filter)
               || Expected.PassesFilter(filter)
               || Actual.PassesFilter(filter)
               || Error.PassesFilter(filter);
    }

    public static implicit operator Evaluation(EvaluationObserver observer) => observer.Model;
    public static implicit operator EvaluationObserver(Evaluation model) => new(model);
}