using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class ResultState : SmartEnum<ResultState, int>
{
    private ResultState(string name, int value) : base(name, value)
    {
    }


    public static readonly ResultState None = new(nameof(None), 0);
    public static readonly ResultState Pending = new(nameof(Pending), 1);
    public static readonly ResultState Running = new(nameof(Running), 2);
    public static readonly ResultState Inconclusive = new(nameof(Inconclusive), 3);
    public static readonly ResultState Passed = new(nameof(Passed), 4);
    public static readonly ResultState Failed = new(nameof(Failed), 5);
    public static readonly ResultState Errored = new(nameof(Errored), 6);

    /// <summary>
    /// Gets the maximum value of a collection of ResultState values or returns a default value if the collection is empty.
    /// </summary>
    /// <param name="states">The collection of ResultState values.</param>
    /// <param name="defaultState">The default ResultState value to return if the collection is empty.</param>
    /// <returns>The maximum ResultState value from the collection or the default value if the collection is empty.</returns>
    public static ResultState MaxOrDefault(ICollection<ResultState> states, ResultState? defaultState = default)
    {
        return states.Count > 0 ? FromValue(states.Max(x => x.Value)) : defaultState ?? Inconclusive;
    }

    /// <summary>
    /// Gets the minimum value of a collection of ResultState values or returns a default value if the collection is empty.
    /// </summary>
    /// <param name="states">The collection of ResultState values.</param>
    /// <param name="defaultState">The default ResultState value to return if the collection is empty.</param>
    /// <returns>The minimum ResultState value from the collection or the default value if the collection is empty.</returns>
    public static ResultState MinOrDefault(ICollection<ResultState> states, ResultState? defaultState = default)
    {
        return states.Count > 0 ? FromValue(states.Min(x => x.Value)) : defaultState ?? Inconclusive;
    }
}