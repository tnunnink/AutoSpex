using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;

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

    public static Property Property = new DesignRadixProperty();

    public static NodeObserver SpecNode = SpecNodeObserver();

    public static ObservableCollection<NodeObserver> Nodes = new(GenerateNodes().Select(n => new NodeObserver(n)));

    public static VariableObserver VariableObserver = new DesignVariableObserver();

    public static Variable Variable = new("MyVar", "123");

    public static ObservableCollection<VariableObserver> Variables =
    [
        new DesignVariableObserver(),
        new DesignVariableObserver(),
        new DesignVariableObserver(),
        new DesignVariableObserver(),
        new DesignVariableObserver()
    ];

    public static ArgumentObserver EmptyArgument = new(Argument.Empty);

    public static ArgumentObserver TextArgument = new(new Argument("Literal Text Value"));

    public static ArgumentObserver EnumArgument = new(new Argument(ExternalAccess.ReadOnly));

    public static ArgumentObserver VariableArgument = new(new Argument(new Variable("Var01", "Some Value")));

    public static ObservableCollection<ArgumentObserver> Arguments =
    [
        new DesignArgumentObserver(),
        new DesignArgumentObserver()
    ];

    public static ObservableCollection<Argument> RadixOptions =>
        new(LogixEnum.Options<Radix>().Select(e => new Argument(e)));

    public static SourceObserver Source = new(new Source(L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X")));

    public static ObservableCollection<SourceObserver> Sources =
    [
        new SourceObserver(new Source(L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X")))
            {Name = "Test Source 01"},
        new SourceObserver(new Source(L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X")))
            {Name = "Test Source 02", IsSelected = true},
        new SourceObserver(new Source(L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X")))
            {Name = "Test Source 03"},
    ];

    public static ObservableCollection<LogixElement> DataTypes = new(new DesignSourceObserver().Model.L5X.DataTypes);

    public static ObservableCollection<ElementObserver> Tags = new(new DesignSourceObserver().Model.L5X.Query<Tag>()
        .SelectMany(t => t.Members()).Select(x => new ElementObserver(x)));

    public static ObservableCollection<ElementObserver> Rungs =
        new(new DesignSourceObserver().Model.L5X.Query<Rung>().Select(x => new ElementObserver(x)));

    public static string? RawData = new DesignSourceObserver().Model.L5X.Tags.FirstOrDefault()?.Serialize().ToString();

    public static LogixElement? Tag = new DesignSourceObserver().Model.L5X.Tags.FirstOrDefault();

    public static ElementObserver TagObserver = new(Tag!);

    public static PropertyObserver RadixPropertyObserver => new DesignRadixPropertyObserver();

    public static PropertyObserver TagNamePropertyObserver => new(new DesignTextProperty(),
        new ElementObserver(new Tag {Name = "MyTag"}));

    public static PropertyObserver MembersPropertyObserver => new(new DesignMembersProperty(),
        new ElementObserver(new Tag {Name = "MyTag", Value = new TIMER()}));

    private static IEnumerable<Node> GenerateNodes()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        collection.AddSpec();
        folder.AddSpec();
        yield return collection;
        yield return Node.NewCollection();
    }

    private static NodeObserver SpecNodeObserver()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();
        return new NodeObserver(spec);
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

public class DesignArgumentObserver() : ArgumentObserver(new Argument("Literal Text Value"));

public class DesignRadixProperty() : Property(typeof(Tag), "Radix", typeof(Radix));

public class DesignTextProperty() : Property(typeof(Tag), "TagName", typeof(TagName));

public class DesignMembersProperty() : Property(typeof(Tag), "Members", typeof(IEnumerable<Tag>));

public class DesignRadixPropertyObserver()
    : PropertyObserver(new DesignRadixProperty(), new ElementObserver(new Tag()));

public class DesignVariableObserver() : VariableObserver(new Variable("MyVar", "123"));

public class DesignSourceObserver()
    : SourceObserver(new Source(L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X")));