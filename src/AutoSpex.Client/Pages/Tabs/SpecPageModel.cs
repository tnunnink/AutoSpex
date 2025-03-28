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

    [ObservableProperty] private ResultPageModel? _resultDrawer;

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
    }

    /// <inheritdoc />
    public override async Task<Result> Save(Result? result = default)
    {
        if (Spec is null)
            return Result.Fail($"Spec configuration was not correctly loaded for {node.Name}.");

        return await Mediator.Send(new SaveSpec(Spec.Model));
    }
}