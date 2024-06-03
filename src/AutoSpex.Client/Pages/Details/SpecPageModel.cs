using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using AutoSpex.Persistence;

namespace AutoSpex.Client.Pages;

public class SpecPageModel(NodeObserver node) : NodePageModel(node)
{
    /// <inheritdoc />
    protected override async Task Run()
    {
        var specs = await LoadSpecs(CancellationToken.None);

        var run = new Run();
        run.AddSpecs(specs);

        await Navigator.Navigate(new RunObserver(run));
    }

    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SpecCriteriaPageModel(Node));
        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));
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