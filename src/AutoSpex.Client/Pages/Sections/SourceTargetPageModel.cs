using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceTargetPageModel : PageViewModel, IRecipient<Observer.Deleted>
{
    [ObservableProperty] private SourceObserver? _target;
    public Task<SourceSelectorPageModel> SourceList => Navigator.Navigate<SourceSelectorPageModel>();

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
}