using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;
using L5Sharp.Core;
using Range = AutoSpex.Engine.Range;

namespace AutoSpex.Client.Components;

[PublicAPI]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class TestData
{
    #region Sources

    private const string TestSource = @"C:\Users\tnunn\Documents\L5X\Test.L5X";

    public static SourceObserver SourceTest = new(new Source(L5X.Load(TestSource)));

    public static SourceObserver SourceEmpty = new(new Source());

    public static ObservableCollection<SourceObserver> Sources = [SourceTest, SourceTest, SourceEmpty];

    #endregion

    #region Elements

    public static ObservableCollection<ElementObserver> DataTypeElements = new(GetDataTypes());

    private static IEnumerable<ElementObserver> GetDataTypes()
    {
        /*var content = L5X.New("Test", "1756-L83E", new Revision(33.1));

        content.Add(new DataType("MyTestType") { Description = "this is a test element" });
        content.Add(new DataType("AnotherType") { Description = "this is a test element" });
        content.Add(new DataType("MyComplexType") { Description = "this is a test element" });

        return content.DataTypes.Select(e => new ElementObserver(e));*/

        yield return new DataType("MyTestType") { Description = "this is a test element" };
        yield return new DataType("AnotherType") { Description = "this is a test element" };
        yield return new DataType("MyComplexType") { Description = "this is a test element" };
    }

    #endregion

    #region Properties

    public static PropertyObserver RadixPropertyObserver = new(
        Element.Tag.This.GetProperty("Radix"),
        new ElementObserver(new Tag { Name = "MyTag" })
    );

    public static PropertyObserver TagNamePropertyObserver => new(
        Element.Tag.This.GetProperty("TagName"),
        new ElementObserver(new Tag { Name = "MyTag" })
    );

    public static PropertyObserver MembersPropertyObserver => new(
        Element.Tag.This.GetProperty("Members"),
        new ElementObserver(new Tag { Name = "MyTag", Value = new TIMER() })
    );

    #endregion

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
        collection.AddSpec("Test Spec", s => s.Fetch(Element.Tag).Where("TagName", Operation.Containing, "Test"));
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

    public static SpecObserver SpecObserver = new(Spec.Configure(c =>
        {
            c.Fetch(Element.Tag);
            c.Where("TagName", Operation.Containing, "TestTag");
            c.Confirm("Value", Operation.EqualTo, 123);
        })
    );

    public static SpecObserver SpecObserverManyCriterion = new(Spec.Configure(c =>
        {
            c.Fetch(Element.Tag);
            c.Where("TagName", Operation.Containing, "TestTag");
            c.Where("DataType", Operation.EqualTo, "MyType");
            c.Where("ExternalAccess", Operation.In,
                new List<object> { ExternalAccess.ReadOnly, ExternalAccess.ReadWrite });
            c.Confirm("Value", Operation.GreaterThan, 123);
            c.Confirm("Description", Operation.EndingWith, "Some text value");
            c.Confirm("Scope.Program", Negation.Not, Operation.EqualTo, "MyContianer");
        })
    );

    #endregion

    #region Criterion

    public static readonly CriterionObserver EmptyCriterion = new Criterion();

    public static readonly CriterionObserver BoolCriterion =
        new(new Criterion(Element.Tag.Property("Constant"), Operation.EqualTo, true));

    public static readonly CriterionObserver NumberCriterion =
        new(new Criterion(Element.Tag.Property("Dimensions"), Operation.GreaterThanOrEqualTo, 10));

    public static readonly CriterionObserver TextCriterion =
        new(new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test"));

    public static readonly CriterionObserver EnumCriterion =
        new(new Criterion(Element.Tag.Property("Radix"), Operation.EqualTo, Radix.Binary));

    public static readonly CriterionObserver InnerCriterion =
        new(new Criterion(Element.Tag.Property("Members"), Operation.Any,
            new Criterion(Element.Tag.Property("TagName"), Operation.Like, "%MemberName")));

    public static readonly CriterionObserver TernaryCriterion =
        new(new Criterion(Element.Tag.Property("Value"), Operation.Between, new Range(1, 12)));

    public static readonly CriterionObserver InCriterion = new(
        new Criterion(
            Element.Tag.Property("TagName"),
            Operation.In,
            new List<string> { "First", "Second", "Third Or Longer" }
        )
    );

    public static readonly ObservableCollection<CriterionObserver> Criteria =
    [
        EmptyCriterion,
        BoolCriterion,
        NumberCriterion,
        TextCriterion,
        EnumCriterion,
        InnerCriterion,
        TernaryCriterion,
        InCriterion
    ];

    #endregion

    #region Values

    public static ValueObserver NullValue = new(() => null);
    public static ValueObserver BooleanTrueValue = new(true);
    public static ValueObserver BooleanFalseValue = new(false);
    public static ValueObserver IntegerValue = new(34567);
    public static ValueObserver DoubleValue = new(1.234);
    public static ValueObserver TextValue = new("SomeTestValue");

    public static ValueObserver AtomicBoolValue = new(new BOOL(true));
    public static ValueObserver AtomicSintValue = new(new SINT(12));
    public static ValueObserver AtomicIntValue = new(new INT(123));
    public static ValueObserver AtomicDintValue = new(new DINT(123123));
    public static ValueObserver AtomicLintValue = new(new LINT(123123123));

    public static ValueObserver RadixValue = new(Radix.Float);
    public static ValueObserver DataTypeValue = new(new DataType("TestType"));
    public static ValueObserver RungValue = new(new Rung("XIC(Testing)"));
    public static ValueObserver TagValue = new(new Tag("TestTag", new DINT()));

    public static ValueObserver NumberCollectionValue = new(
        new ObserverCollection<object?, ValueObserver>([1, 2, 3], x => new ValueObserver(x))
    );

    #endregion

    #region Range

    public static RangeObserver RangeValue = new(new Range(1, 12));

    #endregion

    #region Runs

    public static RunObserver Run => new(new Run(Node.NewCollection(), new Source()));

    public static ObservableCollection<RunObserver> Runs = [Run, Run, Run];

    #endregion

    #region Outcomes

    public static OutcomeObserver DefaultOutcome = new(new Outcome { Name = "Default Outcome" });

    public static IEnumerable<OutcomeObserver> DefaultOutcomes = [DefaultOutcome, DefaultOutcome, DefaultOutcome];

    #endregion

    #region Evaluations

    public static EvaluationObserver PassedEvaluation = new(
        Evaluation.Passed(
            new Criterion(Element.Tag.Property("DataType"), Operation.EqualTo, "DINT"),
            new Tag("TestTag", new DINT()),
            "DINT")
    );

    public static EvaluationObserver FailedEvaluation = new(
        Evaluation.Failed(new Criterion(Element.Tag.Property("DataType"), Operation.Containing, "Pump"),
            new Tag("TestTag", new DINT()),
            "DINT")
    );

    public static EvaluationObserver ErroredEvaluation = new(
        Evaluation.Errored(
            new Criterion(Element.Tag.Property("DataType"), Operation.EqualTo, "DINT"),
            new Tag("TestTag", new DINT()),
            new ArgumentException("Could not execute code due to this throw exception"))
    );

    public static ObservableCollection<EvaluationObserver> Evaluations =
        new(new[] { PassedEvaluation, FailedEvaluation, ErroredEvaluation });

    #endregion

    #region Suppressions

    public static SuppressionObserver Suppression =
        new(new Suppression(Guid.NewGuid(), "This is the reason why this spec is being suppressed."));

    public static ObservableCollection<SuppressionObserver> Suppresions = [Suppression, Suppression, Suppression];

    #endregion

    #region Overrides

    public static ObservableCollection<OverrideObserver> Overrides =
        new(GenerateSpecs().Select(n => new OverrideObserver(n)));

    #endregion
}