using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    private Evaluation(ResultState result, Criterion criterion, object candidate, object? actual)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(criterion);

        CriterionId = criterion.CriterionId;
        SourceId = GetSourceId(candidate);
        Result = result;
        Candidate = candidate.ToText();
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected().Select(x => x.ToText());
        Actual = actual.ToText();
    }

    private Evaluation(ResultState result, Criterion criterion, object candidate, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(criterion);

        CriterionId = criterion.CriterionId;
        SourceId = GetSourceId(candidate);
        Result = result;
        Candidate = candidate.ToText();
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected().Select(x => x.ToText());
        Error = exception;
    }

    private Evaluation(ResultState result, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(result);

        Result = result;
        Error = exception;
    }

    public Guid CriterionId { get; } = Guid.Empty;
    public Guid SourceId { get; } = Guid.Empty;
    public ResultState Result { get; } = ResultState.None;
    public string Candidate { get; } = string.Empty;
    public string Criteria { get; } = string.Empty;
    public IEnumerable<string> Expected { get; } = [];
    public string Actual { get; } = string.Empty;
    public Exception? Error { get; }


    public static Evaluation Passed(Criterion criterion, object candidate, object? actual) =>
        new(ResultState.Passed, criterion, candidate, actual);

    public static Evaluation Failed(Criterion criterion, object candidate, object? actual) =>
        new(ResultState.Failed, criterion, candidate, actual);

    public static Evaluation Errored(Criterion criterion, object candidate, Exception exception) =>
        new(ResultState.Errored, criterion, candidate, exception);

    public static Evaluation Errored(Exception exception) =>
        new(ResultState.Errored, exception);

    /// <inheritdoc />
    public override string ToString() => $"Expected: {Criteria} {Expected} Found: {Actual};";

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;

    /// <summary>
    /// Tries to get the injected source id from the candidate logix element to be used as addition information
    /// indicating which source produced this evaluation result.
    /// </summary>
    private static Guid GetSourceId(object? candidate)
    {
        if (candidate is not LogixElement element) return Guid.Empty;
        var xml = element.Serialize();
        var root = xml.Ancestors(L5XName.RSLogix5000Content).FirstOrDefault();
        return root?.Attribute("SourceId")?.Value.Parse<Guid>() ?? Guid.Empty;
    }
}