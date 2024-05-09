namespace AutoSpex.Engine;

[Flags]
public enum ResultState
{
    None,
    Pending,
    Passed,
    Failed,
    Error
}