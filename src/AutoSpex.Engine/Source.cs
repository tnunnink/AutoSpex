using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a source L5X content that can be run against any set of specifications.
/// Source content will be stored locally and the user can inspect, update, and create runs against the source.
/// </summary>
public class Source
{
    private readonly Dictionary<Guid, Variable> _overrides = [];

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
    /// Gets the collection of <see cref="Variable"/> objects representing the overrides that allow the user to
    /// change the input data to variables that are referenced on any node in the project.
    /// </summary>
    [JsonInclude]
    public IEnumerable<Variable> Overrides => _overrides.Values;

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
    /// Adds an override value for a specified variable to this source.
    /// </summary>
    /// <param name="variable">The overriden variable object.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variable"/> is null.</exception>
    public void AddOverride(Variable variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        _overrides[variable.VariableId] = variable;
    }

    /// <summary>
    /// Removes the specified override variable from the source. This will delete the override variable from the internal collection.
    /// </summary>
    /// <param name="variable">The variable to remove from the overrides.</param>
    public void RemoveOverride(Variable variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        _overrides.Remove(variable.VariableId);
    }

    /// <summary>
    /// Clears all overrides stored in the source.
    /// </summary>
    public void ClearOverrides() => _overrides.Clear();

    /// <summary>
    /// Overrides the values of the provided variables using the configured <see cref="Overrides"/> collection of the source.
    /// </summary>
    /// <param name="variables">The variables whose values should be overridden.</param>
    public void Override(IEnumerable<Variable> variables)
    {
        foreach (var variable in variables)
        {
            if (!_overrides.TryGetValue(variable.VariableId, out var match)) continue;
            variable.Value = match.Value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Source Duplicate()
    {
        var duplicate = new Source(Content);

        foreach (var variable in _overrides.Values)
        {
            duplicate.AddOverride(variable);
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
    /// Adds/sets the source metadata to the L5X content.
    /// This is so we can read back this data from the element without having to find the source information.
    /// </summary>
    private void InjectMetadata(L5X content)
    {
        content.Serialize().SetAttributeValue("SourceId", SourceId.ToString());
    }
}