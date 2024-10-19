using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceSelectorPageModel : PageViewModel,
    IRecipient<SourceObserver.Targeted>,
    IRecipient<Observer.Deleted>
{
    [ObservableProperty] private bool _showNavigationButton = true;

    [ObservableProperty] private SourceObserver? _targeted;
    public Task<SourceSelectorListPageModel> SourceList => Navigator.Navigate<SourceSelectorListPageModel>();

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new GetTargetSource());
        if (result.IsFailed) return;
        Targeted = result.Value;
        RegisterDisposable(Targeted);
    }

    /// <summary>
    /// When an source is removed from the application we to check if that is the targeted instance and set to
    /// null if so. This forces the user to select or create a new source.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not SourceObserver observer) return;
        if (Targeted is null) return;
        if (Targeted != observer) return;
        Targeted = null;
    }

    /// <summary>
    /// When the user targets a source from anywhere in the app, update the local
    /// <see cref="Targeted"/> reference.
    /// </summary>
    public void Receive(SourceObserver.Targeted message)
    {
        Targeted = message.Source;
    }
}