﻿namespace AutoSpex.Engine;

public class EmptyOperation() : UnaryOperation("Empty")
{
    protected override bool Evaluate(object? input)
    {
        return input is not null && input switch
        {
            string s => string.Equals(s, string.Empty),
            _ => false
        };
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}