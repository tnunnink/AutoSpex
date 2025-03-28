using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class SourceObserver : Observer<Source>, IRecipient<Observer.Get<SourceObserver>>
{
    /// <inheritdoc/>
    public SourceObserver(Source source) : base(source)
    {
    }

    public override string Icon => nameof(Source);
    public override string Name => Model.Name;
    public string Location => Model.Location;
    public string UpdatedOn => Model.UpdatedOn.ToShortDateString();


    #region Messages

    /// <summary>
    /// Handle the get message by replying with this source instance if it satisfies the provided predicate.
    /// </summary>
    public void Receive(Get<SourceObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate(this))
        {
            message.Reply(this);
        }
    }

    #endregion


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Name.Satisfies(filter) || Description.Satisfies(filter);
    }

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source model) => new(model);
}