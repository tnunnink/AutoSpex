using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SpecObserver : Observer<Spec>,
    IRecipient<Observer.Get<SpecObserver>>,
    IRecipient<StepObserver.GetInputTo>,
    IRecipient<PropertyInput.GetDataTo>,
    IRecipient<ArgumentInput.SuggestionRequest>
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
    /// Responds to the request for determining the step input proeprty for this query observer.
    /// If this spec verify step is the requesting step then reply with the current configured return property of the query.
    /// </summary>
    public void Receive(StepObserver.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;
        if (!Verify.Is(message.Step)) return;
        message.Reply(Query.Model.Returns);
    }

    /// <summary>
    /// Responds to the request for data to the finaly verification step of this spec.
    /// This is sent by a requesting <see cref="PropertyInput"/> observer.
    /// This handler only responds for criterion it contains, and simply uses the current Query config to return.
    /// </summary>
    public void Receive(PropertyInput.GetDataTo message)
    {
        if (Source?.Model.Content is null) return;
        if (message.Observer is not CriterionObserver criterion) return;
        if (!Verify.Criteria.Has(criterion)) return;

        try
        {
            var data = Query.Model.Execute(Source.Model.Content).ToList();
            data.ForEach(message.Reply);
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content and current property type.
            // If getting object value fails then it could be because the user configured the criterion incorrectly.
        }
    }

    /// <summary>
    /// Responds to an argument suggestion request for argument contained in verify criteria in this spec instance.
    /// Since we may have an in-memory source context, we can use that to evaluate what are possible values that could
    /// be input to the criterion.
    /// </summary>
    public void Receive(ArgumentInput.SuggestionRequest message)
    {
        if (Source?.Model.Content is null) return;

        var criterion = Verify.Criteria.SingleOrDefault(c => c.Contains(message.Argument));
        if (criterion is null) return;

        try
        {
            var data = Query.Model.Execute(Source.Model.Content);
            var values = criterion.ValuesFor(message.Argument, data);
            var suggestions = values.Select(v => new ValueObserver(v)).Where(x => !x.IsEmpty).ToList();
            suggestions.ForEach(message.Reply);
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content and current property type.
            // If getting object value fails then it could be because the user configured the criterion incorrectly.
        }
    }

    public static implicit operator SpecObserver(Spec model) => new(model);

    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}