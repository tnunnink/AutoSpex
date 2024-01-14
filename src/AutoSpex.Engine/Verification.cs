using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

[PublicAPI]
public record Verification
{
    private Verification(ResultState result, object? candidate, params Evaluation[] evaluations)
    {
        Result = result;
        Type = candidate?.GetType().TypeIdentifier();
        Data = candidate is LogixElement element ? element.Serialize().ToString() : candidate?.ToString();
        Evaluations = evaluations ?? throw new ArgumentNullException(nameof(evaluations));
    }

    public ResultState Result { get; }
    public string? Type { get; }
    public string? Data { get; }
    public IReadOnlyCollection<Evaluation> Evaluations { get; }

    public IEnumerable<string> Successes =>
        Evaluations.Where(e => e.Result == ResultState.Passed).Select(e => e.Message);

    public IEnumerable<string> Failures =>
        Evaluations.Where(e => e.Result == ResultState.Failed).Select(e => e.Message);

    public IEnumerable<string> Errors =>
        Evaluations.Where(e => e.Result == ResultState.Error).Select(e => e.Message);

    public static Verification For(object? candidate, Evaluation evaluation) =>
        new(evaluation.Result, candidate, evaluation);

    public static Verification All(object? candidate, Evaluation[] evaluations) =>
        new(evaluations.Max(e => e.Result), candidate, evaluations);

    public static Verification Any(object? candidate, Evaluation[] evaluations) =>
        new(evaluations.Min(e => e.Result), candidate, evaluations);
}