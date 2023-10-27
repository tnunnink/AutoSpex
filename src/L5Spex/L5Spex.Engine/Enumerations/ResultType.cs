namespace L5Spex.Engine.Enumerations;

[Flags]
public enum ResultType
{
    None,
    Suppressed,
    Passed,
    Failed,
    Inconclusive,
    Error
}