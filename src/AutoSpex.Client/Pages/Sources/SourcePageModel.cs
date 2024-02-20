using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcePageModel : DetailPageModel
{
    /// <inheritdoc/>
    public SourcePageModel(SourceObserver source)
    {
        Source = source;
    }
    
    [ObservableProperty] private SourceObserver _source;
}