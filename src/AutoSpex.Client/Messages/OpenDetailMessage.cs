using System;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Features;

/// <summary>
/// A generic message sent to the details view model to open a new tab and load the details or interface for a specific
/// project items. This could be a specification, source fil, or anything else.
/// </summary>
public class OpenDetailMessage
{
    /// <summary>
    /// Creates a new instance of the <see cref="OpenDetailMessage"/> class.
    /// </summary>
    /// <param name="details"></param>
    public OpenDetailMessage(DetailViewModel details)
    {
        Details = details;
    }

    /// <summary>
    /// The item instance to be opened by the details view model. This item should contain all relevant data required
    /// for the details view model to load it's content.
    /// </summary>
    public DetailViewModel Details { get; }

    /// <summary>
    /// Whether the node should set focus to the node name text entry. This is for newly added nodes that we want
    /// to have the user rename.
    /// </summary>
    public bool NewItem { get; init; }

    /// <summary>
    /// Whether to show the node is a separate tab from the first default open tab. This creates a new tab if the node
    /// is not already open in an existing tab.
    /// </summary>
    public bool InNewTab { get; init; }

    /// <summary>
    /// The id of the node for which to replace the contents with the newly navigated node provided in the message. This
    /// means replace this tab with this node.
    /// </summary>
    public Guid ReplaceTab { get; init; }
}