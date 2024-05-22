using System;
using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>
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
        Track(nameof(VerifyCount));
        Track(nameof(CountOperation));
        Track(nameof(CountValue));
        Track(nameof(FilterInclusion));
        Track(nameof(VerificationInclusion));
        Track(Filters);
        Track(Verifications);
    }

    public override Guid Id => Model.SpecId;

    [Required]
    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, (s, e) => s.Element = e, validate: true);
    }

    public bool VerifyCount
    {
        get => Model.Settings.VerifyCount;
        set => SetProperty(Model.Settings.VerifyCount, value, Model, (s, v) => s.Settings.VerifyCount = v);
    }

    public Operation CountOperation
    {
        get => Model.Settings.CountOperation;
        set => SetProperty(Model.Settings.CountOperation, value, Model, (s, v) => s.Settings.CountOperation = v);
    }

    public int CountValue
    {
        get => Model.Settings.CountValue;
        set => SetProperty(Model.Settings.CountValue, value, Model, (s, v) => s.Settings.CountValue = v);
    }

    public Inclusion FilterInclusion
    {
        get => Model.Settings.FilterInclusion;
        set => SetProperty(Model.Settings.FilterInclusion, value, Model, (s, v) => s.Settings.FilterInclusion = v);
    }

    public Inclusion VerificationInclusion
    {
        get => Model.Settings.VerificationInclusion;
        set => SetProperty(Model.Settings.VerificationInclusion, value, Model,
            (s, v) => s.Settings.VerificationInclusion = v);
    }

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; }

    public static SpecObserver Empty => new(new Spec());
    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}