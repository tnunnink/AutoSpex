using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcePageModel : DetailPageModel,
    IRecipient<SourceObserver.Deleted>,
    IRecipient<SourceObserver.Renamed>
{
    private readonly Guid _sourceId;

    /// <inheritdoc/>
    public SourcePageModel(Guid sourceId)
    {
        _sourceId = sourceId;
    }

    public override string Route => $"{GetType().Name}/{_sourceId}";
    public override string Title => Source?.Name ?? "Not Found";
    public override string Icon => "Source";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))]
    private SourceObserver? _source;

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetSource(_sourceId));
        if (result.IsFailed) return;

        Source = new SourceObserver(result.Value);

        await Navigator.Navigate(() => new SourceInfoPageModel(Source));
        await Navigator.Navigate(() => new QueryPageModel(Source));
        await Navigator.Navigate(() => new SourceContentPageModel(Source));
    }

    public override void Receive(NavigationRequest message)
    {
        if (!message.Page.Route.Contains(_sourceId.ToString())) return;
        NavigateTabPage(message.Page, message.Action);
    }

    public void Receive(Observer<Source>.Deleted message)
    {
        if (message.Observer.Id != _sourceId) return;
        ForceClose();
    }

    public void Receive(Observer<Source>.Renamed message)
    {
        if (message.Observer is not SourceObserver source) return;
        if (source.Id != _sourceId) return;
        OnPropertyChanged(nameof(Title));
    }
}