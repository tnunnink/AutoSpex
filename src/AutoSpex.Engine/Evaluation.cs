using System.Text;

namespace AutoSpex.Engine;

/// <summary>
/// A lightweight object containing the result of evaluating a criterion instance. This object will contain the result
/// state value along with a formated message to provide further details.
/// </summary>
public record Evaluation
{
    /// <summary>
    /// Creates a new <see cref="Evaluation"/> instance with the provided result and message.
    /// </summary>
    private Evaluation(ResultState result, string message)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// The <see cref="ResultState"/> of the evaluation.
    /// </summary>
    public ResultState Result { get; }

    /// <summary>
    /// The message associated with the evaluation, describing details of the result or any error encountered.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new passing <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
    public static Evaluation Passed(Criterion criterion, object? candidate, object? actual)
    {
        var builder = new StringBuilder();

        builder.Append("Expected ")
            .Append(candidate.ToText())
            .Append(" to have ")
            .Append(criterion)
            .Append(" and found ")
            .Append(actual.ToText());

        return new Evaluation(ResultState.Passed, builder.ToString());
    }

    /// <summary>
    /// Creates a new failed <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
    public static Evaluation Failed(Criterion criterion, object? candidate, object? actual)
    {
        var builder = new StringBuilder();

        builder.Append("Expected ")
            .Append(candidate.ToText())
            .Append(" to have ")
            .Append(criterion)
            .Append(" but found ")
            .Append(actual.ToText());

        return new Evaluation(ResultState.Failed, builder.ToString());
    }

    /// <summary>
    /// Creates a new errored <see cref="Evaluation"/> with the provided criterion, candidate, and the produced exception. 
    /// </summary>
    public static Evaluation Errored(Criterion criterion, object? candidate, Exception exception)
    {
        var builder = new StringBuilder();

        builder.Append("Expected ")
            .Append(candidate.ToText())
            .Append(" to have ")
            .Append(criterion)
            .Append(" but got error ")
            .Append(exception.Message);

        return new Evaluation(ResultState.Errored, builder.ToString());
    }

    /// <summary>
    /// Creates a new errored <see cref="Evaluation"/> a produced exception. 
    /// </summary>
    public static Evaluation Errored(Exception exception)
    {
        return new Evaluation(ResultState.Errored, exception.Message);
    }

    /// <inheritdoc />
    public override string ToString() => Message;

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;
}