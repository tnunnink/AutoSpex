namespace AutoSpex.Engine;

public record Verification
{
    private Verification(ResultState result, params Evaluation[] evaluations)
    {
        Result = result;
        Evaluations = evaluations ?? throw new ArgumentNullException(nameof(evaluations));
    }

    public ResultState Result { get; }
    public IReadOnlyCollection<Evaluation> Evaluations { get; }
    public static Verification For(Evaluation evaluation) => new(evaluation.Result, evaluation);
    public static Verification All(Evaluation[] evaluations) => new(evaluations.Max(e => e.Result), evaluations);
    public static Verification Any(Evaluation[] evaluations) => new(evaluations.Min(e => e.Result), evaluations);
}