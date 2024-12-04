using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Components;

[PublicAPI]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class DesignData
{
    private const string TestSource = @"C:\Users\tnunn\Documents\L5X\Test.L5X";

    #region Sources

    public static SourceObserver SourceTest = new(new Source(L5X.Load(TestSource)));

    public static SourceObserver SourceEmpty = new(new Source());

    public static ObservableCollection<SourceObserver> Sources = [SourceTest, SourceTest, SourceEmpty];

    #endregion

    #region LogixElements

    public static LogixComponent DataType = SourceTest.Model.Content.DataTypes.First();

    public static ObservableCollection<LogixElement> DataTypes = new(SourceTest.Model.Content.DataTypes);

    public static ObservableCollection<ElementObserver> DataTypeElements =
        new(SourceTest.Model.Content.DataTypes.Select(t => new ElementObserver(t)));

    public static LogixElement? Tag = SourceTest.Model.Content.Tags.Find("TestSimpleTag");

    public static ObservableCollection<ElementObserver> Tags = new(SourceTest.Model.Content.Query<Tag>()
        .Where(t => t.Description is not null).Select(x => new ElementObserver(x)));

    public static ElementObserver TagObserver = new(Tag!);

    public static LogixCode Rung = DesignRung();

    private static LogixCode DesignRung()
    {
        var rung = SourceTest.Model.Content?.Query<Rung>().First();
        rung.Text = "XIC(Some_Tag_Name)[GRT(MyTag,1)NEQ(AnotherTag.Member,0)]MOV(0,OutputTag);";
        rung.Comment = "This is a test rung that we are using to mock the look of the UI.";
        return rung;
    }

    public static ObservableCollection<ElementObserver> Rungs =
        new(SourceTest.Model.Content.Query<Rung>().Select(x => new ElementObserver(x)));

    public static LogixElement? Module = SourceTest.Model.Content.Modules.Find("RackIO");

    public static LogixElement? Program = SourceTest.Model.Content.Programs.Find("MainProgram");

    #endregion
}