namespace AutoSpex.Engine;

public class Verification
{
    private Verification(ResultState result, params Evaluation[] evaluations)
    {
        Result = result;
        Evaluations = evaluations ?? throw new ArgumentNullException(nameof(evaluations));
    }

    public ResultState Result { get; }
    public IEnumerable<Evaluation> Evaluations { get; }

    public static Verification For(Evaluation evaluation) => new(evaluation.Result, evaluation);

    public static Verification All(Evaluation[] evaluations)
    {
        var result = ResultState.MaxOrDefault(evaluations.Select(e => e.Result).ToList());
        return new Verification(result, evaluations);
    }

    public static Verification Any(Evaluation[] evaluations)
    {
        var result = ResultState.MinOrDefault(evaluations.Select(e => e.Result).ToList());
        return new Verification(result, evaluations);
    }
}