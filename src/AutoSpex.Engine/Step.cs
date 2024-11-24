using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A step in the process of running a specification. This abstraction deifines the primary things we need each step
/// to have. Each step will process some data and return a collection of resulting data. Each step will require knowledge
/// of the input type or the type that we expect the input data to be. This will help determine how steps are configured. 
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(Query), nameof(Query))]
[JsonDerivedType(typeof(Filter), nameof(Filter))]
[JsonDerivedType(typeof(Select), nameof(Select))]
[JsonDerivedType(typeof(Verify), nameof(Verify))]
public abstract class Step()
{
    private readonly List<Criterion> _criteria = [];

    protected Step(IEnumerable<Criterion> criteria) : this()
    {
        _criteria = criteria.ToList();
    }

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the step.
    /// Each step may have a collection of criteria configured for which it needs to process data.
    /// </summary>
    [JsonInclude]
    public IEnumerable<Criterion> Criteria => _criteria;

    /// <summary>
    /// Processes the current step by taking an input collection of objects and returning an output collection of the same
    /// or different type, depending on the purpose or implementation of the step.
    /// </summary>
    /// <param name="input">A collection of objects to process.</param>
    /// <returns>A collection of objects that represent the result of the processing for this step.</returns>
    public abstract IEnumerable<object> Process(IEnumerable<object> input);

    /// <summary>
    /// Adds a new <see cref="Criterion"/> instance to the <see cref="Criteria"/> for this step. 
    /// </summary>
    public Criterion Add()
    {
        var criterion = new Criterion();
        _criteria.Add(criterion);
        return criterion;
    }

    /// <summary>
    /// Adds the provided <see cref="Criterion"/> to the list of criteria to be used for by this step.
    /// </summary>
    /// <param name="criterion">The criterion to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="criterion"/> is <c>null</c>.</exception>
    public void Add(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        _criteria.Add(criterion);
    }

    /// <summary>
    /// Removes a provided <see cref="Criterion"/> from the list of criteria to be used for filtering.
    /// </summary>
    /// <param name="criterion">The criterion to be removed.</param>
    public void Remove(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        _criteria.Remove(criterion);
    }

    /// <summary>
    /// Moves a criterion in the list of criteria to a new position specified by the indices.
    /// </summary>
    /// <param name="oldIndex">The current index of the criterion to be moved.</param>
    /// <param name="newIndex">The new index where the criterion should be placed after moving.</param>
    public void Move(int oldIndex, int newIndex)
    {
        var item = _criteria[oldIndex];
        _criteria.RemoveAt(oldIndex);
        _criteria.Insert(newIndex, item);
    }

    /// <summary>
    /// Clears the list of criteria for this step.
    /// </summary>
    public void Clear()
    {
        _criteria.Clear();
    }

    /// <summary>
    /// Determines whether the current Step contains a specific Criterion.
    /// </summary>
    /// <param name="criterion">The Criterion to check for in the Step.</param>
    /// <returns>True if the Step contains the specified Criterion; otherwise, false.</returns>
    public bool Contains(Criterion criterion)
    {
        return _criteria.Any(c => c.Contains(criterion));
    }
}