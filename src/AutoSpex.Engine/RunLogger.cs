using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AutoSpex.Engine;

public class RunLogger : ILogger
{
    // Static instance for BeginScope to avoid allocations if scopes are not used.
    private static readonly NullScope Instance = new();

    /// <summary>
    /// Gets or sets the minimum log level at which log entries will be recorded.
    /// </summary>
    /// <remarks>
    /// Log entries with a <see cref="LogLevel"/> lower than the value of this property
    /// will be ignored and not recorded. This property acts as a filter to reduce the
    /// volume of logs by specifying the threshold for logging. The default value is
    /// <see cref="LogLevel.Information"/>.
    /// </remarks>
    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets the collection of all log entries recorded during the execution of the application.
    /// </summary>
    /// <remarks>
    /// This property stores a thread-safe collection of <see cref="RunLog"/> instances that
    /// represent detailed information about individual log entries, including their timestamp,
    /// log level, and message. It is primarily used for tracking logs generated during runtime
    /// and is particularly useful for debugging run execution.
    /// </remarks>
    public ConcurrentBag<RunLog> Logs { get; } = [];

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var log = new RunLog
        {
            Level = logLevel,
            Message = formatter(state, exception)
        };

        Logs.Add(log);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && logLevel >= MinLogLevel;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    {
        return Instance;
    }

    /// <summary>
    /// Represents a no-operation implementation of <see cref="IDisposable"/> used as a placeholder for logging scopes.
    /// </summary>
    /// <remarks>
    /// The <c>NullScope</c> class is designed to be a singleton instance that implements
    /// the <see cref="IDisposable"/> interface. It is used primarily to avoid allocations
    /// when logging scopes are not required or used.
    /// </remarks>
    /// <threadsafety>
    /// This class is thread-safe as it does not maintain any state or mutable members.
    /// </threadsafety>
    private sealed class NullScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}