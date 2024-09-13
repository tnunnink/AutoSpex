using AutoSpex.Engine;

namespace AutoSpex.Persistence;

public static class Extensions
{
    public static Dictionary<Guid, Node> BuildTree(this IEnumerable<Node> nodes)
    {
        var lookup = new Dictionary<Guid, Node>();
        
        foreach (var node in nodes)
        {
            lookup.TryAdd(node.NodeId, node);

            if (lookup.TryGetValue(node.ParentId, out var parent))
                parent.AddNode(node);
        }

        return lookup;
    }
}