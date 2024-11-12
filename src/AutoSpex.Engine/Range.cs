using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a range of values defined by a minimum and maximum value.
/// </summary>
public class Range
{
    /// <summary>
    /// Creates a new default <see cref="Range"/> instance.
    /// </summary>
    public Range()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Range"/> initialized with the provided min and max values.
    /// </summary>
    /// <param name="min">The minimum value for the range.</param>
    /// <param name="max">The maximum value for the range.</param>
    public Range(object? min, object? max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Gets or sets the minimum value for the range.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    public object? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for the range.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    public object? Max { get; set; }

    /// <summary>
    /// Checks if the given value is within the specified range defined by Min and Max values.
    /// </summary>
    /// <param name="value">The value to check if it is within the range.</param>
    /// <returns>True if the value is within the range, false otherwise.</returns>
    public bool InRange(object? value)
    {
        if (value is not IComparable comparable) return false;
        return comparable.CompareTo(Min) >= 0 && comparable.CompareTo(Max) <= 0;
    }

    public override string ToString() => $"{Min} and {Max}";
}