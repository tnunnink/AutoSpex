using System.Collections;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A step of a query or specification that will select specific properties to transform the output to subsequent steps.
/// </summary>
public class Select() : Step
{
    /// <summary>
    /// Creates a new default <see cref="Select"/> step with default values.
    /// </summary>
    public Select(params string[] properties) : this()
    {
        Properties.AddRange(properties);
    }

    /// <summary>
    /// The collection of properties paths to select from input objects in order to transform the output.
    /// These are used in <see cref="Process"/> to transform the input collection to the output. 
    /// </summary>
    [JsonInclude]
    public List<string> Properties { get; private init; } = [];


    /// <inheritdoc />
    public override IEnumerable<object?> Process(IEnumerable<object?> input)
    {
        var results = new List<object?>();

        foreach (var item in input)
        {
            switch (Properties.Count)
            {
                case 0:
                    results.Add(item);
                    break;
                case 1:
                    results.AddRange(SelectSingle(item));
                    break;
                case > 1:
                    results.Add(SelectMany(item));
                    break;
            }
        }

        return results;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Select step is the one step that will transform the input into a different output based on the configured
    /// collection of <see cref="Properties"/>.
    /// </remarks>
    public override Property Returns(Property input)
    {
        return Properties.Count switch
        {
            0 => input,
            1 => Property.This(input.GetProperty(Properties[0]).InnerType),
            _ => Element.Dynamic(Properties).This
        };
    }

    /// <summary>
    /// Handles selecting single inner property. If this is a collection will return each item in collection.
    /// Otherwise, returns the strongly-typed object.
    /// </summary>
    private IEnumerable<object?> SelectSingle(object? item)
    {
        var origin = Property.This(item);
        var path = Properties[0];
        var property = origin.GetProperty(path);
        var value = property.GetValue(item);

        return value switch
        {
            string text => [text],
            IEnumerable enumerable => enumerable.Cast<object>(),
            _ => [value]
        };
    }

    /// <summary>
    /// Handles case for multiple properties. We will create a new dynamic object using the .NET ExpandoObject.
    /// </summary>
    private ExpandoObject SelectMany(object? item)
    {
        var origin = Property.This(item);

        var bag = (IDictionary<string, object?>)new ExpandoObject();

        foreach (var path in Properties)
        {
            var property = origin.GetProperty(path);
            var value = property.GetValue(item);
            bag.Add(path, value);
        }

        return (ExpandoObject)bag;
    }
}