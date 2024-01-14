using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Evaluation
{
    private Evaluation(ResultState result, string message)
    {
        Result = result;
        Message = message;
    }
    
    public ResultState Result { get; init; } = ResultState.None;
    public string Message { get; init; } = "No Evaluation Available";

    public static Evaluation Passed(Criterion criterion, string? type, object[]? args, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        var reference = $"{type}.{criterion.Property}".Trim().Trim('.');
        var result = $"Evaluation Passed for {reference}";
        var expected = args is not null ? $" {string.Join(',', args)}." : string.Empty;
        var message = $"{result}. Value {criterion.Operation.ShouldMessage}{expected} Found: {actual}";

        return new Evaluation(ResultState.Passed, message);
    }

    public static Evaluation Failed(Criterion criterion, string? type, object[]? args, object? actual)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        var reference = $"{type}.{criterion.Property}".Trim().Trim('.');
        var result = $"Evaluation Failed for {reference}";
        var expected = args is not null ? $" {string.Join(',', args)}." : string.Empty;
        var message = $"{result}. Value {criterion.Operation.ShouldMessage}{expected} Found: {actual}";

        return new Evaluation(ResultState.Failed, message);
    }

    public static Evaluation Error(Criterion criterion, Exception exception, string? type)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        var reference = $"{type}.{criterion.Property}".Trim().Trim('.');
        var result = $"Evaluation Error for {reference} using operation '{criterion.Operation}'";
        var message = $"{result}. Exception: {exception.Message}.";

        return new Evaluation(ResultState.Error, message);
    }

    public override string ToString() => Message;
    public static implicit operator bool(Evaluation evaluation) => evaluation.Result == ResultState.Passed;
}