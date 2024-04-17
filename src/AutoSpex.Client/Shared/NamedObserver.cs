using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Shared;

public abstract partial class NamedObserver<TModel>(TModel model) : Observer<TModel>(model),
    IRecipient<NamedObserver<TModel>.Renamed>
{
    /// <summary>
    /// The name property identifying this observable object.
    /// </summary>
    public abstract string Name { get; set; }

    /// <summary>
    /// A command to perform a rename of the <see cref="NamedObserver{TModel}"/> object. This command should write the
    /// current <see cref="Name"/> value to the database and update the underlying model. This is done using the command
    /// and corresponding <see cref="Renamed"/> message.
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async function to perform.</returns>
    /// <remarks>
    /// This call forwards the call to <see cref="RenameModel"/> which deriving classes implement send the request
    /// to update the record in the database. If the result is successful then we clear changes for <see cref="Name"/>
    /// and we send the <see cref="Renamed"/> message which other instance of this object can receive to react and update
    /// their name as well.
    /// </remarks>
    [RelayCommand]
    private async Task Rename(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 100) return;

        Name = name;
        var result = await RenameModel(name);
        if (result.IsFailed) return;

        AcceptChanges(nameof(Name));
        Messenger.Send(new Renamed(this));
    }

    /// <summary>
    /// Handles the observer renamed message by updating this object instance's name if it is not the sender of the
    /// message. If this object is updated then it in turn sends a renamed message.
    /// </summary>
    /// <param name="message"></param>
    public virtual void Receive(Renamed message)
    {
        if (message.Observer is not { } observer) return;
        if (ReferenceEquals(this, observer)) return;
        if (Id != observer.Id) return;

        if (Name != observer.Name)
        {
            Name = observer.Name;
            return;
        }
        
        OnPropertyChanged(nameof(Name));
    }

    /// <summary>
    /// Sends the request to the database to update this object's name.
    /// </summary>
    protected abstract Task<Result> RenameModel(string name);

    /// <summary>
    /// The message sent when this object's name is successfully renamed.
    /// </summary>
    /// <param name="Observer">The sender of the message.</param>
    public record Renamed(NamedObserver<TModel> Observer);
}