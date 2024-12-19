using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class VerifyObserver : StepObserver<Verify>,
    IRecipient<Observer.GetSelected>
{
    public VerifyObserver(Verify model) : base(model)
    {
        Criteria = new ObserverCollection<Criterion, CriterionObserver>(Model.Criteria,
            c => new CriterionObserver(c, DetermineInput)
        );

        Track(Criteria);
    }

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// </summary>
    public ObserverCollection<Criterion, CriterionObserver> Criteria { get; }

    /// <summary>
    /// The collection of criteria that are selected from the UI.
    /// </summary>
    public ObservableCollection<CriterionObserver> Selected { get; } = [];

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
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Paste Criteria",
            Icon = Resource.Find("IconFilledPaste"),
            Command = PasteCommand
        };

        yield return new MenuActionItem
        {
            Header = "Copy Criteria",
            Icon = Resource.Find("IconFilledCopy"),
            Command = CopyCommand
        };

        yield return new MenuActionItem
        {
            Header = "Clear Criteria",
            Icon = Resource.Find("IconFilledTrash"),
            Command = DeleteCommand
        };
    }
}