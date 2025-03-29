namespace AutoSpex.Engine;

public class RunContext
{
    public RunContext(Node node)
    {
    }
    
    
    public IEnumerable<Node> Nodes { get; } = [];
    public IEnumerable<Source> Sources { get; } = [];
}