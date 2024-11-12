using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a source L5X content that can be run against any set of specifications.
/// Source content will be stored locally and the user can inspect, update, and create runs against the source.
/// </summary>
public class Source
{
    private readonly Dictionary<Guid, Suppression> _suppressions = [];
    private readonly Dictionary<Guid, Node> _overrides = [];

    /// <summary>
    /// Creates a new <see cref="Source"/> with no content. 
    /// </summary>
    public Source()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Source"/> provided the location on disc of the L5X file.
    /// </summary>
    public Source(L5X content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        Name = content.Info.TargetName ?? "New Source";
        TargetName = content.Info.TargetName ?? string.Empty;
        TargetType = content.Info.TargetType ?? string.Empty;
        ExportedOn = content.Info.ExportDate?.ToString() ?? string.Empty;
        ExportedBy = content.Info.Owner ?? string.Empty;
        Description = content.Controller.Description ?? string.Empty;

        InjectMetadata(content);
        Content = content;
    }

    /// <summary>
    /// Represents the unique identifier of a source file.
    /// </summary>
    [JsonInclude]
    public Guid SourceId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the source file.
    /// </summary>
    /// <remarks>
    /// This property represents the name of the source file without the file extension.
    /// </remarks>
    [JsonInclude]
    public string Name { get; set; } = "New Source";

    /// <summary>
    /// Gets or sets a value indicating whether the source is the default run target.
    /// </summary>
    [JsonIgnore]
    public bool IsTarget { get; set; }

    /// <summary>
    /// The name of the element that is the target of the L5X content export.
    /// We will use this to help further identify content this source contains.
    /// </summary>
    [JsonInclude]
    public string TargetName { get; private set; } = string.Empty;

    /// <summary>
    /// The type of the element that is the target of the L5X content export.
    /// We will use this to help further identify content this source contains.
    /// </summary>
    [JsonInclude]
    public string TargetType { get; private set; } = string.Empty;

    /// <summary>
    /// The date/time that the L5X content was exported.
    /// We will use this to help further identify content this source contains.
    /// </summary>
    [JsonInclude]
    public string ExportedOn { get; private set; } = string.Empty;

    /// <summary>
    /// The name of the users that exported the L5X content.
    /// We will use this to help further identify content this source contains.
    /// </summary>
    [JsonInclude]
    public string ExportedBy { get; private set; } = string.Empty;

    /// <summary>
    /// The controller/project description of the L5X content.
    /// </summary>
    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// The <see cref="L5X"/> content this source contains.
    /// </summary>
    [JsonIgnore]
    public L5X Content { get; private set; } = L5X.Empty();

    /// <summary>
    /// Represents the collection of nodes that should be ignored when this source is run.
    /// These nodes will be excluded from processing or consideration within the specified context.
    /// The user can add, remove, or clear the ignored nodes as needed.
    /// </summary>
    [JsonInclude]
    public IEnumerable<Suppression> Suppressions => _suppressions.Values;

    /// <summary>
    /// Gets the collection of <see cref="Override"/> objects representing the overrides that allow the user to
    /// change the configuration of a spec when this source is run.
    /// </summary>
    [JsonInclude]
    public IEnumerable<Node> Overrides => _overrides.Values;

    /// <summary>
    /// Represents an empty source object with default property values and an empty source id.
    /// </summary>
    public static Source Empty => new() { SourceId = Guid.Empty };

