using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;

namespace AutoSpex.Client.Components;

public static class DesignProjects
{
    public static readonly ObservableCollection<ProjectObserver> Projects =
    [
        new DesignRealProjectObserver(),
        new DesignFakeProjectObserver(),
        new DesignRealProjectObserver()
    ];
}

public class DesignRealProjectObserver() : ProjectObserver(new Project(@"C:\Users\admin\Documents\Spex\Test.spex"));
public class DesignFakeProjectObserver() : ProjectObserver(new Project(@"C:\Users\admin\Documents\Spex\Fake.spex"));