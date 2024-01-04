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
        var controller = new Controller {Name = "TestController"};

        var filter = Filter.By(new Criterion("Name", Operation.StartsWith, "Test"));

        var result = filter.Compile()(controller);

        result.Should().BeTrue();
    }

    [Test]
    public void All_CollectionOfCriteria_ShouldProduceExpectedFilteredResult()
    {
        var tags = new List<Tag>
        {
            new() {Name = "TestTag", Value = new INT(100), Constant = false},
            new() {Name = "AnotherTag", Value = true, Constant = true},
            new() {Name = "ThirdTestTag", Value = 1.23, Constant = false},
        };

        var criteria = new List<Criterion>
        {
            Element.Has("Name", Operation.Contains, "Test"),
            Element.Has("DataType", Operation.Equal, "INT"),
            Element.Has("Radix", Operation.Equal, Radix.Decimal),
            Element.Has("Constant", Operation.Equal, false)
        };

        var filter = Filter.All(criteria);

        var results = tags.Where(filter.Compile()).Cast<Tag>().ToList();
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("TestTag");
    }

    [Test]
    public void Any_CollectionOfCriteria_ShouldProduceExpectedFilteredResult()
    {
        var tags = new List<Tag>
        {
            new() {Name = "TestTag", Value = new INT(100), Constant = false},
            new() {Name = "AnotherTag", Value = true, Constant = true},
            new() {Name = "ThirdTestTag", Value = 1.23, Constant = false},
        };

        var criteria = new List<Criterion>
        {
            Element.Has("Name", Operation.Contains, "Test"),
            Element.Has("DataType", Operation.Equal, "INT"),
            Element.Has("Radix", Operation.Equal, Radix.Decimal),
            Element.Has("Constant", Operation.Equal, false)
        };

        var filter = Filter.Any(criteria);

        var results = tags.Where(filter.Compile()).Cast<Tag>().ToList();
        results.Should().HaveCount(3);
    }

    [Test]
    public void GroupTesting()
    {
        var tag = new Tag {Name = "Test", Value = new DINT(123), Description = "This is a tag for testing purposes "};

        var first = Filter
            .By(Element.Has("Name", Operation.Contains, "Test"))
            .Chain(Element.Has("DataType", Operation.Equal, "INT"), ChainType.Or);

        var second = Filter
            .By(Element.Has("Value", Operation.Equal, "123"))
            .Chain(Element.Has("Description", Operation.Contains, "Tag"), ChainType.And);

        var filter = first.Or(second).Compile();

        var result = filter(tag);
        result.Should().BeTrue();
    }
}