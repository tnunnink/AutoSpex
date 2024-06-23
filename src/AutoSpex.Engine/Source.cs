using System.Xml.Linq;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a L5X file that has been added to the project. We use the sources to run against a set of specifications.
/// Uses can add and update sources as needed. This class will compress the L5X data to conserve space in the project
/// file since we intend to add many sources. Every source should also have a corresponding <see cref="Node"/> to which
/// it belongs.
/// </summary>
public class Source
{
    private L5X? _l5X;

    public Source()
    {
    }

    public Source(Node node)
    {
        SourceId = node.NodeId;
        Name = node.Name;
    }

    public Source(L5X l5X)
    {
        Update(l5X, true);
    }

    public Guid SourceId { get; private init; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public string TargetName { get; private set; } = string.Empty;
    public DateTime ExportedOn { get; private set; } = DateTime.Now;
    public string ExportedBy { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public L5X L5X => _l5X ??= ParseContent();

    /// <summary>
    /// Updates this source's content and properties to that of the provided <see cref="L5X"/> file.
    /// Optionally scrubs the L5K data if needed.
    /// </summary>
    /// <param name="l5X">The <see cref="L5X"/> file to update this source with.</param>
    /// <param name="scrub">The option to remove all L5K data to conserve space.</param>
    /// <exception cref="ArgumentNullException"><paramref name="l5X"/> is null.</exception>
    public void Update(L5X l5X, bool scrub)
    {
        _l5X = l5X ?? throw new ArgumentNullException(nameof(l5X));

        SeedId(_l5X, SourceId);
        if (scrub) ScrubData(_l5X);

        TargetType = _l5X.Info.TargetType ?? string.Empty;
        TargetName = _l5X.Info.TargetName ?? string.Empty;
        ExportedOn = _l5X.Info.ExportDate ?? DateTime.Today;
        ExportedBy = _l5X.Info.Owner ?? string.Empty;
        Content = _l5X.ToString().Compress();
    }

    /// <summary>
    /// Decompresses and parses the string content into a <see cref="L5X"/> object to be queried.
    /// </summary>
    private L5X ParseContent()
    {
        if (string.IsNullOrEmpty(Content) || string.IsNullOrWhiteSpace(Content))
            return L5X.Empty();

        var xml = Content.Decompress();

        return L5X.Parse(xml);
    }

    /// <summary>
    /// Removes all L5K formatted data from the file to conserve space. This can be controlled by the user.
    /// </summary>
    private static void ScrubData(ILogixSerializable content)
    {
        var root = content.Serialize();

        var data = root.Descendants(L5XName.Data)
            .Where(e => e.Attribute(L5XName.Format)?.Value == DataFormat.L5K)
            .ToList();

        foreach (var element in data)
        {
            element.Remove();
        }
    }

    /// <summary>
    /// Injects the source id of this object into the L5X content so that we can later get which source the containing
    /// elements/data belong to. 
    /// </summary>
    private static void SeedId(ILogixSerializable content, Guid id)
    {
        var root = content.Serialize();
        root.Add(new XAttribute(nameof(SourceId), id.ToString()));
    }
}