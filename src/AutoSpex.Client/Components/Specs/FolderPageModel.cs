using System;

namespace AutoSpex.Client.Components;

public partial class FolderPageModel(Observers.NodeObserver node) : NodePageModel(node)
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