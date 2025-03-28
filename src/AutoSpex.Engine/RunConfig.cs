namespace AutoSpex.Engine;

public class RunConfig()
{
    public RunConfig(NodeInfo node) : this()
    {
        Node = node;
    }
    
    public Guid ConfigId { get; private init; } = Guid.NewGuid();
    public string Name { get; set; } = "Default Config";
    public NodeInfo Node { get; set; } = NodeInfo.Empty;
    public List<Source> Sources { get; } = [];
}