using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

/// <summary>
/// Represents the type of logical chain used to combine criterion for filtering.
/// </summary>
public class Chain : SmartEnum<Chain, int>
{
    private Chain(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Represents an AND logic operation between two boolean results.
    /// </summary>
    public static readonly Chain And = new(nameof(And), 0);

    /// <summary>
    /// Represents an OR logic operation between two boolean results.
    /// </summary>
    public static readonly Chain Or = new(nameof(Or), 1);
}