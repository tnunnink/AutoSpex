using System;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunPageModel(NodeObserver node, RunObserver? run = default) : NodePageModel(node)
{
    /// <summary>
    /// this is the Run that is either passed in preconfigured or loaded from the database. We want to hold it here
    /// and pass it into child tabs as needed. Either way this should get set prior to navigating tabs, or we throw error.
    /// We also don't need to save this before executing the run command since we can just pass in this instance.
    /// </summary>
    [ObservableProperty] private RunObserver? _runObserver = run;

    /// <inheritdoc />
    public override async Task Load()
    {
        //If no preconfigured run observer is passed in then we need to load it from the database.
        //Otherwise, this will be an in memory object unless the user wants to save what they have configured.
        if (RunObserver is null)
        {
            var result = await Mediator.Send(new GetRun(Node.Id));
            if (result.IsFailed) return;
            RunObserver = new RunObserver(result.Value);
        }

        Track(RunObserver);
        await base.Load();
    }

    /// <inheritdoc />
    protected override async Task NavigateTabs()
    {
        if (RunObserver is null)
            throw new ArgumentException("Can not load run page without a valid RunObserver.");

        await Navigator.Navigate(() => new RunResultsPageModel(RunObserver));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }

    /// <inheritdoc />
    protected override async Task Run()
    {
        if (RunObserver is null) return;
        await RunObserver.Open();
    }

    /// <inheritdoc />
    protected override bool CanRun() => RunObserver is not null;
}