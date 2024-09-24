using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public ValueObserver Candidate => new(Model.Candidate);
    public string Criteria => Model.Criteria;
    public ObservableCollection<ValueObserver> Expected => new(Model.Expected.Select(x => new ValueObserver(x)));
    public ValueObserver Actual => new(Model.Actual);
    public Exception? Error => Model.Error;


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;

        var passes = string.IsNullOrEmpty(filter)
                     || Candidate.Filter(filter)
                     || Criteria.Satisfies(filter)
                     || Expected.Any(x => x.Filter(filter))
                     || Actual.Filter(filter)
                     || Error is not null && Error.Message.Satisfies(filter);

        return passes;
    }

    public static implicit operator Evaluation(EvaluationObserver observer) => observer.Model;
    public static implicit operator EvaluationObserver(Evaluation model) => new(model);
}