using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceManagerPageModel() : DetailPageModel("Sources"), IRecipient<Observer.GetSelected>
{
    public override string Route => "Sources";
    public override string Icon => "Source";

    public ObserverCollection<Source, SourceObserver> Sources { get; } = [];

    public ObservableCollection<SourceObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    public override async Task Load()
    {
        var sources = await Mediator.Send(new ListSources());

        Sources.Bind(sources.ToList(), s => new SourceObserver(s));
    }

    [RelayCommand]
    private async Task AddSource()
    {
        var source = await Prompter.Show<SourceObserver?>(() => new NewSourcePageModel());
        if (source is null) return;

        var result = await Mediator.Send(new CreateSource(source));
        if (Notifier.ShowIfFailed(result)) return;

        Sources.Add(source);
        Messenger.Send(new Observer.Created<SourceObserver>(source));
        await Navigator.Navigate(source);
    }

    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not SourceObserver observer) return;
        if (!Sources.Any(s => s.Is(observer))) return;

        foreach (var source in Selected)
        {
            message.Reply(source);
        }
    }

    public override void Receive(Observer.Deleted message)
    {
        if (message.Observer is not SourceObserver observer) return;
        Sources.Remove(observer);

        base.Receive(message);
    }

    partial void OnFilterChanged(string? value)
    {
        Sources.Filter(value);
    }
}