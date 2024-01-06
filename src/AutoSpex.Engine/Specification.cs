namespace AutoSpex.Engine;

public class Specification
{
    private readonly List<Criterion> _filters = [];
    private readonly List<Criterion> _verifications = [];

    private Specification(Element element)
    {
        Element = element;
    }

    public Element Element { get; private init; }
    public SpecificationOptions Options { get; private init; } = SpecificationOptions.Default;
    public Criterion Range { get; private set; } = new(Operation.GreaterThan, 0);
    public IEnumerable<Criterion> Filters => _filters;
    public IEnumerable<Criterion> Verifications => _verifications;


    public static Specification For(Element element) => new(element);

    public void Filter(Criterion criterion)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        _filters.Add(criterion);
    }

    public Criterion Filter(Action<Criterion>? config)
    {
        var criterion = new Criterion();
        config?.Invoke(criterion);
        _filters.Add(criterion);
        return criterion;
    }

    public void Verify(Criterion criterion)
    {
        if (criterion is null)
            throw new ArgumentNullException(nameof(criterion));

        _verifications.Add(criterion);
    }
    
    public Criterion Verify(Action<Criterion>? config = default)
    {
        var criterion = new Criterion();
        config?.Invoke(criterion);
        _verifications.Add(criterion);
        return criterion;
    }

    public void Remove(Criterion criterion) => RemoveCriterion(criterion);

    public void Remove(IEnumerable<Criterion> criteria)
    {
        foreach (var criterion in criteria)
            RemoveCriterion(criterion);
    }

    private void RemoveCriterion(Criterion criterion)
    {
        _filters.Remove(criterion);
        _verifications.Remove(criterion);
    }
}