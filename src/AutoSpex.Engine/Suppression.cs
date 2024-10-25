namespace AutoSpex.Engine;

/// <summary>
/// A supporession is a construct that assocciates a source with a specific node (spec) to automaticall suppress the
/// result of after running since it does not apply to the given source file, for the reason defined by the user.
/// </summary>
public class Suppression(Guid nodeId, string reason)
{
    /// <summary>
    /// Represents the unique identifier of the Node associated with a suppression.
    /// </summary>
    public Guid NodeId { get; } = nodeId;

    /// <summary>
    /// Represents the reason associated with a suppression.
    /// </summary>
    public string Reason { get; set; } = reason;
}