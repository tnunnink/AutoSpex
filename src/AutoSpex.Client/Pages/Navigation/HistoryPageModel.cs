using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class HistoryPageModel : PageViewModel, IRecipient<RunObserver.Created>
{
    public override string Title => "History";
    public override string Icon => "History";
    public ObservableCollection<RunGroup> Runs { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListRuns());
        if (result.IsFailed) return;
        
        Runs.Clear();
        
        foreach (var group in result.Value.GroupBy(r => r.RanOn.Date))
        {
            var runs = group.Select(r => new RunObserver(r));
            var runGroup = new RunGroup(group.Key, new ObservableCollection<RunObserver>(runs));
            Runs.Add(runGroup);
        }
    }

    public void Receive(Observer<Run>.Created message)
    {
        if (message.Observer is not RunObserver run) return;
        
        var group = Runs.FirstOrDefault(g => g.RunDate.Date == run.Model.RanOn);
        if (group is not null)
        {
            group.Runs.Add(run);
            return;
        }
        
        Runs.Add(new RunGroup(run.Model.RanOn, [run]));
    }
}

public record RunGroup(DateTime RunDate, ObservableCollection<RunObserver> Runs);