﻿namespace AutoSpex.Engine.Operations;

public class EndsWithOperation : BinaryOperation
{
    public EndsWithOperation() : base("EndsWith")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (string.IsNullOrEmpty(value.ToString()))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        
        return input is not null && input.ToString()?.EndsWith(value.ToString()!) == true;
    }
}