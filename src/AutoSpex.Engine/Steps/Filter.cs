using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

/// <summary>
/// A step of a specification that will filter input objects based on a configured collection of <see cref="Criterion"/>. 
/// </summary>
public class Filter : Step
{
    /// <summary>
    /// Used to construct object using Json serializer.
    /// </summary>
    [JsonConstructor]
    private Filter(Match match, IEnumerable<Criterion> criteria) : base(criteria)
    {
        Match = match;
    }

    /// <summary>
    /// Creates a new default <see cref="Filter"/> object with default values.
    /// </summary>
    public Filter()
    {
    }

    /// <summary>
    /// The <see cref="Match"/> type that specifies whether all or any criterion should be satisfied for a given
    /// object to pass the filter step. 
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Match, int>))]
    public Match Match { get; set; } = Match.All;


    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object> input)
    {
        var filtered = new List<object>();

        foreach (var item in input)
        {
            var evaluations = Criteria.Select(c => c.Evaluate(item));
            var passed = Match == Match.All ? evaluations.All(x => x) : evaluations.Any(x => x);
            if (!passed) continue;
            filtered.Add(item);
        }

        return filtered;
    }
}