using L5Spex.Client.Observers;

namespace L5Spex.Client.Messages;

public class NodeCreatedMessage
{
    public NodeCreatedMessage(NodeObserver node)
    {
        Node = node;
    }

    public NodeObserver Node { get; }
}