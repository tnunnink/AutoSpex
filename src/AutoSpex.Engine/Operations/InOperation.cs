namespace AutoSpex.Engine;

public class InOperation() : Operation("In")
{
    public override bool Execute(object? input, params object[] values)
    {
        return input is not null && values.Length != 0 && values.Contains(input);
    }
    
    public override int NumberOfArguments => -1;

    protected override bool Supports(TypeGroup group) => group != TypeGroup.Collection && group != TypeGroup.Boolean;
}