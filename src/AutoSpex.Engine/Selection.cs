using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A type that defines a property selection and an optional alias name. This is to assist with builing complex dynamic
/// objects that are subsets of certain properties from complex element types.
/// </summary>
public class Selection()
{
    private string _alias = string.Empty;
    private string _property = string.Empty;

    /// <summary>
    /// Creates a new <see cref="Selection"/> with the specified property and optional alias.
    /// </summary>
    /// <param name="property">The property path that represents the selection.</param>
    /// <param name="alias">The optional alias name to use as the key for this selection. If not provided it will be set
    /// using the provided property name.</param>
    public Selection(string property, string? alias = default) : this()
    {
        Property = property;
        if (alias is not null) Alias = alias;
    }

    /// <summary>
    /// Creates a new <see cref="Selection"/> from the specified property and optional alias.
    /// </summary>
    /// <param name="property">The property path that represents the selection.</param>
    public Selection(Property property) : this()
    {
        ArgumentNullException.ThrowIfNull(property);
        Property = property.Path;
    }

    /// <summary>
    /// Gets or sets the alias name to use as the key for the selection.
    /// </summary>
    [JsonInclude]
    public string Alias
    {
        get => _alias;
        set => SetAlias(value);
    }

    /// <summary>
    /// Represents a property that can be selected for a dynamic object.
    /// </summary>
    [JsonInclude]
    public string Property
    {
        get => _property;
        set => SetProperty(value);
    }

    /// <summary>
    /// Creates a new <see cref="Selection"/> with the specified property.
    /// </summary>
    /// <param name="property">The property path that represents the selection.</param>
    /// <returns>A new <see cref="Selection"/> object with the specified property.</returns>
    public static Selection Select(string property) => new(property);

    /// <summary>
    /// Creates a new <see cref="Selection"/> with the specified alias for the property.
    /// </summary>
    /// <param name="alias">The optional alias name to use as the key for this selection.</param>
    /// <returns>A new <see cref="Selection"/> object with the specified alias.</returns>
    public Selection As(string alias) => new(Property, alias);

    /// <summary>
    /// Sets the alias name for the selection.
    /// </summary>
    /// <param name="alias">The alias name to be set for the selection. Must not be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown when the provided alias is null, empty, or contains the property separator character.</exception>
    private void SetAlias(string alias)
    {
        _alias = alias;
    }

    /// <summary>
    /// Sets the property path representing the selection. If the provided property path is null or empty, an ArgumentException
    /// will be thrown. If no alias is set, it will be automatically generated from the property path.
    /// </summary>
    /// <param name="property">The property path to set for the selection.</param>
    /// <exception cref="ArgumentException">Thrown when the provided property path is null or empty.</exception>
    private void SetProperty(string property)
    {
        _property = property;

        if (string.IsNullOrEmpty(_alias))
        {
            SetAlias(ExtractAlias(_property));
        }
    }

    /// <summary>
    /// Determines a valid alias name from the provided property name.
    /// </summary>
    private static string ExtractAlias(string property)
    {
        var index = property.LastIndexOfAny(Engine.Property.Separators) + 1;
        var alias = index > 0 ? property[index..] : property;
        return alias.TrimEnd(']');
    }
}