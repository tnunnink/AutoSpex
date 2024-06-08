using System;
using System.Collections.Generic;
using System.Linq;
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
    public string CandidateName => Model.Candidate.ToText();
    public string Message => Model.Message;
    public IEnumerable<ValueObserver> Expected => Model.Expected.Select(x => new ValueObserver(x));
    public ValueObserver? Actual => Model.Actual is not null ? new ValueObserver(Model.Actual) : default;


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        if (string.IsNullOrEmpty(filter)) return true;
        if (CandidateName.Contains(filter, StringComparison.OrdinalIgnoreCase)) return true;
        if (Message.Contains(filter, StringComparison.OrdinalIgnoreCase)) return true;
        if (Expected.Any(x => x.Text.Contains(filter))) return true;
        if (Actual is not null && Actual.Text.Contains(filter)) return true;
        return false;
    }

    public static implicit operator Evaluation(EvaluationObserver observer) => observer.Model;
    public static implicit operator EvaluationObserver(Evaluation model) => new(model);
}