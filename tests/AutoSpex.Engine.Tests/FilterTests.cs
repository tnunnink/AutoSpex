using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using AutoSpex.Engine.Operations;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class FilterTests
{
    [Test]
    public void All_WhenCalled_ShouldAlwaysReturnTrue()
    {
        var filter = Filter.All();

        var result = filter.Compile()(false);

        result.Should().BeTrue();
    }

    [Test]
    public void None_WhenCalled_ShouldAlwaysReturnFalse()
    {
        var filter = Filter.None();

        var result = filter.Compile()(true);

        result.Should().BeFalse();
    }

    [Test]
    public void On_ValidCriterion_ShouldReturnExpectedResult()
    {
        var filter = Filter.By(new Criterion(Element.Controller, "Name", Operation.StartsWith, "Test"));
        var controller = new Controller {Name = "TestController"};

        var result = filter.Compile()(controller);

        result.Should().BeTrue();
    }

    [Test]
    public void GroupTesting()
    {
        var tag = new Tag {Name = "Test", Value = new DINT(123), Description = "This is a tag for testing purposes "};

        var first = Filter
            .By(Element.Tag.Has("Name", Operation.Contains, "Test"))
            .Or(Element.Tag.Has("DataType", Operation.EqualTo, "INT"));

        var second = Filter
            .By(Element.Tag.Has("Value", Operation.EqualTo, "123"))
            .And(Element.Tag.Has("Description", Operation.Contains, "Tag"));

        var filter = first.Or(second).Compile();

        var result = filter(tag);
        result.Should().BeTrue();
    }

    [Test]
    public void Groutests()
    {
        var tag = new Tag {Name = "Test", Value = new DINT(123)};

        Expression<Func<object, bool>> first = o => ((Tag) o).Name.StartsWith("Test");
        Expression<Func<object, bool>> second = o => ((Tag) o).DataType.Equals("DINT");
        Expression<Func<object, bool>> third = o => ((Tag) o).Value.Equals("321");
        Expression<Func<object, bool>> fourth = o => ((Tag) o).Description!.Contains("Tag");


        var filter = first.And(second).Or(third).And(fourth);

        var text = filter.ToReadableString();
        text.Should().NotBeEmpty();

        var result = filter.Compile()(tag);
        result.Should().BeTrue();
    }
}