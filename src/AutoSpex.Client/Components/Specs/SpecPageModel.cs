using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

[UsedImplicitly]
public partial class SpecPageModel : NodePageModel, IRecipient<Messages.ElementSelectedMessage>
{
    private Spec _spec;
    private bool _isActive = false;

    public SpecPageModel(Observers.NodeObserver node) : base(node)
    {
        Messenger.RegisterAll<Guid>(this, Node.Model.NodeId);
        ElementMenu = new ElementMenuViewModel(Node.Model.NodeId);

        /*Filters.CollectionChanged += OnCriterionCollectionChanged;
        Verifications.CollectionChanged += OnCriterionCollectionChanged;*/
    }

    [ObservableProperty] private ElementMenuViewModel _elementMenu;

    [ObservableProperty] private Element? _element;

    [ObservableProperty] private Outcome? _result;

    public ObservableCollection<CriterionViewModel> Filters { get; } = new();
    public ObservableCollection<CriterionViewModel> Verifications { get; } = new();
    

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
        /*if (Element is null) return;
        var criterion = new CriterionViewModel(Node.Model.NodeId, Element, CriterionUsage.Filter);
        Filters.Add(criterion);*/
    }

    [RelayCommand]
    private void AddVerification()
    {
        /*if (Element is null) return;
        var criterion = new CriterionViewModel(Node.NodeId, Element, CriterionUsage.Verification);
        Verifications.Add(criterion);*/
    }

    partial void OnElementChanged(Element? oldValue, Element? newValue)
    {
        Messenger.Send(new PropertyChangedMessage<Element?>(this, "Element", oldValue, newValue));
    }

    public void Receive(Messages.ElementSelectedMessage message)
    {
        Element = message.Element;
    }

    /*private void OnCriterionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
    }*/
}