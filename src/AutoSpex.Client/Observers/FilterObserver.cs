using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class FilterObserver : StepObserver<Filter>
{
    public FilterObserver(Filter model) : base(model)
    {
        Track(nameof(Match));
    }

    /// <summary>
    /// Gets or sets the match type for filtering.
    /// </summary>
    public Match Match
    {
        get => Model.Match;
        set => SetProperty(Model.Match, value, Model, (s, v) => s.Match = v);
    }

    /// <summary>
    /// Command to toggle the match option for this filter.
    /// </summary>
    [RelayCommand]
    private void ToggleMatch()
    {
        Match = Match == Match.All ? Match.Any : Match.All;
    }
}