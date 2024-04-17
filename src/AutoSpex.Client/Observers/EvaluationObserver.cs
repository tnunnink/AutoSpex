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

    /// <summary>
    /// Determines if this evaluation object contains the provided filter text input. 
    /// </summary>
    /// <param name="text">The text to search or filter this evaluation on.</param>
    /// <returns><c>true</c> if this object has a property containing the provided text, otherwise <c>false</c>.</returns>
    public bool Filter(string? text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        if (CandidateName.Contains(text, StringComparison.OrdinalIgnoreCase)) return true;
        if (Message.Contains(text, StringComparison.OrdinalIgnoreCase)) return true;
        if (Expected.Any(x => x.Text.Contains(text))) return true;
        if (Actual is not null && Actual.Text.Contains(text)) return true;
        return false;
    }
}