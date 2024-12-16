namespace AutoSpex.Engine;

public record Comment()
{
    public Comment(string message) : this()
    {
        Message = message;
    }

    /// <summary>
    /// The unique ID of this comment.
    /// </summary>
    public Guid CommentId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The unique ID of the object that this comment belongs to.
    /// </summary>
    public Guid OwnerId { get; set; } = Guid.Empty;

    /// <summary>
    /// THe comment message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The user that posted the comment.
    /// </summary>
    public string User { get; init; } = Environment.UserName;

    /// <summary>
    /// The date/time that the comment was posted.
    /// </summary>
    public DateTime Posted { get; init; } = DateTime.Now;
}