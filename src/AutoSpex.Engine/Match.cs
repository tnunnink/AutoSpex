using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

/// <summary>
/// An option for selecting whether to match all or any criterion in a collection of criteria. 
/// </summary>
public class Match : SmartEnum<Match, int>
{
    private Match(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Represents an option to match all criteria in a collection of criteria.
    /// </summary>
    public static readonly Match All = new(nameof(All), 0);

    /// <summary>
    /// Represents an option to match any criteria in a collection of criteria.
    /// </summary>
    public static readonly Match Any = new(nameof(Any), 1);
}