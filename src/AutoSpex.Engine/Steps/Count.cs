using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

public class Count : Step
{
    /// <summary>
    /// Creates a new default <see cref="Count"/> step with default values.
    /// </summary>
    public Count()
    {
    }

    /// <summary>
    /// Creates a new default <see cref="Count"/> step initialized the provided criterion.
    /// </summary>
    public Count(Criterion criterion)
    {
        Criteria.Add(criterion);
    }
    
    /// <summary>
    /// The <see cref="Match"/> type that specifies whether all or any criterion should be satisfied for a given
    /// object to pass the step. 
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Match, int>))]
    public Match Match { get; set; } = Match.All;

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// </summary>
    [JsonInclude]
    public List<Criterion> Criteria { get; private init; } = [];
    
    /// <inheritdoc />
    public override IEnumerable<object?> Process(IEnumerable<object?> input)
    {
        if (Criteria.Count == 0) return [input.Count()];
        
        var filtered = new List<object?>();

        foreach (var item in input)
        {
            var evaluations = Criteria.Select(c => c.Evaluate(item));
            var passed = Match == Match.All ? evaluations.All(x => x) : evaluations.Any(x => x);
            if (!passed) continue;
            filtered.Add(item);
        }

        return [filtered.Count];
    }

    /// <inheritdoc />
    /// <remarks>
    /// Count will always return a integer number.
    /// </remarks>
    public override Property Returns(Property input)
    {
        return Property.This(typeof(int));
    }
}