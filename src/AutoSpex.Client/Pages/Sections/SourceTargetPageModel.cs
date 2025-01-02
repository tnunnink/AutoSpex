using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceTargetPageModel : PageViewModel,
    IRecipient<SourceObserver.Targeted>,
    IRecipient<Observer.Deleted>
{
    [ObservableProperty] private SourceObserver? _target;
    public Task<SourceSelectorPageModel> SourceList => Navigator.Navigate<SourceSelectorPageModel>();

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new LoadTargetSource());
        if (result.IsFailed) return;
        Target = result.Value;
        RegisterDisposable(Target);
    }

    /// <summary>
    /// When an source is removed from the application we to check if that is the targeted instance and set to
    /// null if so. This forces the user to select or create a new source.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not SourceObserver observer) return;
        if (Target is null) return;
        if (Target != observer) return;
        
        Target.Dispose();
        Target = null;
    }

    /// <summary>
    /// When the user targets a source from anywhere in the app, update the local
    /// <see cref="Target"/> reference. This is the reference that has the loaded content cached so we can quickly
    /// request suggestable values.
    /// </summary>
    public void Receive(SourceObserver.Targeted message)
    {
        Target?.Dispose();
        Target = message.Source;
        RegisterDisposable(Target);
    }
}