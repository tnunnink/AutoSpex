using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a negation criterion that can be used to determine if a given boolean value satisfies the criterion.
/// </summary>
public class Negation : SmartEnum<Negation, bool>
{
    private Negation(string name, bool value) : base(name, value)
    {
    }

    /// <summary>
    /// Indicates that positive logic (true) should satisfy a given Boolean input.
    /// </summary>
    public static readonly Negation Is = new(nameof(Is), true);

    /// <summary>
    /// Indicates that negative logic (false) should satisfy a given Boolean input.
    /// </summary>
    public static readonly Negation Not = new(nameof(Not), false);

    /// <summary>
    /// Returns the opposite of the current <see cref="Negation"/> value.
    /// </summary>
    public Negation Negate => this == Is ? Not : Is;

    /// <summary>
    /// Determines if a given boolean value satisfies a negation criterion.
    /// If the negation is positive (Is), the value should be true.
    /// If the negation is negative (IsNot), the value should be false.
    /// </summary>
    /// <param name="value">The boolean value to be evaluated.</param>
    /// <returns>
    /// Returns true if the value satisfies the negation criterion.
    /// Returns false if the value does not satisfy the negation criterion.
    /// </returns>
    public bool Satisfies(bool value) => this == Is ? value : !value;
}