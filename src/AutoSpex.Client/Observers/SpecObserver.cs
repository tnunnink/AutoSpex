using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>, IRecipient<CriterionObserver.SpecRequest>
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

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; }

    public static SpecObserver Empty => new(new Spec());
    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;

    /// <summary>
    /// Handles the spec request message sent be a child criterion. If this spec contains the criterion id provided
    /// in the message and has not been responded to, then reply with this spec instance as the parent.
    /// </summary>
    public void Receive(CriterionObserver.SpecRequest message)
    {
        if (message.HasReceivedResponse) return;

        if (Filters.Any(x => x.Id == message.CriterionId) || Verifications.Any(x => x.Id == message.CriterionId))
        {
            message.Reply(this);
        }
    }
}