using System;

namespace L5Spex.Features.Sources;

public record SourceRecord(Guid Id, string Path, bool Selected, bool Pinned)
{
    private SourceRecord() : this(default, default, default, default)
    {
    }
    
    public SourceRecord(string path) : this(Guid.NewGuid(), path, false, false)
    {
        Path = path;
    }

    public SourceRecord(string Path, bool Selected, bool Pinned) : this(Guid.NewGuid(), Path, Selected, Pinned)
    {
    }
    
    public SourceRecord Select(bool selected) => this with {Selected = selected};
    
    public SourceRecord Pin(bool pinned) => this with {Pinned = pinned};
}