using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Outcome
{
    public Outcome(Node node, long duration, IList<Verification> verifications)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        if (verifications is null) throw new ArgumentNullException(nameof(verifications));

        NodeId = node.NodeId;
        Spec = node.Name;
        Path = node.Path;
        Duration = duration;
        Result = verifications.Count > 0 ? verifications.Max(v => v.Result) : ResultState.Passed;
        Verified = verifications.Count;
        Passed = verifications.Count(v => v.Result == ResultState.Passed);
        Failed = verifications.Count(v => v.Result == ResultState.Failed);
        Errored = verifications.Count(v => v.Result == ResultState.Error);
        Verifications = new ReadOnlyCollection<Verification>(verifications);
    }

    public Guid OutcomeId { get; } = Guid.NewGuid();
    public Guid NodeId { get; }
    public ResultState Result { get; }
    public string Spec { get; }
    public string Path { get; }
    public DateTime ProducedOn { get; } = DateTime.Now;
    public long Duration { get; }
    public int Verified { get; }
    public int Passed { get; }
    public int Failed { get; }
    public int Errored { get; }
    public IReadOnlyCollection<Verification> Verifications { get; }

    public IEnumerable<string> Successes => Verifications.Where(v => v.Result == ResultState.Passed)
        .SelectMany(v => v.Evaluations).Select(e => e.Message);

    public IEnumerable<string> Failures => Verifications.Where(v => v.Result == ResultState.Failed)
        .SelectMany(v => v.Evaluations).Select(e => e.Message);

    public IEnumerable<string> Errors => Verifications.Where(v => v.Result == ResultState.Error)
        .SelectMany(v => v.Evaluations).Select(e => e.Message);
}