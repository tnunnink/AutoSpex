using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>, IRecipient<CriterionObserver.RemoveMessage>
{
    public SpecObserver(Spec model) : base(model)
    {
        Filters = new ObserverCollection<Criterion, CriterionObserver>(
            Model.Filters,
            m => new CriterionObserver(m, Model.Element.Type));

        Verifications = new ObserverCollection<Criterion, CriterionObserver>(
            Model.Verifications,
            m => new CriterionObserver(m, Model.Element.Type));

        Track(nameof(Element));
        Track(Filters);
        Track(Verifications);

        Messenger.RegisterAll(this);
    }

    public override bool IsChanged => base.IsChanged || Filters.IsChanged || Verifications.IsChanged;

    [Required]
    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, UpdateElement, validate: true);
    }

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; }

    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;

    public void Receive(CriterionObserver.RemoveMessage message)
    {
        Filters.Remove(message.Criterion);
        Verifications.Remove(message.Criterion);
    }

    private void UpdateElement(Spec spec, Element element)
    {
        spec.Element = element;

        foreach (var criterion in Filters)
            criterion.Type = spec.Element.Type;

        foreach (var criterion in Verifications)
            criterion.Type = spec.Element.Type;
    }
}