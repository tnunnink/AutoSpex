using AutoSpex.Engine;

namespace AutoSpex.Persistence;

public record Change
{
    private Change()
    {
    }

    /// <summary>
    /// Represents the unique identifier for a change.
    /// </summary>
    public Guid ChangeId { get; } = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for the entity that changed.
    /// </summary>
    public Guid EntityId { get; init; } = Guid.Empty;

    /// <summary>
    /// The name of the request that this change was associated with.
    /// </summary>
    public string Request { get; init; } = string.Empty;

    /// <summary>
    /// Represents the type of change associated with a record.
    /// </summary>
    public ChangeType ChangeType { get; init; } = ChangeType.None;

    /// <summary>
    /// Represents the timestamp when the change occurred, in UTC.
    /// </summary>
    public DateTime ChangedOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Represents the user who made the change.
    /// </summary>
    public string ChangedBy { get; } = Environment.UserName;

    /// <summary>
    /// Represents the additional message associated with the change.
    /// </summary>
    public string Message { get; init; } = string.Empty;


    /// <summary>
    /// Creates a new instance of a Change record with the specified parameters.
    /// </summary>
    /// <param name="id">The GUID of the entity associated with the change.</param>
    /// <param name="type">The type of change as a ChangeType enumeration.</param>
    /// <param name="message">A message detailing the change.</param>
    /// <typeparam name="TRequest">The type of change request for the change.</typeparam>
    /// <returns>A new instance of the Change record.</returns>
    public static Change For<TRequest>(Guid id, ChangeType type, string message) where TRequest : IChangeRequest
    {
        return new Change
        {
            EntityId = id,
            Request = typeof(TRequest).Name,
            ChangeType = type,
            Message = message
        };
    }
}