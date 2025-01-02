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
    /// Creates a new default <see cref="Select"/> step with the provided selection properties.
    /// </summary>
    public Select(params string[] properties) : this()
    {
        Selections.AddRange(properties.Select(p => new Selection(p)));
    }

    /// <summary>
    /// Creates a new default <see cref="Select"/> step with the provided selection object.
    /// </summary>
    public Select(Selection selection) : this()
    {
        Selections.Add(selection);
    }

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// Each step may have a collection of criteria configured for which it needs to process data.
    /// </summary>
    [JsonInclude]
    public List<Selection> Selections { get; private init; } = [];

    /// <inheritdoc />
    public override IEnumerable<object?> Process(IEnumerable<object?> input)
    {
        var results = new List<object?>();

        foreach (var item in input)
        {
            switch (Selections.Count)
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
    /// collection of criteria properties.
    /// </remarks>
    public override Property Returns(Property input)
    {
        return Selections.Count switch
        {
            0 => input,
            1 => Property.This(input.GetProperty(Selections[0].Property).InnerType),
            _ => Element.Dynamic(input, Selections).This
        };
    }

    /// <summary>
    /// Handles selecting single inner property. If this is a collection will return each item in collection.
    /// Otherwise, returns the strongly-typed object.
    /// </summary>
    private IEnumerable<object?> SelectSingle(object? item)
    {
        var origin = Property.This(item);
        var path = Selections[0].Property;
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

        foreach (var selection in Selections)
        {
            var property = origin.GetProperty(selection.Property);
            var value = property.GetValue(item);
            bag.Add(selection.Alias, value);
        }

        return (ExpandoObject)bag;
    }
}