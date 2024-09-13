using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class Chain : SmartEnum<Chain, int>
{
    private Chain(string name, int value) : base(name, value)
    {
    }

    public static readonly Chain And = new(nameof(And), 0);

    public static readonly Chain Or = new(nameof(Or), 1);

    public bool Combine(Evaluation a, Evaluation b) => this == And ? a && b : a || b;
}