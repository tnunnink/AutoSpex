namespace AutoSpex.Persistence;

public interface IDbLoggable
{
    /// <summary>
    /// The <see cref="Guid"/> ID of the node that the command request is for. 
    /// </summary>
    Guid NodeId { get; }
    
    /// <summary>
    /// The message to log to the database the gives context to what changed.
    /// </summary>
    string Message { get; }
}