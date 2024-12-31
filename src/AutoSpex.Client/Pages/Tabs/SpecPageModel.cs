using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class SpecPageModel(NodeObserver node) : PageViewModel("Spec")
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public override string Icon => "IconFilledClipboard";

    [ObservableProperty] private SpecObserver? _spec;

    [ObservableProperty] private bool _showDrawer;

    [ObservableProperty] private ResultDrawerPageModel? _resultDrawer;

    /// <inheritdoc />
    public override async Task Load()
    {
        if (node.IsVirtual)
        {
            Spec = node.Model.Spec;
        }
        else
        {
            var result = await Mediator.Send(new LoadSpec(node.Id));
            if (Notifier.ShowIfFailed(result)) return;
            Spec = new SpecObserver(result.Value);
        }

        Track(Spec);

        ResultDrawer = new ResultDrawerPageModel();
        RegisterDisposable(ResultDrawer);
    }

    /// <inheritdoc />
    public override async Task<Result> Save(Result? result = default)
    {
        if (Spec is null)
            return Result.Fail($"Spec configuration was not correctly loaded for {node.Name}.");

        var payload = node.Model.Copy();
        payload.Configure(Spec.Model);
        return await Mediator.Send(new SaveSpec(payload));
    }

    /// <summary>
    /// Uses the current test runner page to execeute this specification.
    /// Opens the runner drawer if not alread open.
    /// </summary>
    public async Task RunSpec()
    {
        if (Spec is null || ResultDrawer is null) return;

        var result = await Mediator.Send(new NewRun(node.Id));
        if (Notifier.ShowIfFailed(result)) return;

        var run = new RunObserver(result.Value);
        run.Node.Model.Configure(Spec);
        await run.ExecuteLocal();

        ResultDrawer.Outcome = run.Outcomes.Count == 1 ? run.Outcomes[0] : null;
        ShowDrawer = true;
    }
}