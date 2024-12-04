using System.Collections;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// 
/// </summary>
public class Select() : Step
{
    /// <summary>
    /// Creates a new default <see cref="Select"/> step with default values.
    /// </summary>
    public Select(string property) : this()
    {
        Property = property;
    }

    /// <summary>
    /// The property of the input type to select and return the value of to the next step in the chain.
    /// </summary>
    [JsonInclude]
    public string Property { get; set; } = string.Empty;

    /// <inheritdoc />
    public override IEnumerable<object?> Process(IEnumerable<object?> input)
    {
        var results = new List<object?>();

        foreach (var item in input)
        {
            var origin = item is not null ? Engine.Property.This(item.GetType()) : Engine.Property.Default;
            var property = origin.GetProperty(Property);
            var value = property.GetValue(item);

            switch (value)
            {
                case string text:
                    results.Add(text);
                    break;
                case IEnumerable collection:
                    results.AddRange(collection.Cast<object>());
                    break;
                default:
                    results.Add(value);
                    break;
            }
        }

        return results;
    }
}