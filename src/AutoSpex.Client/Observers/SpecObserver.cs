using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class SpecObserver : Observer<Spec>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Request<SpecObserver>>
{
    public SpecObserver(Spec model) : base(model)
    {
        Filters = new ObserverCollection<Criterion, CriterionObserver>(
            Model.Filters, m => new CriterionObserver(m));

        Verifications = new ObserverCollection<Criterion, CriterionObserver>(
            Model.Verifications, m => new CriterionObserver(m));

        Range = Model.Range.Criterion;

        Track(nameof(Element));
        Track(nameof(FilterInclusion));
        Track(nameof(VerificationInclusion));
        Track(nameof(RangeEnabled));
        Track(Filters);
        Track(Verifications);
        Track(Range);
    }

    public override Guid Id => Model.SpecId;
    public NodeObserver Node => Model.Node;
    public override string Name => Model.Name;

    public Element Element
    {
        get => Model.Query.Element;
        set => SetProperty(Model.Query.Element, value, Model, (s, v) => s.Query.Element = v);
    }

    public string? ElementName
    {
        get => Model.Query.Name;
        set => SetProperty(Model.Query.Name, value, Model, (s, v) => s.Query.Name = v);
    }

    public Inclusion FilterInclusion
    {
        get => Model.FilterInclusion;
        set => SetProperty(Model.FilterInclusion, value, Model, (s, v) => s.FilterInclusion = v);
    }

    public Inclusion VerificationInclusion
    {
        get => Model.VerificationInclusion;
        set => SetProperty(Model.VerificationInclusion, value, Model, (s, v) => s.VerificationInclusion = v);
    }

    public bool RangeEnabled
    {
        get => Model.Range.Enabled;
        set => SetProperty(Model.Range.Enabled, value, Model, (s, b) => s.Range.Enabled = b);
    }

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; }
    public CriterionObserver Range { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Override to navigated the loaded node observer.
    /// </remarks>
    protected override Task Navigate()
    {
        return Navigator.Navigate(Node);
    }

    /// <summary>
    /// Updates the specification query element type and resets all the criteria. We may not do this in the future
    /// if I can get better error handling for the criterion observer. 
    /// </summary>
    [RelayCommand]
    private void UpdateElement(Element? element)
    {
        if (element is null) return;

        Element = element;

        Filters.Clear();
        Verifications.Clear();

        AddFilterCommand.NotifyCanExecuteChanged();
        AddVerificationCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Adds a filter to the specification.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddCriteria))]
    private void AddFilter()
    {
        if (Element == Element.Default) return;
        Filters.Add(new CriterionObserver(new Criterion(Element.Type)));
    }

    /// <summary>
    /// Adds a verification to the specification.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddCriteria))]
    private void AddVerification()
    {
        if (Element == Element.Default) return;
        Verifications.Add(new CriterionObserver(new Criterion(Element.Type)));
    }

    /// <summary>
    /// Determines if we can add and filter or verifications which should depend on if an element is selected.
    /// </summary>
    private bool CanAddCriteria() => Element != Element.Default;

    /// <summary>
    /// Changes the state of the filter inclusion property to the opposite value.
    /// </summary>
    [RelayCommand]
    private void ToggleFilterInclusion()
    {
        FilterInclusion = FilterInclusion == Inclusion.All ? Inclusion.Any : Inclusion.All;
    }

    /// <summary>
    /// Changes the state of the filter inclusion property to the opposite value.
    /// </summary>
    [RelayCommand]
    private void ToggleVerificationInclusion()
    {
        VerificationInclusion = VerificationInclusion == Inclusion.All ? Inclusion.Any : Inclusion.All;
    }

    /// <summary>
    /// Opens the source explorer dialog to allow the user to select content from a source.
    /// </summary>
    [RelayCommand]
    private Task OpenSourceExplorer()
    {
        return Prompter.Show(() => new SourceExplorerPageModel(Element));
    }

    /// <summary>
    /// If a criterion delete message is received we will delete all selected criterion from the list.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not CriterionObserver observer) return;

        RemoveSelectedFilters(observer);
        RemoveSelectedVerifications(observer);
    }

    /// <summary>
    /// If the request matches the requirements then return this spec observer instance.
    /// We want to handle requests for the spec id, or and id of a nested criterion or argument.
    /// </summary>
    public void Receive(Request<SpecObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (Id == message.Id)
        {
            message.Reply(this);
        }

        if (Filters.Any(c => c.Id == message.Id) ||
            Filters.SelectMany(x => x.Arguments).Any(a => a.Id == message.Id))
        {
            message.Reply(this);
        }

        if (Verifications.Any(c => c.Id == message.Id) ||
            Verifications.SelectMany(x => x.Arguments).Any(a => a.Id == message.Id))
        {
            message.Reply(this);
        }
    }

    private void RemoveSelectedFilters(CriterionObserver observer)
    {
        if (!Filters.Contains(observer)) return;

        var selected = Filters.Where(x => x.IsSelected).ToList();

        foreach (var filter in selected)
            Filters.Remove(filter);
    }

    private void RemoveSelectedVerifications(CriterionObserver observer)
    {
        if (!Verifications.Contains(observer)) return;

        var selected = Verifications.Where(x => x.IsSelected).ToList();

        foreach (var filter in selected)
            Verifications.Remove(filter);
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Open",
            Icon = Resource.Find("IconLineLink"),
            Command = NavigateCommand
        };
        
        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPen"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    public static SpecObserver Empty => new(new Spec());
    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}