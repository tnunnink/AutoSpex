using Microsoft.Extensions.Logging;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a single log entry for a runtime operation, including log level, timestamp, and message details.
/// </summary>
public class RunLog
{
    /// <summary>
    /// Gets the severity level of the log entry.
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    public DateTime Logged { get; private init; } = DateTime.Now;

    /// <summary>
    /// Gets the message details of the log entry.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}