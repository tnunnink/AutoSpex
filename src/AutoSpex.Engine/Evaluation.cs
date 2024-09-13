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

        Result = result;
        Candidate = candidate;
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected();
        Actual = actual;
    }

    private Evaluation(ResultState result, Criterion criterion, object candidate, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(criterion);

        Result = result;
        Candidate = candidate;
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected();
        Error = exception;
    }

    private Evaluation(ResultState result, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(result);

        Result = result;
        Error = exception;
    }

    public ResultState Result { get; } = ResultState.None;
    public object? Candidate { get; }
    public string Criteria { get; } = string.Empty;
    public IEnumerable<object> Expected { get; } = [];
    public object? Actual { get; }
    public Exception? Error { get; }
    public Guid SourceId => GetSourceId(Candidate);
    public string SourceName => GetSourceName(Candidate);
    public string SourcePath => GetSourcePath(Candidate);


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
    /// Tries to get the injected source name from the candidate logix element to be used as addition information
    /// indicating which source this evaluation belongs to.
    /// </summary>
    private static string GetSourceName(object? candidate)
    {
        while (true)
        {
            if (candidate is not List<LogixElement> collection)
                return candidate is LogixElement element
                    ? element.L5X?.Serialize().Attribute("SourceName")?.Value ?? string.Empty
                    : string.Empty;

            candidate = collection.FirstOrDefault();
        }
    }

    /// <summary>
    /// Tries to get the injected source id from the candidate logix element to be used as addition information
    /// indicating which source this evaluation belongs to.
    /// </summary>
    private static Guid GetSourceId(object? candidate)
    {
        while (true)
        {
            if (candidate is not List<LogixElement> collection)
                return candidate is LogixElement element
                    ? element.L5X?.Serialize().Attribute("SourceId")?.Value.Parse<Guid>() ?? Guid.Empty
                    : Guid.Empty;

            candidate = collection.FirstOrDefault();
        }
    }

    /// <summary>
    /// Tries to get the injected source file path from the candidate logix element to be used as addition information
    /// indicating which source this evaluation belongs to.
    /// </summary>
    private static string GetSourcePath(object? candidate)
    {
        while (true)
        {
            if (candidate is not List<LogixElement> collection)
                return candidate is LogixElement element
                    ? element.L5X?.Serialize().Attribute("SourcePath")?.Value ?? string.Empty
                    : string.Empty;

            candidate = collection.FirstOrDefault();
        }
    }
}