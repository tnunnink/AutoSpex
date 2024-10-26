using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
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

    /// <summary>
    /// Determines whenther the local node has a spec that contains the provided argument id.
    /// </summary>
    private bool ContainsArgument(ArgumentObserver argument)
    {
        return _node.Model.Spec.Filters.Any(f => f.Contains(argument.Id)) ||
               _node.Model.Spec.Verifications.Any(v => v.Contains(argument.Id));
    }
}