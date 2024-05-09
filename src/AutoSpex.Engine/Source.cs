using System.Xml.Linq;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

[PublicAPI]
public class Source
{
    private L5X? _l5X;
    private readonly Dictionary<Guid, string> _overrides = [];
    
    private HashSet<string> _elementNames =
        [L5XName.Description, L5XName.Comment, L5XName.Text, L5XName.RevisionNote, L5XName.AdditionalHelpText];
    

    [UsedImplicitly]
    private Source()
    {
    }

    public Source(L5X l5X)
    {
        _l5X = l5X ?? throw new ArgumentNullException(nameof(l5X));

        Name = _l5X.Info.TargetName ?? "New Source";
        Documentation = _l5X.Controller.Description ?? string.Empty;
        TargetType = _l5X.Info.TargetType ?? string.Empty;
        TargetName = _l5X.Info.TargetName ?? string.Empty;
        ExportedOn = _l5X.Info.ExportDate ?? DateTime.Today;
        ExportedBy = _l5X.Info.Owner ?? string.Empty;
        Content = _l5X.ToString().Compress();
    }

    public Guid SourceId { get; private init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public string Documentation { get; set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public string TargetName { get; private set; } = string.Empty;
    public DateTime ExportedOn { get; private set; } = DateTime.Now;
    public string ExportedBy { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public L5X L5X => _l5X ??= ParseContent();

    public void Update(L5X l5X)
    {
        _l5X = l5X ?? throw new ArgumentNullException(nameof(l5X));
        TargetType = _l5X.Info.TargetType ?? string.Empty;
        TargetName = _l5X.Info.TargetName ?? string.Empty;
        ExportedOn = _l5X.Info.ExportDate ?? DateTime.Today;
        ExportedBy = _l5X.Info.Owner ?? string.Empty;
        Content = _l5X.ToString().Compress();
    }

    public IEnumerable<string> GetDistinctValues()
    {
        var values = new HashSet<string>();

        foreach (var element in L5X.Serialize().DescendantsAndSelf())
        {
            if (!_elementNames.Contains(element.Name.LocalName)) continue;
            var value = element.Value.Trim().Trim(';');
            if (string.IsNullOrEmpty(value)) continue;
            values.Add(element.Value);
        }

        foreach (var attribute in L5X.Serialize().DescendantsAndSelf().Attributes())
        {
            var value = attribute.Value.Trim();
            if (string.IsNullOrEmpty(value)) continue;
            values.Add(attribute.Value);
        }

        return values;
    }

    private L5X ParseContent()
    {
        if (string.IsNullOrEmpty(Content) || string.IsNullOrWhiteSpace(Content)) 
            return L5X.New("Temp", "Temp"); //todo we need empty
        
        var xml = Content.Decompress();
        
        return L5X.Parse(xml);
    }
    
    public class SourceValueComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => StringComparer.Ordinal.Equals(x, y);

        public int GetHashCode(string obj) => obj.StableHash();
    }
}