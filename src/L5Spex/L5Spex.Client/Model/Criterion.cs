namespace L5Spex.Model;

public class Criterion
{
    public string Type { get; set; }
    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    public Operator Operator { get; set; }
    public object Value { get; set; }
}

public enum Operator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith
}