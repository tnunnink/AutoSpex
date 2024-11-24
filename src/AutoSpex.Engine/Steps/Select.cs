using System.Collections;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// 
/// </summary>
public class Select : Step
{
    /// <summary>
    /// Used to construct object using Json serializer.
    /// </summary>
    [JsonConstructor]
    private Select(Property property)
    {
        Property = property;
    }

    /// <summary>
    /// Creates a new default <see cref="Filter"/> object with default values.
    /// </summary>
    public Select()
    {
    }

    /// <summary>
    /// The property of the input type to select and return the value of to the next step in the chain.
    /// </summary>
    [JsonConverter(typeof(JsonPropertyConverter))]
    public Property Property { get; set; } = Property.Default;


    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object> input)
    {
        var results = new List<object>();

        foreach (var item in input)
        {
            var value = Property != Property.Default ? Property.GetValue(item) : item;

            switch (value)
            {
                //this is because string is IEnumerable
                case string text:
                    results.Add(text);
                    break;
                case IEnumerable collection:
                    results.AddRange(collection.Cast<object>());
                    break;
                case not null: //todo I think we migth ultimately need to add null unless specified otherwise (criteria?)
                    results.Add(value);
                    break;
            }
        }

        return results;
    }
}