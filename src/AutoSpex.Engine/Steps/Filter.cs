using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

/// <summary>
/// A step of a specification that will filter input objects based on a configured criteria. 
/// </summary>
public class Filter : Step
{
    /// <summary>
    /// Creates a new default <see cref="Filter"/> step with default values.
    /// </summary>
    public Filter()
    {
    }

    /// <summary>
    /// Creates a new default <see cref="Filter"/> step initialized with a criterion defined by the provided parameters.
    /// </summary>
    public Filter(string property, Operation operation, object? argument = default)
    {
        Criteria.Add(new Criterion(property, operation, argument));
    }

    /// <summary>
    /// Creates a new default <see cref="Filter"/> step initialized the provided criterion.
    /// </summary>
    public Filter(Criterion criterion)
    {
        Criteria.Add(criterion);
    }

    /// <summary>
    /// The <see cref="Match"/> type that specifies whether all or any criterion should be satisfied for a given
    /// object to pass the filter step. 
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Match, int>))]
    public Match Match { get; set; } = Match.All;

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// Each step may have a collection of criteria configured for which it needs to process data.
    /// </summary>
    [JsonInclude]
    public List<Criterion> Criteria { get; private init; } = [];

    /// <inheritdoc />
    public override IEnumerable<object?> Process(IEnumerable<object?> input)
    {
        var filtered = new List<object?>();

        foreach (var item in input)
        {
            var evaluations = Criteria.Select(c => c.Evaluate(item));
            var passed = Match == Match.All ? evaluations.All(x => x) : evaluations.Any(x => x);
            if (!passed) continue;
            filtered.Add(item);
        }

        return filtered;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Filter step does not tranform the input, so it will always return whatever the input is.
    /// </remarks>
    public override Property Returns(Property input)
    {
        return input;
    }
}