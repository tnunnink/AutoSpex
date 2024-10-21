using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class CriteriaPageModel : PageViewModel, IRecipient<ArgumentObserver.SuggestionRequest>
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
    /// Query the database for variables that are in scope of this argument. These are variables that belong to or are
    /// inherited by the node object.
    /// </summary>
    public async void Receive(ArgumentObserver.SuggestionRequest message)
    {
        //Only reply to arguments this node/spec contains.
        if (!ContainsArgument(message.Argument)) return;

        //Remove the prefix '@' so we can return all variables we want to reference.
        var filter = message.Filter?.TrimStart(Reference.Prefix);

        var variables = await Mediator.Send(new GetScopedVariables(_node.Id));
        var values = variables.Select(v => new ValueObserver(v)).Where(v => v.Filter(filter)).ToList();
        values.ForEach(message.Reply);
    }

    /// <summary>
    /// Determines whenther the local node has a spec that contains the provided argument id.
    /// </summary>
    private bool ContainsArgument(ArgumentObserver argument)
    {
        return _node.Model.Spec.Filters.Any(f => f.Contains(argument.Id)) ||
               _node.Model.Spec.Verifications.Any(v => v.Contains(argument.Id));
    }
}