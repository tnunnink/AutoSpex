using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Action = AutoSpex.Engine.Action;

namespace AutoSpex.Client.Observers;

public partial class OutcomeObserver : Observer<Outcome>,
    IRecipient<OutcomeObserver.Running>,
    IRecipient<OutcomeObserver.Complete>
{
    /// <inheritdoc/>
    public OutcomeObserver(Outcome model) : base(model)
    {
        Result = model.Result;
        Node = GetObserver<NodeObserver>(x => x.Id == Model.NodeId);
        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(
            refresh: () => Model.Evaluations.Select(e => new EvaluationObserver(e)).ToList(),
            count: () => Model.Evaluations.Count
        );

        RegisterDisposable(Node);
        RegisterDisposable(Evaluations);
    }

    protected override bool PromptForDeletion => false;
    public override Guid Id => Model.OutcomeId;
    public override string Name => Model.Name;

    [ObservableProperty] private NodeObserver? _node;

    [ObservableProperty] private ResultState _result = ResultState.None;

    [ObservableProperty] private ResultState _filterState = ResultState.None;
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }
    public string Duration => $"{Model.Duration} ms";
    public string PassRate => $"{Model.PassRate:F1} %";
    public int Count => GetFilterCount();
    public IEnumerable<ResultState> States => GetDistinctStates();

    /// <inheritdoc />
    /// <remarks>Sends the message that is handled by the page containing this outcome instance.</remarks>
    protected override Task Navigate()
    {
        Messenger.Send(new Show(this));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        //Set text for highlighting from the UI.
        FilterText = filter;

        //Check matches against name and node path.
        return string.IsNullOrEmpty(filter)
               || Name.Satisfies(filter)
               || Node?.Filter(filter) is true;
    }

    [RelayCommand(CanExecute = nameof(CanAddSuppression))]
    private async Task AddSuppression(string? reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            //todo prompt for reason
            /*reason = await Prompter.Show<string?>(new () => )*/
        }

        if (string.IsNullOrEmpty(reason)) return;

        var run = GetObserver<RunObserver>(r => r.Model.Outcomes.Any(x => x.OutcomeId == Id));
        if (run is null) return;

        var fule = Action.Suppress(Model.NodeId, reason);
        var result = await Mediator.Send(new AddSuppression(run.Source.Id, fule));
        Notifier.ShowIfFailed(result);
    }

    private bool CanAddSuppression() => Result.Value > ResultState.Passed.Value;

    /// <summary>
    /// When the selected filter state changes refresh the visible evaluations.
    /// </summary>
    partial void OnFilterStateChanged(ResultState value)
    {
        Evaluations.Filter(x => value == ResultState.None || x.Result == value);
        OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// When we receive the running message for the outcome with the same local id, then we want to set the result
    /// state to pending to notify the UI which outcome is processing.
    /// </summary>
    public void Receive(Running message)
    {
        if (Model.OutcomeId != message.Outcome.OutcomeId) return;

        Dispatcher.UIThread.Invoke(() => { Result = ResultState.Running; });
    }

    /// <summary>
    /// When we receive the complete message and the outcome is the same as the underlying model, then we want to
    /// update the local state to refresh/notify the Ui the outcome has been processed.
    /// </summary>
    public void Receive(Complete message)
    {
        if (Model.OutcomeId != message.Outcome.OutcomeId) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            Result = message.Outcome.Result;
            Evaluations.Refresh();
            Refresh();
        });
    }

    /// <summary>
    /// A message send that indicates an outcome is about to be run.
    /// </summary>
    /// <param name="Outcome">The outcome that is about to run.</param>
    public record Running(Outcome Outcome);

    /// <summary>
    /// A message sent that indicates an outcome just completed running.
    /// </summary>
    /// <param name="Outcome">The outcome produced by the run.</param>
    public record Complete(Outcome Outcome);

    /// <summary>
    /// A message that will notify a page to show this observer instance and its results.
    /// </summary>
    /// <param name="Outcome">The outcome to show.</param>
    public record Show(OutcomeObserver Outcome);

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Suppress",
            Icon = Resource.Find("IconLineBan"),
            Command = AddSuppressionCommand,
            DetermineVisibility = () => Result.IsFaulted
        };

        yield return new MenuActionItem
        {
            Header = "Override",
            Icon = Resource.Find("IconFilledPencil"),
            Command = AddSuppressionCommand,
            DetermineVisibility = () => Result.IsFaulted
        };

        yield return new MenuActionItem
        {
            Header = $"Open {Node?.Type}",
            Icon = Resource.Find("IconLineLaunch"),
            Command = Node?.NavigateCommand,
            DetermineVisibility = () => Node is not null
        };
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Suppress",
            Icon = Resource.Find("IconLineBan"),
            Command = AddSuppressionCommand,
            DetermineVisibility = () => Result.IsFaulted
        };

        yield return new MenuActionItem
        {
            Header = "Override",
            Icon = Resource.Find("IconFilledPencil"),
            Command = AddSuppressionCommand,
            DetermineVisibility = () => Result.IsFaulted
        };

        yield return new MenuActionItem
        {
            Header = $"Open {Node?.Type}",
            Icon = Resource.Find("IconLineLaunch"),
            Command = Node?.NavigateCommand,
            DetermineVisibility = () => Node is not null
        };
    }

    /// <summary>
    /// Gets the numer of evaluations based on the current filter state.
    /// </summary>
    private int GetFilterCount()
    {
        return FilterState != ResultState.None
            ? Model.Evaluations.Count(e => e.Result == FilterState)
            : Model.Evaluations.Count;
    }

    /// <summary>
    /// Gets distinct result states of this outcome's evaluations for filter selection.
    /// </summary>
    private List<ResultState> GetDistinctStates()
    {
        var states = new List<ResultState> { ResultState.None };
        states.AddRange(Model.Evaluations.Select(x => x.Result).Distinct());
        return states.OrderBy(r => r.Value).ToList();
    }

    public static implicit operator Outcome(OutcomeObserver observer) => observer.Model;
    public static implicit operator OutcomeObserver(Outcome model) => new(model);
}