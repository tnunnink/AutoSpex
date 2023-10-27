﻿namespace L5Spex.Engine.Operations;

public class InOperation : Operation
{
    public InOperation() : base("In")
    {
    }

    public override bool Evaluate(object? input, params object[] values)
    {
        return input is not null && values.Length != 0 && values.Contains(input);
    }
}