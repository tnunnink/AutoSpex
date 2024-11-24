using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class FilterObserver : StepObserver<Filter>
{
    public FilterObserver(Filter model) : base(model)
    {
        Track(nameof(Match));
    }

    public Match Match
    {
        get => Model.Match;
        set => SetProperty(Model.Match, value, Model, (s, v) => s.Match = v);
    }
}