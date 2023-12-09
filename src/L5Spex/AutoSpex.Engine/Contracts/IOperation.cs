namespace AutoSpex.Engine.Contracts;

public interface IOperation
{
    /// <summary>
    /// Performs the operation on the input and provided values and returns the result.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="values"></param>
    /// <returns>The result of the predicate operation.</returns>
    bool Evaluate(object? input, params object[] values);
}