using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A step in the process of running a specification. This abstraction deifines the primary things we need each step
/// to have. Each step will process some data and return a collection of resulting data. 
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(Filter), nameof(Filter))]
[JsonDerivedType(typeof(Select), nameof(Select))]
[JsonDerivedType(typeof(Verify), nameof(Verify))]
public abstract class Step
{
    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// Each step may have a collection of criteria configured for which it needs to process data.
    /// </summary>
    [JsonInclude]
    public List<Criterion> Criteria { get; private init; } = [];

    /// <summary>
    /// Processes the current step by taking an input collection of objects and returning an output collection of the same
    /// or different type, depending on the purpose or implementation of the step.
    /// </summary>
    /// <param name="input">A collection of objects to process.</param>
    /// <returns>A collection of objects that represent the result of the processing for this step.</returns>
    public abstract IEnumerable<object?> Process(IEnumerable<object?> input);

    /// <summary>
    /// Adds a new <see cref="Criterion"/> instance to the <see cref="Criteria"/> for this step. 
    /// </summary>
    public Criterion Add()
    {
        var criterion = new Criterion();
        Criteria.Add(criterion);
        return criterion;
    }
}