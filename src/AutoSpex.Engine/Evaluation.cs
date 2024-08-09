using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    private Evaluation(ResultState result, Criterion criterion, object candidate, object? actual)
    {
        Result = result;
        Candidate = candidate;
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected();
        Actual = actual;
    }

    private Evaluation(ResultState result, Criterion criterion, object candidate, Exception exception)
    {
        Result = result;
        Candidate = candidate;
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected();
        Error = exception;
    }
    
    private Evaluation(ResultState result, Exception exception)
    {
        Result = result;
        Error = exception;
    }

    public ResultState Result { get; } = ResultState.None;
    public string? Message { get; }
    public object Candidate { get; }
    public string Criteria { get; }
    public IEnumerable<object> Expected { get; }
    public object? Actual { get; }
    public Exception? Error { get; }
    public Guid SourceId => GetSourceId();
    public string SourceName => GetSourceName();
    public string SourcePath => GetSourcePath();


    /// <summary>
    /// Creates a new Passed evaluation instance with the provided data.
    /// </summary>
    /// <param name="criterion"></param>
    /// <param name="candidate"></param>
    /// <param name="actual"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Evaluation Passed(Criterion criterion, object candidate, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Passed, criterion, candidate, actual);
    }

    public static Evaluation Failed(Criterion criterion, object candidate, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Failed, criterion, candidate, actual);
    }

    public static Evaluation Errored(Criterion criterion, object candidate, Exception exception)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        return new Evaluation(ResultState.Error, criterion, candidate, exception);
    }

    public static Evaluation Errored(Exception exception)
    {
        return new Evaluation(ResultState.Error, exception);
    }

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;

    public override string ToString() => $"Expected: {Criteria} {Expected} Found: {Actual};";

    /// <summary>
    /// Tries to get the injected source name from the candidate logix element to be used as addition information
    /// indicating which source this evaluation belongs to.
    /// </summary>
    private string GetSourceName()
    {
        return Candidate is LogixElement element
            ? element.L5X?.Serialize().Attribute("SourceName")?.Value ?? string.Empty
            : string.Empty;
    }

    /// <summary>
    /// Tries to get the injected source id from the candidate logix element to be used as addition information
    /// indicating which source this evaluation belongs to.
    /// </summary>
    private Guid GetSourceId()
    {
        return Candidate is LogixElement element
            ? element.L5X?.Serialize().Attribute("SourceId")?.Value.Parse<Guid>() ?? Guid.Empty
            : Guid.Empty;
    }

    /// <summary>
    /// Tries to get the injected source file path from the candidate logix element to be used as addition information
    /// indicating which source this evaluation belongs to.
    /// </summary>
    private string GetSourcePath()
    {
        return Candidate is LogixElement element
            ? element.L5X?.Serialize().Attribute("SourcePath")?.Value ?? string.Empty
            : string.Empty;
    }
}