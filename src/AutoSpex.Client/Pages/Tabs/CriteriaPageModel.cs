using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class CriteriaPageModel(NodeObserver node) : PageViewModel("Criteria")
{
    public override string Route => $"Spec/{Node.Id}/{Title}";
    public NodeObserver Node { get; } = node;

    [RelayCommand]
    private void AddSpec()
    {
        Node.Specs.Add(new SpecObserver(new Spec()));
    }
}