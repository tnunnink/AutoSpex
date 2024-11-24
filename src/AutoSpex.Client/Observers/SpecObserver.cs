using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>, IRecipient<Observer.Get<SpecObserver>>
{
    public SpecObserver(Spec model) : base(model)
    {
        Steps = new ObserverCollection<Step, StepObserver>
        (
            refresh: () => Model.Steps.Select(InstantiateStep).ToList(),
            add: (_, s) => Model.AddStep(s),
            remove: (_, s) => Model.RemoveStep(s),
            count: () => Model.Steps.Count()
        );

        Track(Steps);
    }

    public override Guid Id => Model.SpecId;
    public ObserverCollection<Step, StepObserver> Steps { get; }

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

    private static StepObserver InstantiateStep(Step step)
    {
        return step switch
        {
            Query query => new QueryObserver(query),
            Filter filter => new FilterObserver(filter),
            Select select => new SelectObserver(select),
            Verify verify => new VerifyObserver(verify),
            _ => throw new ArgumentOutOfRangeException(nameof(step))
        };
    }

    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}