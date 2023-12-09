namespace AutoSpex.Client.Features.Nodes;

/// <summary>
/// A generic message sent to the details view model to open a new tab and load the details or interface for a specific
/// project items. This could be a specification, source fil, or anything else.
/// </summary>
public class OpenNode
{
    /// <summary>
    /// Creates a new instance of the <see cref="OpenNode"/> class.
    /// </summary>
    /// <param name="node">The node instance to be opened.</param>
    public OpenNode(Node node)
    {
        Node = node;
    }

    /// <summary>
    /// The item instance to be opened by the details view model. This item should contain all relevant data required
    /// for the details view model to load it's content.
    /// </summary>
    public Node Node { get; }
}