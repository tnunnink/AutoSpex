using System;
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
    public static readonly ObservableCollection<DetailPageModel> Tabs =
    [
        new TestDetailPageModel(),
        new TestDetailPageModel(),
        new TestDetailPageModel(),
        new TestDetailPageModel(),
        new TestDetailPageModel(),
        new TestDetailPageModel()
    ];

    public static readonly ObservableCollection<PageViewModel> Pages =
    [
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel(),
        new TestPageModel()
    ];

    public static ProjectObserver RealProject = new DesignRealProjectObserver();
    public static ProjectObserver FakeProject = new DesignFakeProjectObserver();

    public static readonly ObserverCollection<Project, ProjectObserver> Projects =
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

    public static Property Property = new DesignRadixProperty();

    public static NodeObserver SpecNode = SpecNodeObserver();

    public static ObservableCollection<NodeObserver> Nodes = new(GenerateNodes().Select(n => new NodeObserver(n)));

    public static ObservableCollection<NodeObserver> Specs = new(GenerateSpecs().Select(n => new NodeObserver(n)));

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

    //todo should we get something better than pointing to local disc
    public static L5X Content = L5X.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X");

    //todo should we get something better than pointing to local disc
    public static SourceObserver Source = new(new Source(L5X.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X")));

    public static ObservableCollection<SourceObserver> Sources =
    [
        new SourceObserver(new Source(L5X.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X"))),
        new SourceObserver(new Source(L5X.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X"))),
        new SourceObserver(new Source(L5X.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X"))),
    ];

    public static LogixComponent DataType = new DesignSourceObserver().Model.L5X.DataTypes.First();

    public static ObservableCollection<LogixElement> DataTypes = new(new DesignSourceObserver().Model.L5X.DataTypes);

    public static ObservableCollection<ElementObserver> DataTypeElements =
        new(new DesignSourceObserver().Model.L5X.DataTypes.Select(t => new ElementObserver(t)));

    public static ObservableCollection<ElementObserver> Tags = new(new DesignSourceObserver().Model.L5X.Query<Tag>()
        .Where(t => t.Description is not null).SelectMany(t => t.Members()).Select(x => new ElementObserver(x)));

    public static LogixCode Rung = DesignRung();

    private static LogixCode DesignRung()
    {
        var rung = new DesignSourceObserver().Model.L5X.Query<Rung>().First();
        rung.Text = "XIC(Some_Tag_Name)[GRT(MyTag,1)NEQ(AnotherTag.Member,0)]MOV(0,OutputTag);";
        rung.Comment = "This is a test rung that we are using to mock the look of the UI.";
        return rung;
    }

    public static ObservableCollection<ElementObserver> Rungs =
        new(new DesignSourceObserver().Model.L5X.Query<Rung>().Select(x => new ElementObserver(x)));

    public static string? RawData = new DesignSourceObserver().Model.L5X.Tags.FirstOrDefault()?.Serialize().ToString();

    public static LogixElement? Tag = new DesignSourceObserver().Model.L5X.Tags.Find("TestSimpleTag");

    public static LogixElement? Module = new DesignSourceObserver().Model.L5X.Modules.Find("RackIO");

    public static LogixElement? Program = new DesignSourceObserver().Model.L5X.Programs.Find("MainProgram");

    public static ElementObserver TagObserver = new(Tag!);

    public static PropertyObserver RadixPropertyObserver => new DesignRadixPropertyObserver();

    public static PropertyObserver TagNamePropertyObserver => new(new DesignTextProperty(),
        new ElementObserver(new Tag { Name = "MyTag" }));

    public static PropertyObserver MembersPropertyObserver => new(new DesignMembersProperty(),
        new ElementObserver(new Tag { Name = "MyTag", Value = new TIMER() }));

    public static RunObserver Run => new(new Run());

    private static IEnumerable<Node> GenerateNodes()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        collection.AddSpec();
        folder.AddSpec();
        yield return collection;
        yield return Node.NewContainer();
    }

    private static IEnumerable<Node> GenerateSpecs()
    {
        var collection = Node.NewContainer();
        collection.AddSpec("Test Spec");
        collection.AddSpec("Another Spec");
        collection.AddSpec("A Spec with a longer name then most of the other specs that you'd want");
        var folder = collection.AddContainer();
        folder.AddSpec("Test Folder");
        folder.AddSpec("Sub Spec");
        folder.AddSpec("Contained Spec");
        return collection.Specs();
    }

    private static NodeObserver SpecNodeObserver()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        return new NodeObserver(spec);
    }

    public static EvaluationObserver PassedEvaluation = new(
        Evaluation.Passed(new Criterion("DataType", Operation.Equal, "MyType"), new Tag() { Name = "Custom_Tag_Name" },
            ["MyType"], "MyType"));

    public static EvaluationObserver FailedEvaluation = new(
        Evaluation.Failed(new Criterion("DataType", Operation.Contains, "Pump"),
            new Tag() { Name = "MyProgram:Local_Tag_Name_01.Control.Value" }, ["Pump"], "ValveType"));

    public static EvaluationObserver ErroredEvaluation = new(
        Evaluation.Error(new Criterion("DataType", Operation.Equal, "MyType"), new Tag() { Name = "Custom_Tag_Name" },
            new ArgumentException("Could not execute code due to this throw exception")));

    public static ObservableCollection<EvaluationObserver> Evaluations =
        new(new[] { PassedEvaluation, FailedEvaluation, ErroredEvaluation });

    public static OutcomeObserver OutcomePassed = new(new Outcome(new Spec(Guid.Empty, "Spec That Passed")))
    {
        Result = ResultState.Passed, Evaluations = { PassedEvaluation, PassedEvaluation, PassedEvaluation }
    };

    public static OutcomeObserver OutcomeFailed = new(new Outcome(new Spec(Guid.Empty, "Spec That Failed")))
    {
        Result = ResultState.Failed, Evaluations = { PassedEvaluation, PassedEvaluation, FailedEvaluation }
    };

    public static OutcomeObserver OutcomeErrored = new(new Outcome(new Spec(Guid.Empty, "Spec That Erroed")))
    {
        Result = ResultState.Error, Evaluations = { PassedEvaluation, ErroredEvaluation, FailedEvaluation }
    };

    public static ObservableCollection<OutcomeObserver> Outcomes = [OutcomePassed, OutcomeFailed, OutcomeErrored];
}

public class TestPageModel : PageViewModel
{
    public override string Title => "Test Page";
    public override string Icon => "Container";
}

public class TestDetailPageModel : DetailPageModel
{
    public override string Title => "Details Page";
    public override string Icon => "Container";
}

public class DesignRealProjectObserver()
    : ProjectObserver(new Project(@"C:\Users\tnunn\Documents\Spex\New Project.spex"));

public class DesignFakeProjectObserver() : ProjectObserver(new Project(@"C:\Users\tnunn\Documents\Spex\Fake.spex"));

public class DesignCriterionObserver() : CriterionObserver(new Criterion
{
    Property = "Name",
    Operation = Operation.Equal,
    Arguments = ["Test"]
}, Element.Tag.Type);

public class DesignArgumentObserver() : ArgumentObserver(new Argument("Literal Text Value"));

public class DesignRadixProperty() : Property(typeof(Tag), "Radix", typeof(Radix));

public class DesignTextProperty() : Property(typeof(Tag), "TagName", typeof(TagName));

public class DesignMembersProperty() : Property(typeof(Tag), "Members", typeof(IEnumerable<Tag>));

public class DesignRadixPropertyObserver()
    : PropertyObserver(new DesignRadixProperty(), new ElementObserver(new Tag()));

public class DesignVariableObserver() : VariableObserver(new Variable("MyVar", "123"));

public class DesignSourceObserver()
    : SourceObserver(new Source(L5X.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X")));