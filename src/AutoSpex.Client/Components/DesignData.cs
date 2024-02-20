using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;
using Tag = HarfBuzzSharp.Tag;

namespace AutoSpex.Client.Components;

[PublicAPI]
public static class DesignData
{
    public static readonly ObservableCollection<PageViewModel> Pages =
    [
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel()
    ];

    public static readonly ObservableCollection<ProjectObserver> Projects =
    [
        new DesignRealProjectObserver(),
        new DesignFakeProjectObserver(),
        new DesignRealProjectObserver()
    ];

    public static readonly ObservableCollection<CriterionObserver> Criteria =
    [
        new DesignCriterionObserver(),
        new DesignCriterionObserver(),
        new DesignCriterionObserver()
    ];

    public static readonly Breadcrumb ParentBreadcrumb = new DesignParentBreadcrumb();
    public static readonly Breadcrumb TargetBreadcrumb = new DesignTargetBreadcrumb();
    public static readonly Breadcrumb PathBreadcrumb = new DesignPathBreadcrumb();

    public static readonly ObservableCollection<Breadcrumb> Breadcrumbs =
    [
        new DesignParentBreadcrumb(),
        new DesignTargetBreadcrumb()
    ];

    public static Property Property = new DesignProperty();

    public static ObservableCollection<NodeObserver> Nodes = new(GenerateNodes().Select(n => new NodeObserver(n)));

    public static ObservableCollection<VariableObserver> Variables =
    [
        new DesignVariableObserver(),
        new DesignVariableObserver(),
        new DesignVariableObserver(),
        new DesignVariableObserver(),
        new DesignVariableObserver()
    ];

    public static ObservableCollection<ArgumentObserver> Arguments =
    [
        new DesignArgumentObserver(),
        new DesignArgumentObserver()
    ];

    public static ObservableCollection<Argument> RadixOptions =>
        new(LogixEnum.Options<Radix>().Select(e => new Argument(e)));

    public static SourceObserver Source = new DesignSourceObserver();
    
    public static ObservableCollection<SourceObserver> Sources =
    [
        new DesignSourceObserver(),
        new DesignSourceObserver(),
        new DesignSourceObserver()
    ];

    private static IEnumerable<Node> GenerateNodes()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        collection.AddSpec();
        folder.AddSpec();
        yield return collection;
        yield return Node.NewCollection();
    }
}

public class TestPageModel : PageViewModel
{
    public override string Title => "Test Page";
    public override string Icon => "Folder";
}

public class DesignRealProjectObserver() : ProjectObserver(new Project(@"C:\Users\admin\Documents\Spex\Test.spex"));

public class DesignFakeProjectObserver() : ProjectObserver(new Project(@"C:\Users\admin\Documents\Spex\Fake.spex"));

public class DesignParentBreadcrumb() : Breadcrumb(Node.NewCollection(), CrumbType.Parent);

public class DesignTargetBreadcrumb() : Breadcrumb(Node.NewSpec(), CrumbType.Target);

public class DesignPathBreadcrumb() : Breadcrumb(Node.NewCollection().AddFolder().AddSpec(), CrumbType.Target);

public class DesignCriterionObserver() : CriterionObserver(new Criterion(), Element.Tag.Type);

public class DesignArgumentObserver()
    : ArgumentObserver(new Argument("SomeData"), new CriterionObserver(new Criterion(), Element.Tag.Type));

public class DesignProperty() : Property(typeof(Tag), "Radix", typeof(Radix));

public class DesignVariableObserver() : VariableObserver(new Variable("MyVar", "123", "This is a test"));

public class DesignSourceObserver()
    : SourceObserver(new Source(L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X")));