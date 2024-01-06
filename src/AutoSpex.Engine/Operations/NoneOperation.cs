namespace AutoSpex.Engine;

public class NoneOperation() : Operation("None")
{
    public override bool Execute(object? input, params object[] values) => false;

    public override int NumberOfArguments => -1;
    protected override bool Supports(TypeGroup group) => false;
}