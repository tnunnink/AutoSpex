﻿namespace AutoSpex.Engine;

public class NoneOperation() : Operation("None", string.Empty)
{
    /// <inheritdoc />
    public override bool Execute(object? input, params object[] values) => false;

    /// <inheritdoc />
    protected override bool Supports(TypeGroup group) => false;
}