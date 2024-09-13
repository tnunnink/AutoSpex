using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using JetBrains.Annotations;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;

namespace AutoSpex.Client.Components;

[PublicAPI]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class TestData
{
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

    public static SpecObserver SpecObserver = new(Spec.Configure(c =>
        {
            c.Find(Element.Tag);
            c.Filter("TagName", Operation.Containing, "TestTag");
            c.Verify("Value", Operation.EqualTo, 123);
        })
    );

    /*public static ObservableCollection<SpecObserver> Specs =
        new(GenerateSpecs().SelectMany(n => n.Specs.Select(s => new SpecObserver(s))));*/

    #endregion

    #region Criterion

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

    public static readonly ObservableCollection<CriterionObserver> Criteria =
    [
        BoolCriterion,
        NumberCriterion,
        TextCriterion,
        EnumCriterion,
        InnerCriterion
    ];

    #endregion

    #region Variables

    public static Variable Variable = new("MyVar", "This is a test");

    public static VariableObserver VariableObserver = new(Variable);

    public static ObservableCollection<VariableObserver> Variables =
    [
        new VariableObserver(new Variable("flag", true)),
        new VariableObserver(new Variable("numeric", 123)),
    ];

    #endregion

    #region Values

    public static ValueObserver NullValue = new(null);
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

    public static ValueObserver CollectionValue = new(new List<Argument> { new(), new(), new() });
    public static ValueObserver VariableValue = new(Variable);
    public static ValueObserver ReferenceValue = new(new Reference("test_ref"));

    #endregion

    #region Outcomes

    public static OutcomeObserver DefaultOutcome = new(new Outcome(Node.NewSpec()));

    public static IEnumerable<OutcomeObserver> DefaultOutcomes = [DefaultOutcome, DefaultOutcome, DefaultOutcome];

    #endregion

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
}