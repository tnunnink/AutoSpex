using System.Xml.Linq;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Source
{
    private L5X? _l5X;

    public Source()
    {
    }

    public Source(Guid id, string? name = default)
    {
        SourceId = id;
        Name = name ?? string.Empty;
    }

    public Source(L5X l5X)
    {
        Update(l5X, true);
    }

    public Guid SourceId { get; private init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public string TargetName { get; private set; } = string.Empty;
    public DateTime ExportedOn { get; private set; } = DateTime.Now;
    public string ExportedBy { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public L5X L5X => _l5X ??= ParseContent();

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

    private L5X ParseContent()
    {
        if (string.IsNullOrEmpty(Content) || string.IsNullOrWhiteSpace(Content))
            return L5X.Empty();

        var xml = Content.Decompress();

        return L5X.Parse(xml);
    }

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

    private static void SeedId(ILogixSerializable content, Guid id)
    {
        var root = content.Serialize();

        root.Add(new XAttribute(nameof(SourceId), id.ToString()));
    }
}