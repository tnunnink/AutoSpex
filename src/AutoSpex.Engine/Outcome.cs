using System.Text.Json;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome : IEquatable<Outcome>
{
    private List<Evaluation> _evaluations = [];

    public Outcome()
    {
    }

    public Outcome(string spec, string source, IEnumerable<Evaluation> evaluations)
    {
        Spec = Node.NewSpec(spec);
        Source = Node.NewSource(source);
        _evaluations = [..evaluations];
    }

    public Outcome(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Type == NodeType.Spec)
            ConfigureSpec(node);
        if (node.Type == NodeType.Source)
            ConfigureSource(node);
    }

    public Outcome(Node? spec, Node? source)
    {
        ConfigureSpec(spec);
        ConfigureSource(source);
    }

    public Outcome(Spec spec, Source source, ICollection<Verification> verifications, long duration)
    {
        if (spec is null) throw new ArgumentNullException(nameof(spec));
        if (source is null) throw new ArgumentNullException(nameof(source));

        Spec = Node.Create(spec.SpecId, NodeType.Spec, spec.Name);
        Source = Node.Create(source.SourceId, NodeType.Source, source.Name);
        Result = verifications.Count > 0 ? verifications.Max(r => r.Result) : ResultState.Inconclusive;
        Duration = duration;
        _evaluations = verifications.SelectMany(v => v.Evaluations).ToList();
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global needs to be set by Dapper
    public Guid OutcomeId { get; init; } = Guid.NewGuid();
    public Node? Spec { get; private set; }
    public Node? Source { get; private set; }
    public ResultState Result { get; private set; }
    public long Duration { get; private set; }
    public IEnumerable<Evaluation> Evaluations => _evaluations.AsReadOnly();

    public Outcome ConfigureSpec(Node? node)
    {
        if (node is null || node.Feature == NodeType.Spec)
        {
            Spec = node;
        }

        return this;
    }

    public Outcome ConfigureSource(Node? node)
    {
        if (node is null || node.Feature == NodeType.Source)
        {
            Source = node;
        }

        return this;
    }

    public Outcome ConfigureEvaluations(string data)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonEvaluationConverter());

        var evaluations = JsonSerializer.Deserialize<List<Evaluation>>(data, options);
        if (evaluations is null) return this;

        _evaluations.Clear();
        _evaluations.AddRange(evaluations);
        return this;
    }

    public string GetEvaluationData()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonEvaluationConverter());
        return JsonSerializer.Serialize(_evaluations, options);
    }

    public async Task Run(Source source, Spec spec, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(spec);

        if (Source?.NodeId != source.SourceId)
            throw new ArgumentException("The provided source id does not match the configured Outcome source id.");

        if (Spec?.NodeId != spec.SpecId)
            throw new ArgumentException("The provided spec id does not match the configured Outcome spec id.");

        var outcome = await spec.Run(source, token);
        Result = outcome.Result;
        Duration = outcome.Duration;
        _evaluations = [..outcome.Evaluations];
    }

    public bool Equals(Outcome? other) => OutcomeId.Equals(other?.OutcomeId);
    public override bool Equals(object? obj) => obj is Outcome outcome && Equals(outcome);
    public override int GetHashCode() => OutcomeId.GetHashCode();
}