using System.Collections.Specialized;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class FilterObserver : StepObserver<Filter>
{
    public FilterObserver(Filter model) : base(model)
    {
        Track(nameof(Match));

        Criteria.CollectionChanged += OnCriteriaChanged;
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
    /// Only show the match option when we have multiple criteria for this step.
    /// </summary>
    public bool ShowMatch => Criteria.Count > 1;

    /// <summary>
    /// Command to toggle the match option for this filter.
    /// </summary>
    [RelayCommand]
    private void ToggleMatch()
    {
        Match = Match == Match.All ? Match.Any : Match.All;
    }
    
    /// <inheritdoc />
    /// <remarks>Also unsubscribe from the change event.</remarks>
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Criteria.CollectionChanged -= OnCriteriaChanged;
    }
    
    /// <summary>
    /// When the criteria collection changes for a filter step we want to react by changing ths visuals.
    /// Refresh the binding for ShowMatch to allow the user to display the match button.
    /// If There are no longer any criteria automatically delete the entire step.
    /// </summary>
    private void OnCriteriaChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowMatch));

        if (Criteria.Count == 0)
        {
            DeleteCommand.Execute(null);
        }
    }
}