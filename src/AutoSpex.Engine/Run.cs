namespace AutoSpex.Engine;

public class Run
{
    private readonly Dictionary<Guid, Outcome> _outcomes = [];

    public Run()
    {
    }

    public Run(Source? source)
    {
        SourceId = source?.SourceId ?? Guid.Empty;
        Source = source;
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Run";
    public Guid SourceId { get; private set; } = Guid.Empty;
    public Source? Source { get; private set; }
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime RanOn { get; private set; } = DateTime.Now;
    public string RanBy { get; private set; } = Environment.UserName;
    public IEnumerable<Outcome> Outcomes => _outcomes.Values;

    /// <summary>
    /// Adds the provided <see cref="Outcome"/> to the run.
    /// </summary>
    /// <param name="outcome">The <see cref="Outcome"/> to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddOutcome(Outcome outcome)
    {
        if (outcome is null) throw new ArgumentNullException(nameof(outcome));

        _outcomes[outcome.SpecId] = outcome;
    }

    /// <summary>
    /// Adds the provided <see cref="Outcome"/> collection to the run.
    /// </summary>
    /// <param name="outcomes">The collection of <see cref="Outcome"/> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided collection is null.</exception>
    public void AddOutcomes(IEnumerable<Outcome> outcomes)
    {
        if (outcomes is null) throw new ArgumentNullException(nameof(outcomes));

        foreach (var outcome in outcomes)
            _outcomes[outcome.SpecId] = outcome;
    }

    /// <summary>
    /// Adds a <see cref="Spec"/> to the run.
    /// </summary>
    /// <param name="spec">The <see cref="Spec"/> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="spec"/> is null.</exception>
    public void AddSpec(Spec spec)
    {
        if (spec is null) throw new ArgumentNullException(nameof(spec));

        _outcomes[spec.SpecId] = new Outcome(spec);
    }

    /// <summary>
    /// Adds the provided <see cref="Spec"/> objects to the <see cref="Run"/> instance.
    /// </summary>
    /// <param name="specs">The collection of <see cref="Spec"/> objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="specs"/> parameter is null.</exception>
    public void AddSpecs(IEnumerable<Spec> specs)
    {
        if (specs is null) throw new ArgumentNullException(nameof(specs));

        foreach (var spec in specs)
            _outcomes[spec.SpecId] = new Outcome(spec);
    }

    /// <summary>
    /// Clears all outcomes from the run.
    /// </summary>
    public void Clear() => _outcomes.Clear();

    /// <summary>
    /// Removes the <see cref="Outcome"/> with the specified <paramref name="specId"/> from the run.
    /// </summary>
    /// <param name="specId">The identifier of the <see cref="Outcome"/> to remove.</param>
    public void Remove(Guid specId)
    {
        _outcomes.Remove(specId);
    }

    /// <summary>
    /// Updates the result of the run based on the outcomes.
    /// </summary>
    public void UpdateResult()
    {
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
        Result = _outcomes.Max(r => r.Value.Result);
    }

    /// <summary>
    /// Updates the source of the run with the provided <see cref="Source"/> object.
    /// </summary>
    /// <param name="source">The <see cref="Source"/> object to update the run source with.</param>
    public void UpdateSource(Source? source)
    {
        SourceId = source?.SourceId ?? Guid.Empty;
        Source = source;
    }
}