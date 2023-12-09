namespace AutoSpex.Engine;

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