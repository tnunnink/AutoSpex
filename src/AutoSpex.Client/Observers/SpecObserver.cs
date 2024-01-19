using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Components;

public partial class SpecObserver : Observer<Spec>
{
    public SpecObserver(Spec model) : base(model)
    {
        Filters = new ObserverCollection<Criterion, CriterionObserver>(Model.Filters, 
            m => new CriterionObserver(m));
        Verifications = new ObserverCollection<Criterion, CriterionObserver>(Model.Verifications,
            m => new CriterionObserver(m));
    }

    public override bool IsChanged => base.IsChanged || Filters.IsChanged || Verifications.IsChanged;

    [Required]
    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, (s, e) => s.Element = e, validate: true);
    }

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; private set; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; private set; }

    public static implicit operator SpecObserver(Spec model) => new(model);

    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}