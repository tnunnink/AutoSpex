using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class ContainerDetailPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"{node.Type}/{node.Id}/Config";

    [ObservableProperty] private SpecObserver? _condition;

    /// <inheritdoc />
    public override async Task Load()
    {
        if (node.IsVirtual)
        {
            Condition = node.Model.Spec;
        }
        else
        {
            var result = await Mediator.Send(new LoadSpec(node.Id));
            if (Notifier.ShowIfFailed(result)) return;
            Condition = new SpecObserver(result.Value);
        }

        Track(Condition);
    }

    /// <inheritdoc />
    public override async Task<Result> Save(Result? result = default)
    {
        if (Condition is null)
            return Result.Fail($"Condition configuration was not correctly loaded for {node.Name}.");

        return await Mediator.Send(new SaveSpec(Condition.Model));
    }
}