    /// <summary>
    /// Updates the internal <see cref="Content"/> of the source with the provided L5X. 
    /// </summary>
    /// <param name="content">The L5X content to update this source with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
    public void Update(L5X content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        TargetName = content.Info.TargetName ?? string.Empty;
        TargetType = content.Info.TargetType ?? string.Empty;
        ExportedOn = content.Info.ExportDate?.ToString() ?? string.Empty;
        ExportedBy = content.Info.Owner ?? string.Empty;
        Description = content.Controller.Description ?? string.Empty;

        InjectMetadata(content);
        Content = content;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    public void AddOverride(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _overrides[node.NodeId] = node;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    public void RemoveOverride(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _overrides.Remove(node.NodeId);
    }

    /// <summary>
    /// Clears all overrides stored in the source.
    /// </summary>
    public void ClearOverrides() => _overrides.Clear();

    /// <summary>
    /// Adds a suppression to the source with the specified node ID and reason.
    /// </summary>
    /// <param name="nodeId">The ID of the node to be suppressed.</param>
    /// <param name="reason">The reason for the suppression.</param>
    /// <exception cref="ArgumentException">Thrown if the nodeId is empty or if the reason is null or empty.</exception>
    public void AddSuppression(Guid nodeId, string reason)
    {
        if (nodeId == Guid.Empty)
            throw new ArgumentException("Supupressions require a non-empty node id");

        if (string.IsNullOrEmpty(reason))
            throw new ArgumentException("Suppressions require a valid reason.");

        _suppressions[nodeId] = new Suppression(nodeId, reason);
    }

    /// <summary>
    /// Adds a suppression for a specific node identified by its unique nodeId in the source.
    /// </summary>
    /// <param name="suppression">The suppression object containing information about the node to suppress.</param>
    public void AddSuppression(Suppression suppression)
    {
        ArgumentNullException.ThrowIfNull(suppression);
        _suppressions[suppression.NodeId] = suppression;
    }

    /// <summary>
    /// Removes the specified suppression entry corresponding to the given suppression from the Source object.
    /// </summary>
    /// <param name="suppression">The suppression entry to be removed.</param>
    public void RemoveSuppression(Suppression suppression)
    {
        ArgumentNullException.ThrowIfNull(suppression);
        _suppressions.Remove(suppression.NodeId);
    }

    /// <summary>
    /// Clears the list of nodes to suppress for this source.
    /// </summary>
    public void ClearSuppressions() => _suppressions.Clear();

    /// <summary>
    /// Applies overrides for the specified nodes based on the stored specifications in the Source.
    /// </summary>
    /// <param name="nodes">The collection of nodes to apply the overrides to.</param>
    public void Override(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            if (!_overrides.TryGetValue(node.NodeId, out var match)) continue;
            node.Configure(match.Spec);
        }
    }

    /// <summary>
    /// Tries to suppress the outcome by applying a suppression if one is configured for the specified node.
    /// </summary>
    /// <param name="outcome">The outcome to potentially suppress</param>
    /// <returns><c>true</c> if the outcome was suppressed by the source configuration; otherwise, <c>false</c>.</returns>
    public bool Suppresses(Outcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        if (!_suppressions.TryGetValue(outcome.NodeId, out var suppression)) return false;
        outcome.Suppress(suppression.Reason);
        return true;
    }

    /// <summary>
    /// Creates a duplicate instance of the current <see cref="Source"/> object with the same content and configuration.
    /// </summary>
    /// <returns>A new <see cref="Source"/> object that is a duplicate of the current instance.</returns>
    public Source Duplicate()
    {
        var duplicate = new Source(Content);

        foreach (var variable in _overrides.Values)
        {
            duplicate.AddOverride(variable);
        }

        foreach (var node in _suppressions.Values)
        {
            duplicate.AddSuppression(node);
        }

        return duplicate;
    }

    /// <summary>
    /// Creates a shallow copy of the current <see cref="Source"/> object.
    /// </summary>
    /// <returns>
    /// A new <see cref="Source"/> object with the same basic information but not <see cref="Content"/>.
    /// </returns>
    public Source Copy()
    {
        return new Source
        {
            SourceId = SourceId,
            Name = Name,
            TargetName = TargetName,
            TargetType = TargetType,
            ExportedOn = ExportedOn,
            ExportedBy = ExportedBy,
            Description = Description
        };
    }

    /// <summary>
    /// Finds values based on the specified property for elements in the content source.
    /// </summary>
    /// <param name="property">The property for which values need to be found.</param>
    /// <returns>An enumerable collection of distinct values for the specified property.</returns>
    public IEnumerable<object> FindValues(Property property)
    {
        if (Content is null) return [];

        //For any statically known value type like boolean or enum we can return early with predefined options.
        if (property.Group == TypeGroup.Boolean || property.Group == TypeGroup.Enum)
            return GetOptions(property.Type);

        try
        {
            //Since every property origin is/should be the L5Sharp type, we can use that to query for elements in the source.
            var elements = Content.Query(property.Origin);

            //Essentially just get non-null distinct values for the specified property for all elements of the origin type.
            var values = elements.Select(property.GetValue).Where(x => x is not null).Cast<object>().Distinct();

            return values.ToList();
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content.
            return [];
        }
    }

    /// <summary>
    /// Handles the request for tag names to suggest to the user as then enter text in a property entry with indexer
    /// notation. This is very usefuly because we don't need to look up the tag structure,
    /// and instead we can have it prompted to us.
    /// </summary>
    public IEnumerable<TagName> FindTagNames(Spec spec, string? filter)
    {
        if (Content is null) return [];

        //This only applies to tag elements. Guard agains anything else.
        if (spec.Element != Element.Tag) return [];

        try
        {
            //Ideally we want to narrow the search space for tag names using the currently configured filters to
            //improve the performance of this lookup which will happen continuously as text changes

            /*var elements = spec.GetCandidates(Content);*/

            var tags = Content.Query<Tag>().Where(t => spec.Filters.All(f => f.Evaluate(t))).ToList();

            /*return tags.SelectMany(t => t.TagNames()).Select(t => t.Path).Distinct().Select(x => new TagName(x));*/

            var tagNames = tags
                .SelectMany(t => t.TagNames())
                .Select(t => t.Path)
                .Distinct()
                .Where(t => !string.IsNullOrEmpty(t) && t.Satisfies(filter))
                .OrderBy(t => t)
                .Select(t => new TagName($"[{t}]"));

            return tagNames.ToList();
        }
        catch (Exception)
        {
            // ignored because this is just optional.
            // If the user enteres invalid tagnames it will result in and errored evaluation telling them the issue.
            return [];
        }
    }

    /// <summary>
    /// Returns a collection of possible values. This is meant primarily for enumeration types so that we
    /// can provide the user with a selectable set of options for a given enum value. This however will also return
    /// true/false for boolean type and empty collection for anything else (numbers, string, collections, complex objects).
    /// </summary>
    private static IEnumerable<object> GetOptions(Type type)
    {
        var group = TypeGroup.FromType(type);
        if (group == TypeGroup.Boolean) return new object[] { true, false };
        return typeof(LogixEnum).IsAssignableFrom(type) ? LogixEnum.Options(type) : [];
    }

    /// <summary>
    /// Adds/sets the source metadata to the L5X content.
    /// This is so we can read back this data from the element without having to find the source information.
    /// </summary>
    private void InjectMetadata(L5X content)
    {
        content.Serialize().SetAttributeValue("SourceId", SourceId.ToString());
    }
}