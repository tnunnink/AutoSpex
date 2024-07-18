using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using JetBrains.Annotations;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;
using Environment = AutoSpex.Engine.Environment;
using Range = AutoSpex.Engine.Range;

namespace AutoSpex.Client.Components;

[PublicAPI]
public static class DesignData
{
    private const string TestSource = @"C:\Users\tnunn\Documents\L5X\Test.L5X";

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

    public static readonly CriterionObserver BoolCriterion =
        new(new Criterion(Element.Tag.Property("Constant"), Operation.True));

    public static readonly CriterionObserver NumberCriterion =
        new(new Criterion(Element.Tag.Property("Dimensions"), Operation.GreaterThanOrEqualTo, new Argument(10)));

    public static readonly CriterionObserver TextCriterion =
        new(new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test"));

    public static readonly CriterionObserver EnumCriterion =
        new(new Criterion(Element.Tag.Property("Radix"), Operation.EqualTo, Radix.Binary));

    public static readonly CriterionObserver InnerCriterion =
        new(new Criterion(Element.Tag.Property("Members"), Operation.Any,
            new Criterion(Element.Tag.Property("TagName"), Operation.Like, "%MemberName")));

    public static readonly CriterionObserver RangeCriterion = new Range().Criterion;

    public static readonly ObservableCollection<CriterionObserver> Criteria =
    [
        new DesignCriterionObserver(),
        new DesignCriterionObserver(),
        new DesignCriterionObserver()
    ];

    public static Property Property = new DesignRadixProperty();


    public static EnvironmentObserver EnvironmentItemDefault = new(Environment.Default);

    public static EnvironmentObserver EnvironmentItemTest = new(new Environment
    {
        Name = "Some Test Config",
        Comment = "This config is for testing the interface.",
        IsTarget = false
    });

    public static ObservableCollection<EnvironmentObserver> Environments =
        [EnvironmentItemDefault, EnvironmentItemTest];


    public static SpecObserver SpecObserver =
        new(Spec.Configure(c =>
            c.Find(Element.Tag)
                .Where(Element.Tag.Property("TagName"), Operation.Containing, "TestTag")
                .ShouldHave(Element.Tag.Property("Value"), Operation.EqualTo, 123)
        ));

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

    public static ArgumentObserver EmptyArgument = new(new Argument(string.Empty));

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

    #region Source

    public static SourceObserver SourceTest = new(new Source(new Uri(@"C:\Users\tnunn\Documents\L5X\Test.L5X")));

    public static SourceObserver SourceFake = new(new Source(new Uri(@"C:\Users\tnunn\Documents\L5X\Fake.L5X")));

    public static ObservableCollection<SourceObserver> Sources = [SourceTest, SourceTest, SourceFake];

    #endregion

    #region LogixElements

    public static LogixComponent DataType = SourceTest.Model.Load().DataTypes.First();

    public static ObservableCollection<LogixElement> DataTypes = new(SourceTest.Model.Load().DataTypes);

    public static ObservableCollection<ElementObserver> DataTypeElements =
        new(SourceTest.Model.Load().DataTypes.Select(t => new ElementObserver(t)));

    public static ObservableCollection<ElementObserver> Tags = new(SourceTest.Model.Load().Query<Tag>()
        .Where(t => t.Description is not null).Select(x => new ElementObserver(x)));

    public static LogixCode Rung = DesignRung();

    private static LogixCode DesignRung()
    {
        var rung = SourceTest.Model.Load().Query<Rung>().First();
        rung.Text = "XIC(Some_Tag_Name)[GRT(MyTag,1)NEQ(AnotherTag.Member,0)]MOV(0,OutputTag);";
        rung.Comment = "This is a test rung that we are using to mock the look of the UI.";
        return rung;
    }

    public static ObservableCollection<ElementObserver> Rungs =
        new(SourceTest.Model.Load().Query<Rung>().Select(x => new ElementObserver(x)));

    public static string? RawData = SourceTest.Model.Load().Tags.FirstOrDefault()?.Serialize().ToString();

    public static LogixElement? Tag = SourceTest.Model.Load().Tags.Find("TestSimpleTag");

    public static LogixElement? Module = SourceTest.Model.Load().Modules.Find("RackIO");

    public static LogixElement? Program = SourceTest.Model.Load().Programs.Find("MainProgram");

    public static ElementObserver TagObserver = new(Tag!);

    #endregion


    public static PropertyObserver RadixPropertyObserver => new DesignRadixPropertyObserver();

    public static PropertyObserver TagNamePropertyObserver => new(new DesignTextProperty(),
        new ElementObserver(new Tag { Name = "MyTag" }));

    public static PropertyObserver MembersPropertyObserver => new(new DesignMembersProperty(),
        new ElementObserver(new Tag { Name = "MyTag", Value = new TIMER() }));

    #region Nodes

    public static NodeObserver SpecNode = SpecNodeObserver();

    public static ObservableCollection<NodeObserver> Nodes = new(GenerateNodes().Select(n => new NodeObserver(n)));

    public static ObservableCollection<NodeObserver> SpecsNodes = new(GenerateSpecs().Select(n => new NodeObserver(n)));

    private static IEnumerable<Node> GenerateNodes()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddContainer();
        collection.AddSpec();
        folder.AddSpec();
        yield return collection;
        yield return Node.NewCollection();
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
        return collection.Descendants(NodeType.Spec);
    }

