using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class CriteriaPageModel(NodeObserver node) : PageViewModel("Criteria")
{
    public override string Route => $"Spec/Criteria/{node.Id}";

    /// <summary>
    /// The specification config for the current node.
    /// </summary>
    [ObservableProperty] private SpecObserver? _spec;

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

        return await Mediator.Send(new SaveSpec(Spec));
    }
}