using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

public class NodeVariablesPageModel(NodeObserver node) : DetailPageModel
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public override string Title => "Variables";
    public ObserverCollection<Variable, VariableObserver> Variables { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetNodeVariables(node.Id));
        if (result.IsFailed) return;
        Variables.Refresh(result.Value.Select(v => new VariableObserver(v)));
        Track(Variables);
    }
}