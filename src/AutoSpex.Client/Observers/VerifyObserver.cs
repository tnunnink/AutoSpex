using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class VerifyObserver : StepObserver<Verify>
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