using Ardalis.SmartEnum;
using L5Sharp.Core;

namespace AutoSpex.Engine;

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
            ".ACD" => Archive,
            ".L5Z" => Compressed,
            _ => Unknown
        };
    }

    public virtual string Extension => string.Empty;
    public abstract L5X Open(string fileName);
    public abstract Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default);

    /*public abstract Task<Source> Compress(Source source);
    public abstract Task<Source> CompressAsync(Source source, CancellationToken cancellation = default);*/

    private class UnknownSourceType() : SourceType(nameof(Unknown), 0)
    {
        public override L5X Open(string fileName)
        {
            throw new NotSupportedException("");
        }

        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            throw new NotSupportedException("");
        }
    }

    private class MarkupSourceType() : SourceType(nameof(Markup), 1)
    {
        public override L5X Open(string fileName)
        {
            throw new NotImplementedException();
        }

        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }

    private class ArchiveSourceType() : SourceType(nameof(Archive), 2)
    {
        public override L5X Open(string fileName)
        {
            throw new NotImplementedException();
        }

        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }

    private class CompressedSourceType() : SourceType(nameof(Compressed), 3)
    {
        public override L5X Open(string fileName)
        {
            throw new NotImplementedException();
        }

        public override Task<L5X> OpenAsync(string fileName, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}