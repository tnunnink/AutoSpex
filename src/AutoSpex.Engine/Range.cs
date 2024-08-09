using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// A custom config object that is part of a specification that defines how to check the resulting number of candidate
/// objects that pass the specification filter. This allows us to add a special verification for the number of objects
/// the user expect to be returned.
/// </summary>
public class Range
{
    /// <summary>
    /// The default count property of a list of elements.
    /// </summary>
    private static readonly Property Count = Property.This(typeof(List<LogixElement>)).GetProperty("Count")!;
    
    /// <summary>
    /// Whether the range verification is enabled, which will control whether it is run for a specification.
    /// </summary>
    [JsonInclude]
    public bool Enabled { get; set; }
    
    /// <summary>
    /// The <see cref="Criterion"/> defining the evaluation to be performed on the count property of a list of elements.
    /// The property value should not change but the operation and arguments can be set as desired.
    /// </summary>
    [JsonInclude]
    public Criterion Criterion { get; private set; } = new(Count, Operation.GreaterThan, 0);
}