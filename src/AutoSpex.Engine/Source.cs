using System.Security.Cryptography;
using System.Text;
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

        Location = file.FullName;
        Name = Path.GetFileNameWithoutExtension(file.Name);
        Type = SourceType.FromExtension(file.Extension);
        UpdatedOn = file.LastWriteTimeUtc;
        Size = file.Length;
    }

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

    /// <summary>
    /// Creates a new instance of the Source class using the specified file location.
    /// </summary>
    /// <param name="location">The file path to the source file for which the Source instance will be created.</param>
    /// <returns>A new instance of the Source class representing the specified file.</returns>
    /// <exception cref="ArgumentException">Thrown if the location is null, empty, or the file does not exist at the specified location.</exception>
    /// <exception cref="NotSupportedException">Thrown if the file extension is not supported.</exception>
    public static Source Create(string location)
    {
        if (string.IsNullOrEmpty(location))
            throw new ArgumentException("Location is required to create source.");

        if (!File.Exists(location))
            throw new ArgumentException($"No file exists at provided location '{location}'");

        if (!SupportedExtensions.Contains(Path.GetExtension(location).ToUpper()))
            throw new NotSupportedException("The specified file extension is not supported.");

        return new Source(new FileInfo(location));
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

    /// <summary>
    /// Computes the MD5 hash of the location of the source file.
    /// </summary>
    /// <returns>The MD5 hash string of the source file's location.</returns>
    public string HashLocation()
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(Location));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Computes the hash of the content of the source file.
    /// </summary>
    /// <returns>The MD5 hash string representing the content of the source file.</returns>
    public string HashContent()
    {
        using var stream = File.Open(Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var hash = MD5.HashData(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}