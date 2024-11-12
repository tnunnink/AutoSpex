using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class RangeObserver : Observer<Range>
{
    public RangeObserver(Range model) : base(model)
    {
        Min = new ValueObserver(() => Model.Min, ParseValue, v => Model.Min = v);
        Max = new ValueObserver(() => Model.Max, ParseValue, v => Model.Max = v);

        Track(Min);
        Track(Max);
    }

    public ValueObserver Min { get; }
    public ValueObserver Max { get; }

    /// <summary>
    /// The function that parses input values into the actual underlying range min/max value we expect.
    /// all range values should be a date or a number, so we can use those type groups explicitely.
    /// If the provided value is another value observer, which comes from suggesstions, then use that value.
    /// </summary>
    private static object? ParseValue(object? value)
    {
        if (value is ValueObserver observer)
            return observer.Value;

        if (value is not string text) return value;

        if (TypeGroup.Date.TryParse(text, out var date)) return date;
        if (TypeGroup.Number.TryParse(text, out var number)) return number;
        return value;
    }
}