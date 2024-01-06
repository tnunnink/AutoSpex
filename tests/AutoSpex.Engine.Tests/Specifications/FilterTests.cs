namespace AutoSpex.Engine.Tests.Specifications;

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
            new("Name", Operation.Contains, "Test"),
            new("DataType", Operation.Equal, "INT"),
            new("Radix", Operation.Equal, Radix.Decimal),
            new("Constant", Operation.Equal, false)
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
            new("Name", Operation.Contains, "Test"),
            new("DataType", Operation.Equal, "INT"),
            new("Radix", Operation.Equal, Radix.Decimal),
            new("Constant", Operation.Equal, false)
        };

        var filter = Filter.Any(criteria);

        var results = tags.Where(filter.Compile()).Cast<Tag>().ToList();
        results.Should().HaveCount(3);
    }
}