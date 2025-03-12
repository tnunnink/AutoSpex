using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A step in the process of running a specification. This abstraction deifines the primary things we need each step
/// to have. Each step will process some data and return a collection of resulting data. 
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(Count), nameof(Count))]
[JsonDerivedType(typeof(Filter), nameof(Filter))]
[JsonDerivedType(typeof(Select), nameof(Select))]
[JsonDerivedType(typeof(Verify), nameof(Verify))]
public abstract class Step
{
    /// <summary>
    /// Processes the current step by taking an input collection of objects and returning an output collection of the same
    /// or different type, depending on the purpose or implementation of the step.
    /// </summary>
    /// <param name="input">A collection of objects to process.</param>
    /// <returns>A collection of objects that represent the result of the processing for this step.</returns>
    public abstract IEnumerable<object?> Process(IEnumerable<object?> input);

    /// <summary>
    /// Determines what the return <see cref="Property"/> will be given an input property. Most steps will return the
    /// same type that is input, but some may not. This step will force each step to define a method for determining
    /// the return type (i.e. input to the next step).
    /// </summary>
    /// <param name="input">The input type of this step.</param>
    /// <returns>The <see cref="Property"/> that represents a self-referential type of the output of this step.</returns>
    public abstract Property Returns(Property input);
}