using AutoSpex.Engine.Operations;
using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    private readonly string? _message;

    private Evaluation(ResultType result, Criterion criterion, object? actual = null, string? message = null)
    {
        Result = result;
        Element = criterion.Element;
        Property = criterion.Property;
        Operation = criterion.Operation;
        Expected = string.Join(",", criterion.Arguments);
        Actual = actual;
        _message = message;
    }

    public ResultType Result { get; }
    public Element Element { get; }
    public string Property { get; }
    public Operation Operation { get; }
    public string Expected { get; }
    public object? Actual { get; }

    public string Message => _message ?? GenerateMessage();

    public static Evaluation Of(ResultType result, Criterion criterion, object? actual) =>
        new(result, criterion, actual);

    public static Evaluation Passed(Criterion criterion, object? actual) =>
        new(ResultType.Failed, criterion, actual);

    public static Evaluation Failed(Criterion criterion, object? actual = null) =>
        new(ResultType.Failed, criterion, actual);

    public static Evaluation Error(Criterion criterion, string message) =>
        new(ResultType.Error, criterion, message: message);

    public static Evaluation Inconclusive(Criterion criterion, string message) =>
        new(ResultType.Inconclusive, criterion, message: message);

    public static Evaluation NotRun(Criterion criterion) =>
        new(ResultType.None, criterion);

    private string GenerateMessage() =>
        $"Evaluation of {Operation} for {Element.Name}.{Property} {Result}. Expected: '{Expected}' Found: '{Actual}'";

    public override string ToString() => Message;

    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultType.Passed;
}