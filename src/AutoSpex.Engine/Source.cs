using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a source file that can be used to run specifications against.
/// </summary>
public class Source
{
    private static readonly HashSet<string> SupportedExtensions = [".L5X", ".ACD", ".L5Z", ".XML"];

    private Source(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        Hash = file.ComputeHash();
        Location = file.FullName;
        Name = Path.GetFileNameWithoutExtension(file.Name);
        Type = SourceType.FromExtension(file.Extension);
        UpdatedOn = file.LastWriteTimeUtc;
        Size = file.Length;
    }

    /// <summary>
    /// The hash of the file metadata (location, last write time, and size). This is what we will use to identify a
    /// source file and whether it has changes.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// The absolute location on disc of the source file.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// The name of the source file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The <see cref="SourceType"/> indicating whether this is an L5X, ACD, or some other file format. This information
    /// is used to determine how we interact (open, compress, store) the file.
    /// </summary>
    public SourceType Type { get; }

    /// <summary>
    /// The time the file was last updated or written to.
    /// </summary>
    public DateTime UpdatedOn { get; }

    /// <summary>
    /// The size in bytes of the file.
    /// </summary>
    public long Size { get; }


    public static Source Create(string location)
    {
        if (string.IsNullOrEmpty(location))
            throw new ArgumentException("Location is required to create source.");

        if (!File.Exists(location))
            throw new ArgumentException($"No file exists at provided location '{location}'");

        if (!SupportedExtensions.Contains(Path.GetExtension(location)))
            throw new NotSupportedException("The specified file extension is not supported.");

        return new Source(new FileInfo(location));
    }

    /// <summary>
    /// Opens the source file and loads its content as an L5X object.
    /// </summary>
    /// <returns>The content of the source file as an L5X object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the source file has not been properly staged.</exception>
    public L5X Open()
    {
        if (!File.Exists(Location))
            throw new InvalidOperationException($"Source no longer exists at '{Location}'.");

        return Type.Open(Location);
    }

    /// <summary>
    /// Asynchronously loads the L5X content of the source from the specified location on disk.
    /// </summary>
    /// <param name="token">The cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the loaded L5X content.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the source file has not been properly staged.</exception>
    public async Task<L5X> OpenAsync(CancellationToken token = default)
    {
        if (!File.Exists(Location))
            throw new InvalidOperationException($"Source no longer exists at '{Location}'.");

        return await Type.OpenAsync(Location, token);
    }
}