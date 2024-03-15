using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public record Outcome
{
    [UsedImplicitly]
    private Outcome()
    {
    }

    public Outcome(Node node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        
        NodeId = node.NodeId;
        Node = node;
    }

    public Outcome(Node node, long duration, IList<Verification> verifications)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        if (verifications is null) throw new ArgumentNullException(nameof(verifications));

        NodeId = node.NodeId;
        Node = node;
        Duration = duration;
        Result = verifications.Count > 0 ? verifications.Max(v => v.Result) : ResultState.Passed;
        Verified = verifications.Count;
        Passed = verifications.Count(v => v.Result == ResultState.Passed);
        Failed = verifications.Count(v => v.Result == ResultState.Failed);
        Errored = verifications.Count(v => v.Result == ResultState.Error);
        Verifications = new ReadOnlyCollection<Verification>(verifications);
    }

    public Guid OutcomeId { get; } = Guid.NewGuid();
    public Guid NodeId { get; } = Guid.Empty;
    public Node? Node { get; set; } 
    public ResultState Result { get; private set; } = ResultState.None;
    public long Duration { get; private set; }
    public int Verified { get; private set; }
    public int Passed { get; private set; }
    public int Failed { get; private set; }
    public int Errored { get; private set; }
    public IEnumerable<Verification> Verifications { get; private set; } = [];

    public IEnumerable<string> Successes => Verifications.Where(v => v.Result == ResultState.Passed)
        .SelectMany(v => v.Evaluations).Select(e => e.Message);

    public IEnumerable<string> Failures => Verifications.Where(v => v.Result == ResultState.Failed)
        .SelectMany(v => v.Evaluations).Select(e => e.Message);

    public IEnumerable<string> Errors => Verifications.Where(v => v.Result == ResultState.Error)
        .SelectMany(v => v.Evaluations).Select(e => e.Message);
}