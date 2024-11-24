using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// 
/// </summary>
public class Query() : Step
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    public Query(Element element) : this()
    {
        ArgumentNullException.ThrowIfNull(element);
        Element = element;
    }

    /// <summary>
    /// The <see cref="Engine.Element"/> type to query when this step is processed.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<Element, string>))]
    public Element Element { get; set; } = Element.Default;

    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object> input)
    {
        if (input.FirstOrDefault() is not L5X content)
            throw new ArgumentException(
                "The query step require the first argument supplied to be the L5X content to query.");

        return content.Query(Element.Type).Where(e => Criteria.All(c => c.Evaluate(e)));
    }
}