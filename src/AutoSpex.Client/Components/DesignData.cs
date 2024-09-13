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
using Argument = AutoSpex.Engine.Argument;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Client.Components;

[PublicAPI]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class DesignData
{
    private const string TestSource = @"C:\Users\tnunn\Documents\L5X\Test.L5X";

    #region Pages

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

    #endregion

    #region Environments

    public static EnvironmentObserver EnvironmentItemDefault = new(new Environment());

    public static EnvironmentObserver EnvironmentItemTest = new(new Environment
    {
        Name = "Some Test Config",
        Comment = "This config is for testing the interface.",
        IsTarget = false
    });

    public static ObservableCollection<EnvironmentObserver>
        Environments = [EnvironmentItemDefault, EnvironmentItemTest];

    #endregion

    #region Arguments

    public static ArgumentObserver EmptyArgument = new(new Argument(string.Empty));

    public static ArgumentObserver TextArgument = new(new Argument("Literal Text Value"));

    public static ArgumentObserver EnumArgument = new(new Argument(ExternalAccess.ReadOnly));

    public static ArgumentObserver VariableArgument = new(new Argument(new Variable("Var01", "Some Value")));

    public static ObservableCollection<ArgumentObserver> Arguments =
    [
        EmptyArgument,
        TextArgument,
        EnumArgument,
        VariableArgument
    ];

    public static ObservableCollection<Argument> RadixOptions =>
        new(LogixEnum.Options<Radix>().Select(e => new Argument(e)));

    #endregion

    #region Sources

    public static SourceObserver SourceTest = new(new Source(new Uri(TestSource)));

    public static SourceObserver SourceFake = new(new Source(new Uri(TestSource)));

    public static ObservableCollection<SourceObserver> Sources = [SourceTest, SourceTest, SourceFake];

    #endregion

    #region LogixElements

    public static LogixComponent DataType = SourceTest.Model.Load().DataTypes.First();

    public static ObservableCollection<LogixElement> DataTypes = new(SourceTest.Model.Load().DataTypes);

    public static ObservableCollection<ElementObserver> DataTypeElements =
        new(SourceTest.Model.Load().DataTypes.Select(t => new ElementObserver(t)));

    public static LogixElement? Tag = SourceTest.Model.Load().Tags.Find("TestSimpleTag");

    public static ObservableCollection<ElementObserver> Tags = new(SourceTest.Model.Load().Query<Tag>()
        .Where(t => t.Description is not null).Select(x => new ElementObserver(x)));

    public static ElementObserver TagObserver = new(Tag!);

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

    public static LogixElement? Module = SourceTest.Model.Load().Modules.Find("RackIO");

    public static LogixElement? Program = SourceTest.Model.Load().Programs.Find("MainProgram");

    #endregion

    #region Properties

    public static PropertyObserver RadixPropertyObserver = new(
        Element.Tag.This.GetProperty("Radix")!,
        new ElementObserver(new Tag { Name = "MyTag" })
    );

    public static PropertyObserver TagNamePropertyObserver => new(
        Element.Tag.This.GetProperty("TagName")!,
        new ElementObserver(new Tag { Name = "MyTag" })
    );

    public static PropertyObserver MembersPropertyObserver => new(
        Element.Tag.This.GetProperty("Members")!,
        new ElementObserver(new Tag { Name = "MyTag", Value = new TIMER() })
    );

    #endregion


    #region Runs

    public static RunObserver Run => new(new Run());

    #endregion

    /*public static EvaluationObserver PassedEvaluation = new(
        Evaluation.Passed(new Criterion(Element.Tag.Property("DataType"), Operation.EqualTo, "MyType"),
            new Tag("TestTag", "MyType"), "MyType")
    );*/

    /*public static EvaluationObserver FailedEvaluation = new(
        Evaluation.Failed(new Criterion(Element.Tag.Property("DataType"), Operation.Containing, "Pump"),
            new Tag("TestTag", "MyType"),
            "ValveType")
    );*/

    public static EvaluationObserver ErroredEvaluation = new(
        Evaluation.Errored(new Criterion(Element.Tag.Property("DataType"), Operation.EqualTo, "MyType"),
            new Tag("TestTag", "MyType"),
            new ArgumentException("Could not execute code due to this throw exception"))
    );
    
    /*public static ObservableCollection<EvaluationObserver> Evaluations =
        new(new[] { PassedEvaluation, FailedEvaluation, ErroredEvaluation });*/
    
}

public class TestPageModel : PageViewModel
{
    public TestPageModel()
    {
        Title = "Test Page";
    }

    public override string Icon => "Collections";
}

public class TestDetailPageModel() : DetailPageModel("Details Page")
{
    public override string Icon => "Container";
}