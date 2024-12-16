using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class FilterObserver : StepObserver<Filter>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.GetSelected>
{
    public FilterObserver(Filter model) : base(model)
    {
        Criteria = new ObserverCollection<Criterion, CriterionObserver>(Model.Criteria,
            c => new CriterionObserver(c, DetermineInput)
        );

        Criteria.CollectionChanged += OnCriteriaChanged;

        Track(Criteria);
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
    /// The collection of <see cref="Criterion"/> that define the step.
    /// </summary>
    public ObserverCollection<Criterion, CriterionObserver> Criteria { get; }

    /// <summary>
    /// The collection of criteria that are selected from the UI.
    /// </summary>
    public ObservableCollection<CriterionObserver> Selected { get; } = [];

    /// <inheritdoc />
    public override bool IsEmpty => !Criteria.HasItems;

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

    /// <summary>
    /// Adds a filter to the specification.
    /// </summary>
    [RelayCommand]
    private void AddCriteria()
    {
        Criteria.Add(new CriterionObserver(new Criterion(), DetermineInput));
    }

    /// <summary>
    /// Command to add the criteria copied to the clipboard to the current step.
    /// </summary>
    [RelayCommand]
    private async Task PasteCriteria()
    {
        var criteria = await GetClipboardObservers<Criterion>();
        var copies = criteria.Select(c => new CriterionObserver(c.Duplicate(), DetermineInput));
        Criteria.AddRange(copies);
    }

    /// <summary>
    /// If a criterion delete message is received we will delete all selected criterion from the list.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not CriterionObserver observer) return;
        Criteria.RemoveAny(x => x.Is(observer));
    }

    /// <summary>
    /// Handle reception of the get selected message by replying with the selected criteria.
    /// </summary>
    public void Receive(GetSelected message)
    {
        if (message.Observer is not CriterionObserver criterion) return;
        if (!Criteria.Any(x => x.Is(criterion))) return;

        foreach (var observer in Selected)
            message.Reply(observer);
    }

    /// <inheritdoc />
    /// <remarks>Also unsubscribe from the change event.</remarks>
    protected override void OnDeactivated()
    {
        Criteria.CollectionChanged -= OnCriteriaChanged;
        base.OnDeactivated();
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