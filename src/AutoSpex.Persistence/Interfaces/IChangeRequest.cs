namespace AutoSpex.Persistence;

public interface IChangeRequest
{
    IEnumerable<Change> GetChanges();
}