using System.Diagnostics;
using JetBrains.Annotations;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given source L5X.
/// </summary>
[PublicAPI]
public class Outcome
{
    /// <summary>
    /// Creates an empty outcome.
    /// </summary>
    private Outcome()
    {
        Spec = default!;
    }

    public Outcome(Spec spec)
    {
        Spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    public Guid OutcomeId { get; private set; } = Guid.NewGuid();
    public Guid SpecId => Spec.SpecId;
    public Spec Spec { get; private set; }
    public ResultState Result { get; private set; } = ResultState.None;
    public long Duration { get; private set; }
    public List<Evaluation> Evaluations { get; } = [];

    /// <summary>
    /// Processes this outcome using the configured spec and provided source content.
    /// </summary>
    /// <param name="content">The L5X to run against.</param>
    public async Task Process(L5X content)
    {
        if (Spec is null) throw new InvalidOperationException("");
        if (content is null) throw new ArgumentNullException(nameof(content));

        if (Evaluations.Count > 0) Evaluations.Clear();

        var stopwatch = Stopwatch.StartNew();
        var verifications = (await Spec.Run(content)).ToList();
        stopwatch.Stop();

        Result = verifications.Count > 0 ? verifications.Max(r => r.Result) : ResultState.None;
        Duration = stopwatch.ElapsedMilliseconds;
        Evaluations.AddRange(verifications.SelectMany(v => v.Evaluations));
    }

    /// <summary>
    /// Resets the result, duration and evaluations to the default state. This is used prior to running again.
    /// </summary>
    public void Reset()
    {
        Result = ResultState.None;
        Duration = 0;
        Evaluations.Clear();
    }

    /// <summary>
    /// Updates the <see cref="Spec"/> associated with the <see cref="Outcome"/>.
    /// </summary>
    /// <param name="spec">The new <see cref="Spec"/> to be associated with the <see cref="Outcome"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="spec"/> is null.</exception>
    public void Update(Spec spec)
    {
        Spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }
}