using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// A object that defines how to retrieve a set of data from a source file. The query will start with an known
/// element type to query. It will then send each element through the configured steps (filter/select)
/// to return the resulting data.
/// </summary>
public class Query()
{
    /// <summary>
    /// Creates a new <see cref="Query"/> initialized with the provided element type.
    /// </summary>
    public Query(Element element) : this()
    {
        Element = element;
        CleanSteps();
    }

    /// <summary>
    /// Creates a new <see cref="Query"/> initialized with the provided element type and steps.
    /// </summary>
    [JsonConstructor]
    public Query(Element element, List<Step> steps) : this()
    {
        Element = element;
        Steps = steps;
        CleanSteps();
    }

    /// <summary>
    /// The <see cref="Engine.Element"/> type to query when this query is executed.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<Element, string>))]
    public Element Element { get; set; } = Element.Default;

    /// <summary>
    /// The collection of <see cref="Step"/> that define how to process data for the query.
    /// </summary>
    /// <remarks>
    /// Each step is run in sequence to produce the result. The user can add steps in any order.
    /// This allows us to narrow search in steps and then select nested properties from collection of object to make
    /// evaluations more clear. 
    /// </remarks>
    [JsonInclude]
    public List<Step> Steps { get; private init; } = [];

    /// <summary>
    /// Gets the <see cref="Property"/> that identifies the type this query returns.
    /// </summary>
    [JsonIgnore]
    public Property Returns => Steps.Aggregate(Element.This, (property, step) => step.Returns(property));

    /// <summary>
    /// Executes the Query and Processing steps on the provided L5X content.
    /// </summary>
    /// <param name="content">The L5X content to process.</param>
    /// <returns>The result of executing the specified Query and Processing steps on the content.</returns>
    public IEnumerable<object?> Execute(L5X content)
    {
        ArgumentNullException.ThrowIfNull(content);

        //Query all elements of the specified type.
        var elements = content.Query(Element.Type).Cast<object?>();

        //Run the resulting element through all configured steps to filter and select data.
        return Steps.Aggregate(elements, (input, step) => step.Process(input));
    }

    /// <summary>
    /// Executes the Query and Processing steps on the provided L5X content.
    /// </summary>
    /// <param name="content">The L5X content to process.</param>
    /// <param name="index">The index number of the step to execute this query to.</param>
    /// <returns>The result of executing the specified Query and Processing steps on the content.</returns>
    public IEnumerable<object?> ExecuteTo(L5X content, int index)
    {
        ArgumentNullException.ThrowIfNull(content);

        //Query all elements of the specified type.
        var elements = content.Query(Element.Type).Cast<object?>();

        //Run the resulting element through all configured steps to filter and select data.
        return Steps[..index].Aggregate(elements, (input, step) => step.Process(input));
    }

    /// <summary>
    /// Determines the property to which the input will flow based on the provided step.
    /// </summary>
    /// <param name="step">The step used to determine the property that the input will flow to.</param>
    /// <returns>The property to which the input will flow based on the provided step.</returns>
    public Property InputTo(Step step)
    {
        var index = Steps.IndexOf(step);
        if (index == -1) return Property.Default;
        return index > 0 ? Steps[..index].Aggregate(Element.This, (p, s) => s.Returns(p)) : Element.This;
    }

    /// <summary>
    /// When we materialize an instance of this object, I want to remove an unconfigured steps.
    /// These are steps with no criterion or no selections (or verify step which should not be a part of query).
    /// This way loaded objects and persisted objects are automatically cleaned as they are interacted with.
    /// This was ultimately to help with stranded step instances post migration.
    /// </summary>
    private void CleanSteps()
    {
        var steps = Steps.ToList();

        foreach (var step in steps)
        {
            switch (step)
            {
                case Filter { Criteria.Count: 0 }:
                case Select { Selections.Count: 0 }:
                case Verify:
                    Steps.Remove(step);
                    break;
            }
        }
    }
}