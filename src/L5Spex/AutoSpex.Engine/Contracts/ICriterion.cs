namespace AutoSpex.Engine.Contracts;

/// <summary>
/// A criterion is a rule that is used to evaluate a candidate object and determine if that object satisfies the rule
/// or condition. This is essentially the same as a specification pattern, but we are using the term criterion
/// since a specification will involve a set of criteria for which to evaluate a candidate object
/// </summary>
public interface ICriterion
{
    Evaluation Evaluate(object? candidate);
    
    Func<object, bool> Compile();

    Func<T, bool> Compile<T>();
}