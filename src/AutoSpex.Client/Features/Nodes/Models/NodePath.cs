using System;

namespace AutoSpex.Client.Features.Nodes;

public class NodePath
{
    private const char Separator = '/';
    private readonly string _path;

    private NodePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
        _path = path;
    }

    public string Name => Segments[^1];
    public string Parent => Segments[^2];
    public string Project => Segments[0];
    
    public bool Contains(string path) => _path.Contains(path, StringComparison.OrdinalIgnoreCase);
    
    private string[] Segments => _path.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

    public static implicit operator string(NodePath path) => path._path;

    public static implicit operator NodePath(string path) => new(path);

    public override string ToString() => _path;
}