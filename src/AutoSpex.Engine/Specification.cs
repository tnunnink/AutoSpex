using AutoSpex.Engine.Operations;

namespace AutoSpex.Engine;

public class Specification
{
    private readonly List<Criterion> _filters = new();
    private readonly List<Criterion> _verifications = new();

    private Specification(Element element)
    {
        Element = element;
    }

    public Element Element { get; }

    public IEnumerable<Criterion> Filters => _filters;

    public Criterion Range { get; private set; } = new(Operation.GreaterThan, 0);

    public IEnumerable<Criterion> Verifications => _verifications;

    public static Specification For(Element element) => new(element);

    public Specification WithFilter(Criterion criterion)
    {
        if (criterion is null) throw new ArgumentNullException(nameof(criterion));
        _filters.Add(criterion);
        return this;
    }

    public Specification WithFilters(ICollection<Criterion> criteria)
    {
        if (criteria is null) throw new ArgumentNullException(nameof(criteria));
        _filters.AddRange(criteria);
        return this;
    }

    public Specification VerifyRange(Operation operation, params Arg[] arguments)
    {
        Range = new Criterion(operation, arguments);
        return this;
    }

    public Specification Verify(Criterion criterion)
    {
        if (criterion is null) throw new ArgumentNullException(nameof(criterion));
        _verifications.Add(criterion);
        return this;
    }

    public Specification Verify(ICollection<Criterion> criteria)
    {
        if (criteria is null) throw new ArgumentNullException(nameof(criteria));
        _verifications.AddRange(criteria);
        return this;
    }
}