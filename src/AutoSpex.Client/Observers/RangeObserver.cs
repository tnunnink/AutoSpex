using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class RangeObserver : Observer<Range>
{
    public RangeObserver(Range model) : base(model)
    {
        Min = new ArgumentInput(() => Model.Min, v => Model.Min = v);
        Max = new ArgumentInput(() => Model.Max, v => Model.Max = v);

        Track(Min);
        Track(Max);
    }

    public ArgumentInput Min { get; }
    public ArgumentInput Max { get; }
}