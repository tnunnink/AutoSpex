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
    public static readonly ResultState Passed = new(nameof(Passed), 3);
    public static readonly ResultState Failed = new(nameof(Failed), 4);
    public static readonly ResultState Errored = new(nameof(Errored), 5);

    /// <summary>
    /// Indicates whether the current ResultState is in a processing state, which includes Pending or Running states.
    /// </summary>
    public bool IsProcessing => Value is 1 or 2;

    /// <summary>
    /// Indicates that the <see cref="ResultState"/> has been determined (result of running a spec). This means
    /// it is either passed, failed, or errored.
    /// </summary>
    public bool IsCompleted => Value > 2;

    /// <summary>
    /// Gets the maximum result from a collection of values or returns a default value if the collection is empty.
    /// </summary>
    /// <param name="states">The collection of ResultState values.</param>
    /// <param name="defaultState">The default ResultState value to return if the collection is empty.
    /// If not provided the default is <see cref="Failed"/>.</param>
    /// <returns>The maximum ResultState value from the collection or the default value if the collection is empty.</returns>
    public static ResultState MaxOrDefault(ICollection<ResultState> states, ResultState? defaultState = null)
    {
        return states.Count > 0 ? FromValue(states.Max(x => x.Value)) : defaultState ?? Failed;
    }
}