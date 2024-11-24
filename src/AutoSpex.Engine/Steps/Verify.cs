using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a step in the automated testing process which verifies the input objects based on a set of criteria.
/// </summary>
public class Verify : Step
{
    /// <summary>
    /// Used to construct object using Json serializer.
    /// </summary>
    [JsonConstructor]
    private Verify(IEnumerable<Criterion> criteria) : base(criteria)
    {
    }

    /// <summary>
    /// Creates a new default <see cref="Verify"/> step object with default values.
    /// </summary>
    public Verify()
    {
    }

    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object> input)
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