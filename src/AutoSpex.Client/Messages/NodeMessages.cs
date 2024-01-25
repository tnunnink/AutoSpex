using System;
using AutoSpex.Client.Observers;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;

namespace AutoSpex.Client.Messages;

/// <summary>
/// 
/// </summary>
/// <param name="node"></param>
public class NodeCreateRequest(NodeObserver node) : AsyncRequestMessage<Result>
{
    public NodeObserver Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
}

public class NodeDeleteRequest(NodeObserver node) : AsyncRequestMessage<Result>
{
    public NodeObserver Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
}

public class NodeRenameRequest(NodeObserver node) : AsyncRequestMessage<Result>
{
    public NodeObserver Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
}

public record NodeRenamedMessage(NodeObserver Node);



//todo should mode of these (other than creation) just pass NodeId an relevant data as record?