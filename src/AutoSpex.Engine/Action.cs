namespace AutoSpex.Engine;

public class Action(Guid nodeId, ActionType type, string reason, Spec? config = default)
{
    /// <summary>
    /// The Id of the node that this action is defined for.
    /// </summary>
    public Guid NodeId { get; } = nodeId;

    /// <summary>
    /// The rule action that this rule will perform when running againt the specified spec.
    /// </summary>
    public ActionType Type { get; init; } = type;

    /// <summary>
    /// Represents the reason associated with a suppression.
    /// </summary>
    public string Reason { get; set; } = reason;

    /// <summary>
    /// The optional configuration used to override a spec when this rule represents an override type.
    /// </summary>
    public Spec? Config { get; init; } = config;

    /// <summary>
    /// Creates a new Rule with the type set to Suppress.
    /// </summary>
    /// <param name="nodeId">The unique identifier of the node.</param>
    /// <param name="reason">The reason for suppressing the rule.</param>
    /// <returns>A new Rule instance with RuleType set to Suppress.</returns>
    public static Action Suppress(Guid nodeId, string reason)
    {
        return new Action(nodeId, ActionType.Suppress, reason);
    }

    /// <summary>
    /// Creates a new Rule with the type set to Override.
    /// </summary>
    /// <param name="node">The Node instance for which the rule is being overridden.</param>
    /// <param name="reason">The reason for overriding the rule.</param>
    /// <returns>A new Rule instance with RuleType set to Override.</returns>
    public static Action Override(Node node, string reason)
    {
        return new Action(node.NodeId, ActionType.Override, reason, node.Spec);
    }
}