using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public class RunsPageModel(string project) : PageViewModel, IRecipient<RunObserver.Created>
{
    public override string Route => $"{project}/Runs";
    public override string Title => "Runs";
    public override string Icon => "Runs";
    public ObservableCollection<RunObserver> Runs { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListRuns());
        if (result.IsFailed) return;

        Runs.Clear();

        foreach (var run in result.Value)
            Runs.Add(run);
    }

    public void Receive(Observer<Run>.Created message)
    {
        if (message.Observer is not RunObserver run) return;
        Runs.Add(run);
    }
}