using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ardalis.SmartEnum.SystemTextJson;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents the query part of the specification. This class is configured with the <see cref="Element"/> and
/// optional <see cref="Name"/> to find in the L5X. It contains the logic for executing the query against a given L5X
/// and returning the resulting <see cref="LogixElement"/> collection.
/// </summary>
public partial class Query
{
    /// <summary>
    /// Creates a default <see cref="Query"/>.
    /// </summary>
    public Query()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Query"/> with the provided element and optioanl name.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> to query.</param>
    /// <param name="name">The name of the element to scope the query to.</param>
    public Query(Element element, string? name = default)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Name = name;
    }

    /// <summary>
    /// The target <see cref="Engine.Element"/> this query will search for.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Element, string>))]
    [JsonInclude]
    public Element Element { get; set; } = Element.Default;

    /// <summary>
    /// The name of the component to lookup in the L5X file. This can also be the tag name of a nested tag.
    /// When set we will use the internal L5X index to find the specified component(s) which optimizes the query step
    /// for specifications.
    /// </summary>
    [JsonInclude]
    public string? Name { get; set; }

    /// <summary>
    /// The segment of <see cref="Name"/> that represents the program name to scope the search to.
    /// User can enter the name in standard 'Program:ProgramName.TagName' format.
    /// </summary>
    [JsonIgnore]
    public string? ContainerName => ExtractContainerName();

    /// <summary>
    /// The segment of <see cref="Name"/> that represents the local tag/routine name to scope the search to.
    /// User can enter the name in standard 'Program:ProgramName.TagName' format.
    /// </summary>
    [JsonIgnore]
    public string? LocalName => ExtractLocalName();

    /// <summary>
    /// Runs the configured query given against the L5X content file and returns the resulting candidate elements
    /// to be further processed.
    /// </summary>
    /// <param name="content">The L5X file to search.</param>
    /// <returns>A collection of <see cref="LogixElement"/> that satisfy the query configuration.</returns>
    public IEnumerable<LogixElement> Execute(L5X content)
    {
        ArgumentNullException.ThrowIfNull(content);

        //If no specific name is specified then we query for all instances.
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(LocalName))
        {
            return Element != Element.Tag
                ? content.Query(Element.Type)
                : content.Query<Tag>().SelectMany(t => t.Members());
        }

        //Tags are special case because they are scoped.
        //We want to get all with the provided tag name then filter based on the container if configured.
        if (Element == Element.Tag)
        {
            //This method gets the nested tag names which is why we treat it differently.
            var scoped = content.All(LocalName);
            return string.IsNullOrEmpty(ContainerName) ? scoped : scoped.Where(t => t.Container == ContainerName);
        }

        //Every other component is global, so we can just use the all method and scope the provided container name if configured.
        var candidates = content.All(new ComponentKey(Element.Name, Name));
        return string.IsNullOrEmpty(ContainerName) ? candidates : candidates.Where(t => t.Container == ContainerName);
    }

    /// <summary>
    /// Gets the container name part of the <see cref="Name"/> property.
    /// </summary>
    private string? ExtractContainerName()
    {
        if (string.IsNullOrEmpty(Name)) return null;

        var match = TagNameRegex().Match(Name);
        if (!match.Success) return null;

        return match.Groups[1].Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Gets the local name part of the <see cref="Name"/> property.
    /// </summary>
    private string? ExtractLocalName()
    {
        if (string.IsNullOrEmpty(Name)) return null;

        var match = TagNameRegex().Match(Name);
        if (!match.Success) return null;

        return match.Groups[2].Success ? match.Groups[2].Value : null;
    }

    /// <summary>
    /// The regex pattern to extract the container and local name from <see cref="Name"/>.
    /// </summary>
    [GeneratedRegex("^(?:Program:([^.]+))?\\.?(.+)")]
    private static partial Regex TagNameRegex();
}