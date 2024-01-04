﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Features.Criteria;
using AutoSpex.Client.Features.Elements;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Specifications;

[UsedImplicitly]
public partial class SpecificationViewModel : NodeDetailViewModel, IRecipient<ElementSelectedMessage>,
    IRecipient<RemoveCriterionMessage>
{
    private Specification _spec;
    private bool _isActive = false;

    public SpecificationViewModel(Node node) : base(node)
    {
        Messenger.RegisterAll(this, Node.NodeId);
        ElementMenu = new ElementMenuViewModel(Node.NodeId);

        Filters.CollectionChanged += OnCriterionCollectionChanged;
        Verifications.CollectionChanged += OnCriterionCollectionChanged;
    }

    [ObservableProperty] private ElementMenuViewModel _elementMenu;

    [ObservableProperty] private Element? _element;

    [ObservableProperty] private RunResult? _result;


    public ObservableCollection<Criterion> Test
    {
        get => new(_spec.Filters);
        set { SetProperty(_spec.Filters, value, _spec, (s, t) => s.WithFilters(t.ToList())); }
    }

    public ObservableCollection<CriterionViewModel> Filters { get; } = new();
    public ObservableCollection<CriterionViewModel> Verifications { get; } = new();

    protected override async Task Load()
    {
        if (_isActive) return;

        var result = await Mediator.Send(new LoadSpecRequest(this));

        Track();

        _isActive = true;
    }

    protected override async Task Save()
    {
        var result = await Mediator.Send(new SaveSpecRequest(this));

        if (result.IsSuccess)
        {
            AcceptChanges();
        }
    }

    protected override bool CanSave() => !HasErrors
                                         && Filters.All(f => !f.HasErrors)
                                         && Verifications.All(f => !f.HasErrors)
                                         && IsChanged;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task Run()
    {
        throw new NotImplementedException();
    }

    private bool CanRun() => false;

    [RelayCommand]
    private void AddFilter()
    {
        if (Element is null) return;
        var criterion = new CriterionViewModel(Node.NodeId, Element, CriterionUsage.Filter);
        Filters.Add(criterion);
    }

    [RelayCommand]
    private void AddVerification()
    {
        if (Element is null) return;
        var criterion = new CriterionViewModel(Node.NodeId, Element, CriterionUsage.Verification);
        Verifications.Add(criterion);
    }

    partial void OnElementChanged(Element? oldValue, Element? newValue)
    {
        Messenger.Send(new PropertyChangedMessage<Element?>(this, "Element", oldValue, newValue));
    }

    public void Receive(ElementSelectedMessage message)
    {
        Element = message.Element;
    }

    public void Receive(RemoveCriterionMessage message)
    {
        var criterion = message.Criterion;

        if (criterion.Usage == CriterionUsage.Filter)
        {
            Filters.Remove(criterion);
        }

        if (message.Criterion.Usage == CriterionUsage.Verification)
        {
            Verifications.Remove(criterion);
        }

        criterion.Dispose();
    }

    private void OnCriterionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
        {
            var criteria = e.NewItems?.OfType<CriterionViewModel>();
            if (criteria == null) return;
            foreach (var criterion in criteria)
            {
                Track(criterion);
            }
        }

        // ReSharper disable once InvertIf
        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace)
        {
            var criteria = e.OldItems?.OfType<CriterionViewModel>();
            if (criteria == null) return;
            foreach (var criterion in criteria)
            {
                Forget(criterion);
            }
        }
    }
}