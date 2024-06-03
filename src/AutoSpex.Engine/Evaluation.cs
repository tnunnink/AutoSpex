using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    private Evaluation(ResultState result, Criterion criterion, object? candidate, object[] args, object? actual)
    {
        Result = result;
        CriterionId = criterion.CriterionId;
        Message = string.Join(" ", criterion.Property, criterion.Operation?.ShouldMessage);
        Candidate = candidate;
        Expected = args;
        Actual = actual;
    }
    
    private Evaluation(ResultState result, Criterion criterion, object? candidate, Exception exception)
    {
        Result = result;
        CriterionId = criterion.CriterionId;
        Message = string.Join(" ", criterion.Property, criterion.Operation?.ShouldMessage);
        Candidate = candidate;
        Expected = [];
        Exception = exception;
    }

    public Guid CriterionId { get; }
    public ResultState Result { get; } = ResultState.None;
    public string Message { get; }
    public object? Candidate { get; }
    public object[] Expected { get; }
    public object? Actual { get; }
    public Exception? Exception { get; }

    public static Evaluation Passed(Criterion criterion, object? candidate, object[] args, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Passed, criterion, candidate, args, actual);
    }

    public static Evaluation Failed(Criterion criterion, object? candidate, object[] args, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Failed, criterion, candidate, args, actual);
    }

    public static Evaluation Error(Criterion criterion, object? candidate, Exception exception)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Error, criterion, candidate, exception);
    }

    public override string ToString() => Message;
    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;
}