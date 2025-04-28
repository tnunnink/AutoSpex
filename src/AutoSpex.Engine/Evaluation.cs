using System.Text;

namespace AutoSpex.Engine;

/// <summary>
/// A lightweight object containing the result of evaluating a criterion instance. This object will contain the result
/// state value along with result values.
/// </summary>
public record Evaluation(ResultState Result, string? Criteria = null, string? Expected = null, string? Returned = null)
{
    /// <summary>
    /// Creates a new passing <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
    public static Evaluation Passed(string criteria, string expected, object? result)
    {
        return new Evaluation(ResultState.Passed, criteria, expected, result.ToText());
    }

    /// <summary>
    /// Creates a new failed <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
    public static Evaluation Failed(string criteria, string expected, object? result)
    {
        return new Evaluation(ResultState.Failed, criteria, expected, result.ToText());
    }

    /// <summary>
    /// Creates a new errored <see cref="Evaluation"/> with the provided criterion, candidate, and the produced exception. 
    /// </summary>
    public static Evaluation Errored(string criteria, string expected, Exception exception)
    {
        return new Evaluation(ResultState.Errored, criteria, expected, exception.Message);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append("Expected ").Append(Criteria).Append(' ').Append(Expected);

        if (Result == ResultState.Passed)
            builder.Append(" and found ");

        if (Result == ResultState.Failed)
            builder.Append(" but found ");

        if (Result == ResultState.Errored)
            builder.Append(" but got error ");

        builder.Append(Returned);

        return builder.ToString();
    }

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;
}