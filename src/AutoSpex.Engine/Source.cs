using System.Text.Json.Serialization;
using System.Xml.Linq;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a source L5X content that can be run against any set of specifications.
/// Source content will be stored locally and the user can inspect, update, and create runs against the source.
/// </summary>
public class Source()
{
    private readonly Dictionary<Guid, Action> _rules = [];

    /// <summary>
    /// Creates a new <see cref="Source"/> with no content. 
    /// </summary>
    public Source(string? name = default) : this()
    {
        Name = name ?? "New Source";
    }

    /// <summary>
    /// Creates a new <see cref="Source"/> provided the location on disc of the L5X file.
    /// </summary>
    public Source(L5X content, string? name = default) : this()
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        Name = name ?? content.Info.TargetName ?? "New Source";
        TargetName = content.Info.TargetName ?? string.Empty;
        TargetType = content.Info.TargetType ?? string.Empty;
        ExportedOn = content.Info.ExportDate?.ToString() ?? string.Empty;
        ExportedBy = content.Info.Owner ?? string.Empty;
        Description = content.Controller.Description ?? string.Empty;

        InjectMetadata(content);
        ScrubContent(content);
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
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// The <see cref="L5X"/> content this source contains.
    /// </summary>
    [JsonIgnore]
    public L5X? Content { get; private set; }

    /// <summary>
    /// The set of rules to be applied to specifications when this source is run. A rule will define whether to suppress
    /// or override the configuration of a specification. Each rule corresponds to a single node for this source and
    /// will be ustilized to alter the outcomes of specs when this source is run.
    /// </summary>
    [JsonInclude]
    public IEnumerable<Action> Rules => _rules.Values;

    /// <summary>
    /// Represents an empty source object with default property values and an empty source id.
    /// </summary>
    public static Source Empty(string? name = default) => new() { SourceId = Guid.Empty, Name = name ?? "New Source" };

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
        ScrubContent(content);
        Content = content;
    }

    /// <summary>
    /// Adds a new rule to the Source based on the provided Rule.
    /// </summary>
    /// <param name="action">The Rule object to be added. Must not be null.</param>
    public void AddRule(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _rules[action.NodeId] = action;
    }

    /// <summary>
    /// Removes a specified rule from the Source based on the provided rule parameter.
    /// </summary>
    /// <param name="action">The rule to be removed from the Source.</param>
    public void RemoveRule(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _rules.Remove(action.NodeId);
    }

    /// <summary>
    /// Clears all rules associated with this source.
    /// </summary>
    public void ClearRules() => _rules.Clear();

    /// <summary>
    /// Applies overrides for the specified nodes based on the stored rules defined in the Source.
    /// </summary>
    /// <param name="nodes">The collection of nodes to apply the overrides to.</param>
    public void Override(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            if (!_rules.TryGetValue(node.NodeId, out var rule)) continue;
            node.Configure(rule.Config);
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

        if (!_rules.TryGetValue(outcome.NodeId, out var rule)) return false;
        if (rule.Type != ActionType.Suppress) return false;

        outcome.Suppress(rule.Reason);
        return true;
    }

    /// <summary>
    /// Creates a duplicate instance of the current <see cref="Source"/> object with the same content and configuration.
    /// </summary>
    /// <returns>A new <see cref="Source"/> object that is a duplicate of the current instance.</returns>
    public Source Duplicate()
    {
        var duplicate = new Source
        {
            Name = Name,
            TargetName = TargetName,
            TargetType = TargetType,
            ExportedOn = ExportedOn,
            ExportedBy = ExportedBy,
            Description = Description,
            Content = Content
        };

        foreach (var rule in _rules.Values)
            duplicate.AddRule(rule);

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
    /// Adds/sets the source metadata to the L5X content.
    /// This is so we can read back this data from the element without having to find the source information.
    /// </summary>
    private void InjectMetadata(L5X content)
    {
        content.Serialize().SetAttributeValue("SourceId", SourceId.ToString());
    }

    /// <summary>
    /// Removes unused L5K data from the source content. This data can add a lot to the total space of the source, and
    /// we want to do our best to conserve that, so we can basically delete a lot of info that is uneeded. The file
    /// can still be restored without L5K data anyway. We are only interested in decorated clear text data.
    /// </summary>
    /// <param name="content">The L5X content from which to scrub the data.</param>
    private static void ScrubContent(L5X content)
    {
        content.Serialize().Descendants(L5XName.Data)
            .Where(d => d.Attribute(L5XName.Format)?.Value == "L5K")
            .Remove();
    }
}