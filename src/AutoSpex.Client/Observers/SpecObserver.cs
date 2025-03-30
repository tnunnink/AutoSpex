using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>, IRecipient<Observer.Get<SpecObserver>>, IRecipient<StepObserver.GetInputTo>
{
    public SpecObserver(Spec model) : base(model)
    {
        Query = new QueryObserver(Model.Query);
        Verify = new VerifyObserver(Model.Verify);

        Track(Query);
        Track(Verify);
    }

    public override Guid Id => Model.SpecId;
    public QueryObserver Query { get; }
    public VerifyObserver Verify { get; }

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
    /// Responds to the request for determining the step input property for this query observer.
    /// If this spec verify step is the requesting step then reply with the current configured return property of the query.
    /// </summary>
    public void Receive(StepObserver.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;
        if (!Verify.Is(message.Step)) return;
        message.Reply(Query.Model.Returns);
    }

    public static implicit operator SpecObserver(Spec model) => new(model);
    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}