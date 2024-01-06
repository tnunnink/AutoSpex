using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    private readonly string? _message;

    private Evaluation(ResultType result, Criterion criterion, object? candidate, object? actual,
        string? message = null, Exception? exception = null)
    {
        Result = result;
        Candidate = candidate;
        Type = candidate?.GetType();
        Property = criterion.Property;
        Operation = criterion.Operation;
        Expected = string.Join(",", criterion.Arguments.Select(a => a.Value));
        Actual = actual;
        Exception = exception;
        _message = exception is not null ? exception.Message : message;
    }

    public ResultType Result { get; }
    public Type? Type { get; }
    public string? Property { get; }
    public Operation Operation { get; }
    public string Expected { get; }
    public object? Actual { get; }
    public object? Candidate { get; }
    public string Message => _message ?? GenerateMessage();
    public Exception? Exception { get; }

    public static Evaluation Passed(Criterion criterion, object? candidate, object? actual) =>
        new(ResultType.Passed, criterion, candidate, actual);

    public static Evaluation Failed(Criterion criterion, object? candidate, object? actual = null) =>
        new(ResultType.Failed, criterion, candidate, actual);

    public static Evaluation Error(Criterion criterion, object? candidate, Exception exception) =>
        new(ResultType.Error, criterion, candidate, null, exception: exception);

    public static Evaluation Inconclusive(Criterion criterion, object? candidate, string message) =>
        new(ResultType.Inconclusive, criterion, candidate, null, message);

    public static Evaluation NotRun(Criterion criterion, object? candidate) =>
        new(ResultType.None, criterion, candidate, null);

    private string GenerateMessage() =>
        $"Evaluation of {Operation} for {Type?.Name}.{Property} {Result}. Expected: '{Expected}' Found: '{Actual}'";

    public override string ToString() => Message;

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultType.Passed;
}