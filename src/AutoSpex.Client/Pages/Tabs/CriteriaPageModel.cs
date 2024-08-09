using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class CriteriaPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"{nameof(Spec)}/{node.Id}/{Title}";
    public override string Title => "Criteria";
    public SpecObserver Spec { get; private set; } = new(new Spec(node));

    public override async Task Load()
    {
        //If this node has not been persisted, just track the spec and return since nothing is in the database (yet).
        if (node.IsVirtual)
        {
            Track(Spec);
            return;
        }

        //Otherwise load the spec for this node from the database.
        var result = await Mediator.Send(new GetSpec(node.Id));
        if (result.IsFailed) return;

        Spec = new SpecObserver(result.Value);
        Track(Spec);
        OnPropertyChanged(nameof(Spec));
    }

    public override Task<Result> Save()
    {
        return Mediator.Send(new SaveSpec(Spec));
    }
}