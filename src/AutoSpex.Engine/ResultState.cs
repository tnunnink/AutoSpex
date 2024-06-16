namespace AutoSpex.Engine;

[Flags]
public enum ResultState
{
    None,
    Pending,
    Running,
    Inconclusive,
    Passed,
    Failed,
    Error
}