    private static NodeObserver SpecNodeObserver()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        return new NodeObserver(spec);
    }

    #endregion

    #region Specs

    public static ObservableCollection<SpecObserver> Specs =
        new(GenerateSpecs().Select(n => new SpecObserver(new Spec(n))));

    #endregion

    #region Runs

    public static RunObserver Run => new(new Run());
    public static RunObserver ExecutedRun = GenerateExecutedRun();

    private static RunObserver GenerateExecutedRun()
    {
        var environment = new Environment();
        environment.Add(new Uri(TestSource));

        var spec = new Spec();
        spec.Find(Element.Module).ShouldHave(Element.Module.Property("Inhibited"), Operation.False);

        var run = new Run(environment);
        run.AddNode(spec.ToNode());

        run.Execute([spec]);

        return new RunObserver(run);
    }

    #endregion

    #region Outcomes

    public static OutcomeObserver DefaultOutcome = new(new Outcome(SpecNode));

    public static IEnumerable<OutcomeObserver> DefaultOutcomes = [DefaultOutcome, DefaultOutcome, DefaultOutcome];

    public static EvaluationObserver PassedEvaluation = new(
        Evaluation.Passed(new Criterion(Element.Tag.Property("DataType"), Operation.EqualTo, "MyType"),
            new Tag("TestTag", "MyType"),
            "MyType")
    );

    public static EvaluationObserver FailedEvaluation = new(
        Evaluation.Failed(new Criterion(Element.Tag.Property("DataType"), Operation.Containing, "Pump"),
            new Tag("TestTag", "MyType"),
            "ValveType")
    );

    public static EvaluationObserver ErroredEvaluation = new(
        Evaluation.Errored(new Criterion(Element.Tag.Property("DataType"), Operation.EqualTo, "MyType"),
            new Tag("TestTag", "MyType"),
            new ArgumentException("Could not execute code due to this throw exception"))
    );

    public static ObservableCollection<EvaluationObserver> Evaluations =
        new(new[] { PassedEvaluation, FailedEvaluation, ErroredEvaluation });

    #endregion


    public static BOOL BoolValue = new(true);
    public static SINT SintValue = new(12);
    public static INT IntValue = new(123);
    public static DINT DintValue = new(123123);
    public static LINT LintValue = new(123123123);

    public static ValueObserver BooleanValueObserver = new(true);
    public static ValueObserver RadixValueObserver = new(Radix.Float);
    public static ValueObserver CollectionValueObserver = new(new List<Argument> { new(), new(), new() });


    public static ChangeLogObserver ChangeLog = new(new ChangeLog
        {
            Command = "SaveSpec",
            Message = "Save specification with nane 'Test spec' with 2 filters and 1 verification.",
            ChangedOn = DateTime.Now,
            ChangedBy = "tnunnink"
        }
    );
}

public class TestPageModel : PageViewModel
{
    public override string Title => "Test Page";
    public override string Icon => "Collections";
}

public class TestDetailPageModel : DetailPageModel
{
    public override string Title => "Details Page";
    public override string Icon => "Container";
}

public class DesignCriterionObserver()
    : CriterionObserver(new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test"));

public class DesignArgumentObserver() : ArgumentObserver(new Argument("Literal Text Value"));

public class DesignRadixProperty() : Property("Radix", typeof(Radix), Element.Tag.This);

public class DesignTextProperty() : Property("TagName", typeof(TagName), Element.Tag.This);

public class DesignMembersProperty() : Property("Members", typeof(IEnumerable<Tag>), Element.Tag.This);

public class DesignRadixPropertyObserver()
    : PropertyObserver(new DesignRadixProperty(), new ElementObserver(new Tag()));

public class DesignVariableObserver() : VariableObserver(new Variable("MyVar", "123"));