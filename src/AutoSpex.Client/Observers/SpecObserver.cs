﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class SpecObserver : Observer<Spec>,
    IRecipient<Observer.Get<SpecObserver>>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.GetSelected>
{
    public SpecObserver(Spec model) : base(model)
    {
        Filters = new ObserverCollection<Criterion, CriterionObserver>(
            Model.Filters, m => new CriterionObserver(m));

        Verifications = new ObserverCollection<Criterion, CriterionObserver>(
            Model.Verifications, m => new CriterionObserver(m));

        Track(nameof(Element));
        Track(Filters);
        Track(Verifications);
    }

    public override Guid Id => Model.SpecId;

    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, (s, v) => s.Element = v);
    }

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; }
    public ObservableCollection<CriterionObserver> SelectedFilters { get; } = [];
    public ObservableCollection<CriterionObserver> SelectedVerifications { get; } = [];

    /// <summary>
    /// Updates the specification query element type and resets all the criteria. We may not do this in the future
    /// if I can get better error handling for the criterion observer. 
    /// </summary>
    [RelayCommand]
    private void UpdateElement(Element? element)
    {
        if (element is null) return;

        Element = element;

        UpdateCriterion(Filters, Element);
        UpdateCriterion(Verifications, Element);

        AddFilterCommand.NotifyCanExecuteChanged();
        AddVerificationCommand.NotifyCanExecuteChanged();
    }

    private static void UpdateCriterion(IEnumerable<CriterionObserver> criteria, Element element)
    {
        foreach (var criterion in criteria)
        {
            criterion.Model.Type = element.Type;
            criterion.Property = element.Property(criterion.Property.Path);
        }
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
    /// Command to add the criteria copied to the clipboard to the current spec filters.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddCriteria))]
    private async Task PasteFilters()
    {
        var criteria = await GetClipboardObservers<Criterion>();

        var copies = criteria.Select(c => new CriterionObserver(new Criterion(Element.Type)
        {
            Property = Element.Property(c.Property.Path),
            Negation = c.Negation,
            Operation = c.Operation,
            Argument = c.Argument
        }));

        Filters.AddRange(copies);
    }

    /// <summary>
    /// Command to add the criteria copied to the clipboard to the current spec verifications.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddCriteria))]
    private async Task PasteVerifications()
    {
        var criteria = await GetClipboardObservers<Criterion>();

        var copies = criteria.Select(c => new CriterionObserver(new Criterion(Element.Type)
        {
            Property = Element.Property(c.Property.Path),
            Negation = c.Negation,
            Operation = c.Operation,
            Argument = c.Argument
        }));

        Verifications.AddRange(copies);
    }

    /// <summary>
    /// Determines if we can add and filter or verifications which should depend on if an element is selected.
    /// </summary>
    private bool CanAddCriteria() => Element != Element.Default;

    /// <summary>
    /// Handles the request to get the spec observer that passes the provied predicate. This allows child criteria
    /// to have access to the spec that contains them.
    /// </summary>
    public void Receive(Get<SpecObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate.Invoke(this))
        {
            message.Reply(this);
        }
    }

    /// <summary>
    /// If a criterion delete message is received we will delete all selected criterion from the list.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not CriterionObserver observer) return;

        //Removes only if it exists, so we don't need to perform checks first.
        Filters.RemoveAny(x => x.Id == observer.Id);
        Verifications.RemoveAny(x => x.Id == observer.Id);
    }

    /// <summary>
    /// Handle reception of the get selected message by replying with the parent's selected filters or verifications
    /// depending on the recieved observer instance.
    /// </summary>
    public void Receive(GetSelected message)
    {
        if (message.Observer is not CriterionObserver criterion) return;

        if (Filters.Any(x => x.Is(criterion)))
        {
            foreach (var filter in SelectedFilters)
                message.Reply(filter);
        }

        // ReSharper disable once InvertIf don't care
        if (Verifications.Any(x => x.Is(criterion)))
        {
            foreach (var filter in SelectedVerifications)
                message.Reply(filter);
        }
    }

    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}