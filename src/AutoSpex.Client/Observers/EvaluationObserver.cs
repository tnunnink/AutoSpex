using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple observer class wrapping an <see cref="Evaluation"/> returned from running a spec/criterion.
/// This class helps with formatting and filtering the returned data.
/// </summary>
public class EvaluationObserver : Observer<Evaluation>
{
    /// <summary>
    /// A simple observer class wrapping an <see cref="Evaluation"/> returned from running a spec/criterion.
    /// This class helps with formatting and filtering the returned data.
    /// </summary>
    /// <param name="model">The <see cref="Evaluation"/> object to wrap.</param>
    public EvaluationObserver(Evaluation model) : base(model)
    {
    }

    public ResultState Result => Model.Result;
    public string Candidate => Model.Candidate;
    public string Criteria => Model.Criteria;
    public string Expected => Model.Expected;
    public string Actual => Model.Actual;
    public string? Error => Model.Error;


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;

        var passes = string.IsNullOrEmpty(filter)
                     || Candidate.Satisfies(filter)
                     || Criteria.Satisfies(filter)
                     || Expected.Satisfies(filter)
                     || Actual.Satisfies(filter)
                     || Error is not null && Error.Satisfies(filter);

        return passes;
    }

    public static implicit operator Evaluation(EvaluationObserver observer) => observer.Model;
    public static implicit operator EvaluationObserver(Evaluation model) => new(model);
}