namespace AutoSpex.Engine;

public class NoneOperation() : Operation("None")
{
    /// <inheritdoc />
    public override bool Execute(object? input, object? value) => false;

    /// <inheritdoc />
    protected override bool Supports(TypeGroup group) => false;

    /// <inheritdoc />
    public override string ToString() => string.Empty;
}