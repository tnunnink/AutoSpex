using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class ResultFilterPageModel(RunDetailPageModel page) : PageViewModel("Filters")
{
    private readonly HashSet<ResultState> _resultFilters = [];
    private readonly HashSet<Guid> _sourceFilters = [];
    
    public override string Route => $"{page.Route}/{Title}";

    [ObservableProperty] private bool _applyFilters;
    
    [ObservableProperty] private string? _filterText;
    
    public ObservableCollection<ResultState> States { get; } =
        [ResultState.Passed, ResultState.Failed, ResultState.Errored, ResultState.Inconclusive];
    
    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(FilterText))
        {
            Filter(FilterText, _resultFilters, _sourceFilters);
        }
    }
    
    /// <summary>
    /// Command to add the provided result state to the configured state filters and call the filter results function
    /// to update the visible outcomes.
    /// </summary>
    /// <param name="result">The result state to add to the filter collection.</param>
    [RelayCommand]
    private void FilterByResult(ResultState result)
    {
        //Try to add the result filter, and if already present then remove it (toggle). 
        if (!_resultFilters.Add(result))
        {
            _resultFilters.Remove(result);
        }

        Filter(FilterText, _resultFilters, _sourceFilters);
    }
    
    /// <summary>
    /// Filters the current set of outcome result objects and their nested evaluations using the configured
    /// filter criteria (keyword, states, sources). 
    /// </summary>
    private void Filter(string? keyword, HashSet<ResultState> results, HashSet<Guid> sources)
    {
        page.Outcomes.Filter(o =>
        {
            //Filter the observer collection in place using the configured filter criteria.
            o.Evaluations.Filter(e =>
                e.Filter(keyword) &&
                (results.Count == 0 || results.Contains(e.Result)) /*&&
                (sources.Count == 0 || sources.Contains(e.Model.SourceId))*/
            );

            //Then filter the outcome using the entered text.
            var passes = o.Filter(keyword);

            //Expand only when we do keyword search and there are child evaluations with a match.
            o.IsExpanded = !string.IsNullOrEmpty(keyword) && o.Evaluations.Count > 0;

            return passes || o.Evaluations.Count > 0;
        });
    }
}