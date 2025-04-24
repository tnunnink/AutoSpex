using System.Text;

namespace AutoSpex.Engine;

/// <summary>
/// A lightweight object containing the result of evaluating a criterion instance. This object will contain the result
/// state value along with a formated message to provide further details.
/// </summary>
public record Evaluation(
    ResultState Result,
    string? Criteria = null,
    string? Target = null,
    string? Expected = null,
    string? Actual = null,
    string? Error = null)
{
    /// <summary>
    /// Creates a new passing <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
    public static Evaluation Passed(Criterion criterion, object? candidate, object? actual)
    {
        return new Evaluation(ResultState.Passed, criterion.ToString(), candidate.ToText(), "", actual.ToText());
    }

    /// <summary>
    /// Creates a new failed <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
    public static Evaluation Failed(Criterion criterion, object? candidate, object? actual)
    {
        return new Evaluation(ResultState.Failed, criterion.ToString(), candidate.ToText(), "", actual.ToText());
    }

    /// <summary>
    /// Creates a new errored <see cref="Evaluation"/> with the provided criterion, candidate, and the produced exception. 
    /// </summary>
    public static Evaluation Errored(Criterion criterion, object? candidate, Exception exception)
    {
        return new Evaluation(ResultState.Errored, criterion.ToString(), candidate.ToText(), Error: exception.Message);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append("Expected ").Append(Target).Append(" to have ").Append(Criteria);

        if (Result == ResultState.Passed)
            builder.Append(" and found ").Append(Actual);

        if (Result == ResultState.Failed)
            builder.Append(" but found ").Append(Actual);

        if (Result == ResultState.Errored)
            builder.Append(" but got error ").Append(Error);

        return builder.ToString();
    }

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;
}