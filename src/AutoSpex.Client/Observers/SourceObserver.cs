using System.IO;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver : Observer<Source>, IRecipient<Observer.Get<SourceObserver>>
{
    private readonly RepoObserver? _repo;

    /// <inheritdoc/>
    public SourceObserver(Source source, RepoObserver? repo = null) : base(source)
    {
        _repo = repo;
    }

    public override string Icon => Model.Type.Name;
    public override string Name => Model.Name;
    public string Location => Model.Location;
    public string RelativePath => GetRelativeOrDefaultPath();
    public string UpdatedOn => $"{Model.UpdatedOn:G}";
    public string Size => $"{Model.Size / 1024} KB";
    public bool IsTargeted => _repo?.Model.IsTargeted(Location) is true;

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter) || RelativePath.Satisfies(filter);
    }

    /// <summary>
    /// Command to toggle the target state of this source for its current parent repo. 
    /// </summary>
    [RelayCommand]
    private async Task ToggleTarget()
    {
        if (_repo is null) return;

        _repo.Model.ToggleTarget(Model);

        var result = await Mediator.Send(new UpdateTargets(_repo));
        if (Notifier.ShowIfFailed(result)) return;

        Refresh();
        _repo?.Refresh();
    }

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

    /// <summary>
    /// Gets the current connected repo path. This can be used to show the path of a source relative to the repo. 
    /// </summary>
    private string GetRelativeOrDefaultPath()
    {
        return _repo is not null
            ? Location.Replace(_repo.Location, @"~\").Replace(@"\\", @"\")
            : Model.Location;
    }

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source model) => new(model);
}