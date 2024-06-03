using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SpecCriteriaPageModel(NodeObserver node) : DetailPageModel
{
    public override string Route => $"Spec/{node.Id}/{Title}";
    public override string Title => "Criteria";

    [ObservableProperty] private SpecObserver _spec = SpecObserver.Empty;

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetSpec(node.Id));
        if (result.IsFailed) return;
        Spec = new SpecObserver(result.Value);
        Track(Spec);
    }

    public override Task<Result> Save()
    {
        return Mediator.Send(new SaveSpec(Spec));
    }
}