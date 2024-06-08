

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global used with Dapper

namespace AutoSpex.Persistence;

public record ChangeLog
{
    public string Command { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime ChangedOn { get; init; } = DateTime.Now;
    public string ChangedBy { get; init; } = string.Empty;
}