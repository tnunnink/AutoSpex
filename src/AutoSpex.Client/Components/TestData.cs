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
    
    #region Reops

    private const string TestRepo = @"C:\Users\tnunnink\Documents\Rockwell";

    public static RepoObserver RepoTest = new(Repo.Configure(TestRepo));

    public static ObservableCollection<RepoObserver> Repos = [RepoTest, RepoTest, RepoTest];

    #endregion
    
    #region Sources

    private const string TestSource = @"C:\Users\tnunnink\Documents\Rockwell\Test.L5X";

    public static SourceObserver SourceTest = new(Source.Create(TestSource));

    public static ObservableCollection<SourceObserver> Sources = [SourceTest, SourceTest, SourceTest];

    #endregion

    #region Elements

    public static TagName TagNameValue = new("MyTag.Member[1].Value");

    #endregion

    #region Properties

    public static Property RadixProperty = Element.Tag.This.GetProperty("Radix");
    public static Property TagNameProperty = Element.Tag.This.GetProperty("TagName");
    public static Property MembersProperty = Element.Tag.This.GetProperty("Members");

    public static PropertyInput RadixPropertyInput = new(() => "Radix", input: () => Element.Tag.This);
    public static PropertyInput TagNamePropertyInput = new(() => "TagName", input: () => Element.Tag.This);
    public static PropertyInput MembersPropertyInput = new(() => "Members", input: () => Element.Tag.This);

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
        collection.AddSpec("Test Spec", s => s.Query(Element.Tag).Where("TagName", Operation.Containing, "Test"));
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
            c.Query(Element.Tag);
            c.Where("TagName", Operation.Containing, "TestTag");
            c.Verify("Value", Operation.EqualTo, 123);
        })
    );

    public static SpecObserver SpecObserverManyCriterion = new(Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Where("TagName", Operation.Containing, "TestTag");
            c.Where("DataType", Operation.EqualTo, "MyType");
            c.Where("ExternalAccess", Operation.In,
                new List<object> { ExternalAccess.ReadOnly, ExternalAccess.ReadWrite });
            c.Verify("Value", Operation.GreaterThan, 123);
            c.Verify("Description", Operation.EndingWith, "Some text value");
            c.Verify("Scope.Program", Negation.Not, Operation.EqualTo, "MyContianer");
        })
    );

    #endregion

    #region Query

    public static FilterObserver FilterObserver = new(new Filter("TagName", Operation.Containing, "TestTag"));
    public static SelectObserver SelectObserver = new(new Select("TagName.Operand"));
    public static CountObserver CountObserver = new(new Count(new Criterion("TagName", Operation.Containing, "TestTag")));
    public static VerifyObserver VerifyObserver = new(new Verify("TagName", Operation.Containing, "TestTag"));

    public static QueryObserver DefaultQueryObserver = new(new Query());

    public static QueryObserver QueryObserver = new(
        new Query(
            Element.Tag,
            [new Filter("TagName", Operation.Containing, "TestTag"), new Select("TagName.Operand")]
        )
    );

    #endregion

    #region Criterion

    public static readonly CriterionObserver EmptyCriterion = new(new Criterion(), () => Property.Default);

    public static readonly CriterionObserver BoolCriterion =
        new(new Criterion("Constant", Operation.EqualTo, true), () => Element.Tag.This);

    public static readonly CriterionObserver NumberCriterion =
        new(new Criterion("Dimensions", Operation.GreaterThanOrEqualTo, 10), () => Element.Tag.This);

    public static readonly CriterionObserver TextCriterion =
        new(new Criterion("Name", Operation.EqualTo, "Test"), () => Element.Tag.This);

    public static readonly CriterionObserver EnumCriterion =
        new(new Criterion("Radix", Operation.EqualTo, Radix.Binary), () => Element.Tag.This);

    public static readonly CriterionObserver InnerCriterion =
        new(new Criterion("Members", Operation.Any, new Criterion("TagName", Operation.Like, "%MemberName")),
            () => Element.Tag.This);

    public static readonly CriterionObserver RangeCriterion =
        new(new Criterion("Value", Operation.Between, new Range(1, 12)), () => Element.Tag.This);

    public static readonly CriterionObserver InCriterion = new(
        new Criterion("TagName", Operation.In, new List<object> { "First", "Second", "Third Or Longer" }),
        () => Element.Tag.This
    );

    public static readonly ObservableCollection<CriterionObserver> Criteria =
    [
        EmptyCriterion,
        BoolCriterion,
        NumberCriterion,
        TextCriterion,
        EnumCriterion,
        InnerCriterion,
        RangeCriterion,
        InCriterion
    ];

    #endregion

    #region ArgumentInput

    public static readonly ArgumentInput EmptyArgument = EmptyCriterion.Argument;
    public static readonly ArgumentInput BoolArgument = BoolCriterion.Argument;
    public static readonly ArgumentInput NumberArgument = NumberCriterion.Argument;
    public static readonly ArgumentInput EnumArgument = EnumCriterion.Argument;
    public static readonly ArgumentInput TextArgument = TextCriterion.Argument;
    public static readonly ArgumentInput CollectionArgument = InCriterion.Argument;

    #endregion

    #region Selection

    public static SelectionObserver DefaultSelect =
        new(new Selection(), () => Element.Tag.This);

    public static SelectionObserver SimpleSelect =
        new(new Selection("TagName"), () => Element.Tag.This);

    public static SelectionObserver AliasSelect =
        new(new Selection("TagName.Member", "TagName"), () => Element.Tag.This);

    #endregion

    #region Values

    public static ValueObserver NullValue = new(null);
    public static ValueObserver BooleanTrueValue = new(true);
    public static ValueObserver BooleanFalseValue = new(false);
    public static ValueObserver IntegerValue = new(34567);
    public static ValueObserver DoubleValue = new(1.234);
    public static ValueObserver TextValue = new("SomeTestValue");

    public static ValueObserver TextOverlowValue =
        new(
            "SomeTestValue SomeTestValue SomeTestValue SomeTestValue SomeTestValue SomeTestValue SomeTestValue SomeTestValue");

    public static ValueObserver DateValue = new(DateTime.Now);

    public static ValueObserver AtomicBoolValue = new(new BOOL(true));
    public static ValueObserver AtomicSintValue = new(new SINT(12));
    public static ValueObserver AtomicIntValue = new(new INT(123));
    public static ValueObserver AtomicDintValue = new(new DINT(123123));
    public static ValueObserver AtomicLintValue = new(new LINT(123123123));

    public static ValueObserver RadixValue = new(Radix.Float);
    public static ValueObserver DataTypeValue = new(new DataType("TestType"));
    public static ValueObserver RungValue = new(new Rung("XIC(Testing)"));
    public static ValueObserver TagValue = new(new Tag("TestTag", new DINT()));

    public static ValueObserver ReferenceThisValue = new(new Reference("$this"));
    public static ValueObserver ReferenceRequiredValue = new(new Reference("$required"));

    public static ValueObserver NumberCollectionValue = new(
        new ObserverCollection<object?, ValueObserver>([1, 2, 3], x => new ValueObserver(x))
    );

    #endregion

    #region Range

    public static RangeObserver RangeValue = new(new Range(1, 12));

    #endregion

    #region Verifications

    public static ResultObserver DefaultResult = new(Node.NewSpec());

    public static ResultObserver PassedResult = new(Node.NewSpec());

    public static ResultObserver FailedResult = new(Node.NewSpec());

    public static ResultObserver ErroredResult = new(Node.NewSpec());

    public static ResultObserver NestedResult = new(Node.NewCollection());

    public static ObservableCollection<ResultObserver> Results =
        [NestedResult, PassedResult, FailedResult, ErroredResult];

    #endregion

    #region Evaluations

    public static EvaluationObserver PassedEvaluation = new(
        Evaluation.Passed(
            new Criterion("DataType", Operation.EqualTo, "DINT"),
            new Tag("TestTag", new DINT()),
            "DINT")
    );

    public static EvaluationObserver FailedEvaluation = new(
        Evaluation.Failed(new Criterion("DataType", Operation.Containing, "Pump"),
            new Tag("TestTag", new DINT()),
            "DINT")
    );

    public static EvaluationObserver ErroredEvaluation = new(
        Evaluation.Errored(
            new Criterion("DataType", Operation.EqualTo, "DINT"),
            new Tag("TestTag", new DINT()),
            new ArgumentException("Could not execute code due to this throw exception"))
    );

    public static ObservableCollection<EvaluationObserver> Evaluations =
        new(new[] { PassedEvaluation, FailedEvaluation, ErroredEvaluation });

    #endregion
}