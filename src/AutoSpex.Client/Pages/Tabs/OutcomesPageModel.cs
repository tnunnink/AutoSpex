﻿using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class OutcomesPageModel : PageViewModel
{
    /// <inheritdoc/>
    public OutcomesPageModel(RunObserver run) : base("Outcomes")
    {
        Run = run;
    }

    public override string Route => $"{nameof(Run)}/{Run.Id}/{Title}";
    public RunObserver Run { get; }

    [ObservableProperty] private OutcomeObserver? _outcome;

    [ObservableProperty] private string? _filter;

    [ObservableProperty] private bool _showResults;

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnOutcomeChanged(OutcomeObserver? value)
    {
        ShowResults = true;
    }

    partial void OnFilterChanged(string? value)
    {
        if (Outcome is null) return;

        var state = Outcome.FilterState;
        var text = value;
        
        Outcome.Evaluations.Filter(x =>
        {
            var hasState = state == ResultState.None || x.Result == state;
            var hasText = x.Filter(text);
            return hasState && hasText;
        });
    }
}