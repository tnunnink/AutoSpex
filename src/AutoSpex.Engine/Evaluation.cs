using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    [JsonConstructor]
    private Evaluation(ResultState result, string candidate, string criteria, IEnumerable<string> expected,
        string actual, string? error)
    {
        Result = result;
        Candidate = candidate;
        Criteria = criteria;
        Expected = expected;
        Actual = actual;
        Error = error;
    }

    private Evaluation(ResultState result, Criterion criterion, object candidate, object? actual)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(criterion);

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

        Result = result;
        Candidate = candidate.ToText();
        Criteria = criterion.GetCriteria();
        Expected = criterion.GetExpected().Select(x => x.ToText());
        Error = exception.Message;
    }

    private Evaluation(ResultState result, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(result);

        Result = result;
        Error = exception.Message;
    }

    /// <summary>
    /// The <see cref="ResultState"/> of the evaluation.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<ResultState, int>))]
    public ResultState Result { get; private init; } = ResultState.None;

    /// <summary>
    /// The string that identifies the candidate object this evaluation ran against.
    /// This will essentially be the scope path of a given logix element. We can use this with source id to find
    /// a strong reference to the evaluated object if needed.
    /// </summary>
    [JsonInclude]
    public string Candidate { get; private init; } = string.Empty;

    /// <summary>
    /// The text the indicates the proeprty and operation that the criterion was configured to evaluate.
    /// </summary>
    [JsonInclude]
    public string Criteria { get; private init; } = string.Empty;

    /// <summary>
    /// The set of values that the evaluation was expecting to find.
    /// </summary>
    [JsonInclude]
    public IEnumerable<string> Expected { get; private init; } = [];

    /// <summary>
    /// The actual value that the was obtained from the candidate object. 
    /// </summary>
    [JsonInclude]
    public string Actual { get; private init; } = string.Empty;

    /// <summary>
    /// The error message that was produced by running the criterion to get the evaluation.
    /// This is only set for "Errored" evaluations.
    /// </summary>
    [JsonInclude]
    public string? Error { get; private init; }


    /// <summary>
    /// Cretes a new passing <see cref="Evaluation"/> with the provided criterion, candidate, and actual value. 
    /// </summary>
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
}