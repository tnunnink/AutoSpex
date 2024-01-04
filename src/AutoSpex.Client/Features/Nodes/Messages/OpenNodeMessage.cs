using System;

namespace AutoSpex.Client.Features.Nodes;

/// <summary>
/// A generic message sent to the details view model to open a new tab and load the details or interface for a specific
/// project items. This could be a specification, source fil, or anything else.
/// </summary>
public class OpenNodeMessage
{
    /// <summary>
    /// Creates a new instance of the <see cref="OpenNodeMessage"/> class.
    /// </summary>
    /// <param name="node">The node instance to be opened.</param>
    public OpenNodeMessage(Node node)
    {
        Node = node;
    }

    /// <summary>
    /// The item instance to be opened by the details view model. This item should contain all relevant data required
    /// for the details view model to load it's content.
    /// </summary>
    public Node Node { get; }

    /// <summary>
    /// Whether the node should set focus to the node name text entry. This is for newly added nodes that we want
    /// to have the user rename.
    /// </summary>
    public bool InFocus { get; init; }

    /// <summary>
    /// Whether to show the node is a separate tab from the first default open tab. This creates a new tab if the node
    /// is not already open in an existing tab.
    /// </summary>
    public bool InTab { get; init; }

    /// <summary>
    /// The id of the node for which to replace the contents with the newly navigated node provided in the message. This
    /// means replace this tab with this node.
    /// </summary>
    public Guid ReplaceTab { get; init; }
}