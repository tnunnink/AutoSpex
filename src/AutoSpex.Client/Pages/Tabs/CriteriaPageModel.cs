using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class CriteriaPageModel : PageViewModel
{
    private readonly NodeObserver _node;

    /// <inheritdoc/>
    public CriteriaPageModel(NodeObserver node) : base("Criteria")
    {
        _node = node;
        Spec = new SpecObserver(node.Model.Spec);
        Track(Spec);
    }

    /// <inheritdoc />
    public override string Route => $"Spec/{_node.Id}/{Title}";

    /// <summary>
    /// The specification config for the current node.
    /// </summary>
    public SpecObserver Spec { get; }
}