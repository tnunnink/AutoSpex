namespace AutoSpex.Engine;

public class NoneOperation() : Operation("None")
{
    public override int NumberOfArguments => -1;
    public override string ShouldMessage => string.Empty;
    public override bool Execute(object? input, params object[] values) => false;
    protected override bool Supports(TypeGroup group) => false;
}