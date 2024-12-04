namespace AutoSpex.Engine;

/// <summary>
/// Represents a step in the automated testing process which verifies the input objects based on a set of criteria.
/// </summary>
public class Verify : Step
{
    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object?> input)
    {
        var evaluations = new List<Evaluation>();

        foreach (var item in input)
        {
            var results = Criteria.Select(x => x.Evaluate(item));
            evaluations.AddRange(results);
        }

        return evaluations;
    }
}