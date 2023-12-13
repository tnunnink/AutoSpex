using System.Collections;

namespace AutoSpex.Engine;

public class Verification : IReadOnlyCollection<Evaluation>
{
    private readonly List<Evaluation> _evaluations;

    public Verification(IEnumerable<Evaluation> evaluations)
    {
        _evaluations = evaluations.ToList() ?? throw new ArgumentNullException(nameof(evaluations));
    }
    
    public int Count => _evaluations.Count;

    public IEnumerator<Evaluation> GetEnumerator() => _evaluations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}