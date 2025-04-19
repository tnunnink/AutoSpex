using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a step in the automated testing process which verifies the input objects based on a set of criteria.
/// </summary>
public class Verify : Step
{
    /// <summary>
    /// Creates a new default <see cref="Verify"/> step with default values.
    /// </summary>
    public Verify()
    {
    }
    
    /// <summary>
    /// Creates a new default <see cref="Filter"/> step initialized with a criterion defined by the provided parameters.
    /// </summary>
    public Verify(string property, Operation operation, object? argument = null)
    {
        Criteria.Add(new Criterion(property, operation, argument));
    }
    
    /// <summary>
    /// Creates a new default <see cref="Filter"/> step initialized the provided criterion.
    /// </summary>
    public Verify(Criterion criterion)
    {
        Criteria.Add(criterion);
    }
    
    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// Each step may have a collection of criteria configured for which it needs to process data.
    /// </summary>
    [JsonInclude]
    public List<Criterion> Criteria { get; private init; } = [];
    
    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object?> input)
    {
        var verifications = new List<Verification>();
        
        foreach (var item in input)
        {
            var evaluations = Criteria.Select(x => x.Evaluate(item)).ToArray();
            verifications.Add(new Verification(item, evaluations));
        }

        return verifications;
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Technically this step always returns an <see cref="Evaluation"/> object. At this point we don't totally care
    /// about the return type, but we implement anyway.
    /// </remarks>
    public override Property Returns(Property input)
    {
        return Property.This(typeof(Evaluation));
    }
}