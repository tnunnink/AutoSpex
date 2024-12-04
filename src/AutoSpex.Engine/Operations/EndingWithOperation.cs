namespace AutoSpex.Engine;

public class EndingWithOperation() : BinaryOperation("Ending With")
{
    protected override bool Evaluate(object? input, object value)
    {
        var text = input?.ToString();
        var argument = value.ToString();

        if (argument is null)
            throw new ArgumentException($"Argument value is required for {Name} operation.");


        return text is not null && text.EndsWith(argument);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}