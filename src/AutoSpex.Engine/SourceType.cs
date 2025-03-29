using Ardalis.SmartEnum;
using L5Sharp.Core;
using L5Sharp.Logix;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a type of source, categorized by its usage or structure, such as Markup, Archive, or Compressed.
/// This is an abstract class that extends the functionality of the SmartEnum base class.
/// </summary>
public abstract class SourceType : SmartEnum<SourceType, int>
{
    private SourceType(string name, int value) : base(name, value)
    {
    }

    public static readonly SourceType Unknown = new UnknownSourceType();
    public static readonly SourceType Markup = new MarkupSourceType();
    public static readonly SourceType Archive = new ArchiveSourceType();
    public static readonly SourceType Compressed = new CompressedSourceType();

    /// <summary>
    /// Determines the source type based on the file extension.
    /// </summary>
    /// <param name="extension">The file extension to determine the source type from.</param>
    /// <returns>The corresponding SourceType based on the file extension, or Unknown if not found.</returns>
    public static SourceType FromExtension(string extension)
    {
        return extension.ToUpper() switch
        {
            ".L5X" => Markup,
            ".XML" => Markup,
            ".ACD" => Archive,
            ".L5Z" => Compressed,
            _ => Unknown
        };
    }

    /// <summary>
    /// Gets the file extension associated with the specific <see cref="SourceType"/>.
    /// </summary>
    /// <remarks>
    /// This property returns the default file extension that corresponds to the current <see cref="SourceType"/> instance.
    /// For example, for a Markup source type, this property would return ".L5X".
    /// For types where a specific extension is not defined or applicable, an empty string is returned.
    /// </remarks>
    public virtual string Extension => string.Empty;

    /// <summary>
    /// Asynchronously opens the specified file and returns an <see cref="L5X"/> representation of its contents.
    /// </summary>
    /// <param name="fileName">The full path of the file to open.</param>
    /// <param name="cancellation">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the parsed <see cref="L5X"/> object from the file.</returns>
    /// <exception cref="NotSupportedException">Thrown when the operation is not supported for the specific <see cref="SourceType"/>.</exception>
    public abstract Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default);

   
    private class UnknownSourceType() : SourceType(nameof(Unknown), 0)
    {
        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            throw new NotSupportedException("The operation is not supported for the Unknown source type.");
        }
    }

    private class MarkupSourceType() : SourceType(nameof(Markup), 1)
    {
        public override string Extension => ".L5X";

        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            return L5X.LoadAsync(fileName, L5XOptions.Index, cancellation);
        }
    }

    private class ArchiveSourceType() : SourceType(nameof(Archive), 2)
    {
        public override string Extension => ".ACD";

        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            return ACD.LoadAsync(fileName, L5XOptions.Index, cancellation);
        }
    }

    private class CompressedSourceType() : SourceType(nameof(Compressed), 3)
    {
        public override string Extension => ".L5Z";

        public override async Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            var xml = await fileName.DecompressToAsync(cancellation);
            return L5X.Parse(xml, L5XOptions.Index);
        }
    }
}