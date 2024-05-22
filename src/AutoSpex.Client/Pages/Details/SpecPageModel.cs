using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SpecPageModel(NodeObserver node) : NodePageModel(node)
{
    [ObservableProperty] private SpecObserver _spec = SpecObserver.Empty;

    public override async Task Load()
    {
        await LoadSpec();
        await NavigateTabs();
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <inheritdoc />
    protected override async Task Run()
    {
        var specs = await LoadSpecs(CancellationToken.None);

        var run = new Run();
        run.AddSpecs(specs);

        await Navigator.Navigate(new RunObserver(run));
    }

    protected override Task Save()
    {
        return base.Save();
    }

    private async Task LoadSpec()
    {
        var result = await Mediator.Send(new GetSpec(Node.Id));
        if (result.IsFailed) return;
        Spec = new SpecObserver(result.Value);
        Track(Spec);
    }

    private async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SpecCriteriaPageModel(Spec));
        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));
        await Navigator.Navigate(() => new SpecSettingsPageModel(Spec));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }

    private async Task<IEnumerable<Spec>> LoadSpecs(CancellationToken token)
    {
        var load = await Mediator.Send(new LoadSpecs([Node.Id]), token);
        if (load.IsFailed) return Enumerable.Empty<Spec>();

        await Mediator.Send(new ResolveVariables(load.Value), token);
        return load.Value;
    }
}