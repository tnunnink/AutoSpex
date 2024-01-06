namespace AutoSpex.Engine;

public class Variable
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public object Default { get; set; }
    public object? Current { get; set; }
}