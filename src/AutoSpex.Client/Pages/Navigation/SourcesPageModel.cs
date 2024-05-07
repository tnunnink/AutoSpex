using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using SourceObserver = AutoSpex.Client.Observers.SourceObserver;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcesPageModel(string project) : PageViewModel, 
    IRecipient<SourceObserver.Created>,
    IRecipient<SourceObserver.Deleted>,
    IRecipient<SourceRequest>
{
    public override string Route => $"{project}/Sources";
    public override string Title => "Sources";
    public override string Icon => "Sources";

    [ObservableProperty] private ObservableCollection<SourceObserver> _sources = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListSources());

        if (result.IsSuccess)
        {
            Sources = new ObservableCollection<SourceObserver>(result.Value.Select(s => new SourceObserver(s)));
        }
    }

    [RelayCommand]
    private async Task AddSource()
    {
        var source = await Prompter.Show<SourceObserver?>(new AddSourcePageModel());
        if (source is null) return;
        await Navigator.Navigate(source);
    }

    public void Receive(SourceRequest message)
    {
        message.Reply(Sources);
    }

    public void Receive(Observer<Source>.Created message)
    {
        if (message.Observer is not SourceObserver source) return;
        Sources.Add(source);
    }

    public void Receive(Observer<Source>.Deleted message)
    {
        if (message.Observer is not SourceObserver source) return;
        Sources.Remove(source);
    }
}