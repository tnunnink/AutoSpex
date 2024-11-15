using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Query()
{
    public Query(string name) : this()
    {
        Name = name;
    }

    private Query(Element element) : this()
    {
        Name = element.Name;
        Element = element;
        Selector = element.This;
    }

    public string Name { get; set; } = string.Empty;
    public Element Element { get; set; } = Element.Default;
    public Property Selector { get; set; } = Property.Default;
    public List<Criterion> Criteria { get; private set; } = [];

    public static List<Query> Predefined => Element.List.Select(e => new Query(e)).ToList();

    public IEnumerable<object> Execute(L5X content)
    {
        return content.Query(Element.Type)
            .Where(e => Criteria.All(c => c.Evaluate(e)))
            .Select(e => Selector.GetValue(e))
            .Cast<object>()
            .ToList();
    }

    public Query Find(Element element)
    {
        return this;
    }

    public Query Where(string property, Operation operation, object? argument)
    {
        return this;
    }

    public Query Select(string property)
    {
        return this;
    }
}