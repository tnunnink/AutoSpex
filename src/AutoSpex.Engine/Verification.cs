using System.Collections;

namespace AutoSpex.Engine;

public class Verification(IEnumerable<Evaluation> evaluations) : IReadOnlyCollection<Evaluation>
{
    private readonly List<Evaluation> _evaluations = evaluations.ToList() ?? throw new ArgumentNullException(nameof(evaluations));

    public int Count => _evaluations.Count;

    public IEnumerator<Evaluation> GetEnumerator() => _evaluations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}