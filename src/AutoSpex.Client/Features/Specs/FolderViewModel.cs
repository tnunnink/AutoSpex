using System;

namespace AutoSpex.Client.Features;

public partial class FolderViewModel(NodeObserver node) : NodeViewModel(node)
{
    protected override Task Load()
    {
        return base.Load();
    }

    protected override Task Save()
    {
        throw new NotImplementedException();
    }

    protected override bool CanSave()
    {
        throw new NotImplementedException();
    }
}