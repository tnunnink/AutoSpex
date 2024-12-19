using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class SpecPageModel(NodeObserver node) : PageViewModel("Spec")
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public override string Icon => "IconLineClipboard";
    
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

        return await Mediator.Send(new SaveSpec(Spec));
    }

    /// <summary>
    /// Uses the current test runner page to execeute this specification.
    /// Opens the runner drawer if not alread open.
    /// </summary>
    [RelayCommand]
    private async Task Test()
    {
        if (Spec is null || ResultDrawer is null) return;

        var result = await Mediator.Send(new NewRun(node.Id));
        if (Notifier.ShowIfFailed(result)) return;

        var run = new RunObserver(result.Value);
        var copy = node.Model.Copy();
        copy.Configure(Spec);
        await run.Execute(copy);

        ResultDrawer.Outcome = run.Outcomes.Count == 1 ? run.Outcomes[0] : null;
        ShowDrawer = true;
    }
}