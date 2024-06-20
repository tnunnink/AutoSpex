using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    public Evaluation(Guid criterionId, ResultState result, string candidate, string criteria,
        string expected, string? actual, string? exception)
    {
        CriterionId = criterionId;
        Result = result;
        Candidate = candidate;
        Criteria = criteria;
        Expected = expected;
        Actual = actual ?? string.Empty;
        Error = exception ?? string.Empty;
    }

    private Evaluation(ResultState result, Criterion criterion, object? candidate, object? actual)
    {
        CriterionId = criterion.CriterionId;
        Result = result;
        Candidate = candidate.ToText();
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected();
        Actual = actual.ToText();
    }

    private Evaluation(ResultState result, Criterion criterion, object? candidate, Exception exception)
    {
        CriterionId = criterion.CriterionId;
        Result = result;
        Candidate = candidate.ToText();
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected();
        Error = exception.Message;
    }

    public Guid CriterionId { get; } = Guid.Empty;
    public ResultState Result { get; } = ResultState.None;
    public string Candidate { get; } = string.Empty;
    public string Criteria { get; } = string.Empty;
    public string Expected { get; } = string.Empty;
    public string Actual { get; } = string.Empty;
    public string Error { get; } = string.Empty;


    public static Evaluation Passed(Criterion criterion, object? candidate, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Passed, criterion, candidate, actual);
    }

    public static Evaluation Failed(Criterion criterion, object? candidate, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Failed, criterion, candidate, actual);
    }

    public static Evaluation Errored(Criterion criterion, object? candidate, Exception exception)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Error, criterion, candidate, exception);
    }

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;

    public override string ToString()
    {
        return $"{Candidate} Expected: {Criteria} {Expected}; Found: {Actual};";
    }
}