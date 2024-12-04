using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>,
    IRecipient<Observer.Get<SpecObserver>>,
    IRecipient<PropertyInput.GetInputTo>,
    IRecipient<PropertyInput.GetDataTo>
{
    public SpecObserver(Spec model) : base(model)
    {
        Query = new QueryObserver(Model.Query, Source);
        Verify = new VerifyObserver(Model.Verify);

        Track(Query);
        Track(Verify);
    }

    public override Guid Id => Model.SpecId;
    public QueryObserver Query { get; }
    public VerifyObserver Verify { get; }

    /// <summary>
    /// The reference to the targeted source for the application. This is the source we need to find suggestions for
    /// nested criterion objects of this spec observer.
    /// </summary>
    private SourceObserver? Source =>
        GetObserver<SourceObserver>(s => s.Model is { IsTarget: true, Content: not null });

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
    /// Handles the request message by replying if this spec contains the provided instance.
    /// All verification criterion will take the return type of the configured query.
    /// </summary>
    public void Receive(PropertyInput.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;
        if (message.Observer is not CriterionObserver observer) return;
        if (!Verify.Criteria.Has(observer)) return;
        message.Reply(Query.Model.Returns);
    }

    /// <summary>
    /// Responds to the request for data to the finaly verification step of this spec.
    /// This is sent by a requesting <see cref="PropertyInput"/> observer.
    /// This handler only responds for criterion it contains, and simply uses the current Query config to return.
    /// </summary>
    public void Receive(PropertyInput.GetDataTo message)
    {
        if (message.Observer is not CriterionObserver criterion) return;
        if (!Verify.Criteria.Has(criterion)) return;

        if (Source?.Model.Content is null)
        {
            message.Reply([]);
            return;
        }

        try
        {
            var data = Query.Model.Execute(Source.Model.Content);
            message.Reply(data);
        }
        catch (Exception)
        {
            message.Reply([]);
        }
    }

    public static implicit operator SpecObserver(Spec model) => new(model);

    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}