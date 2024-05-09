using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class NodeVariablesPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"Node/{Node.Id}/{Title}";
    public override string Title => "Variables";
    public NodeObserver Node { get; } = node;
    public ObserverCollection<Variable, VariableObserver> Variables { get; } = [];
}