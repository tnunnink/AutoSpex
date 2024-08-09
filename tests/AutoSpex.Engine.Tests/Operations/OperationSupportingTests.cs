namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class OperationSupportingTests
{
    [Test]
    public void Supporting_Boolean_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Boolean).ToList();

        results.Should().HaveCount(4);
        results.Should().Contain(Operation.Null);
        results.Should().Contain(Operation.EqualTo);
        results.Should().Contain(Operation.True);
        results.Should().Contain(Operation.False);
    }
    
    [Test]
    public void Supporting_Number_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Number).ToList();

        results.Should().HaveCount(8);
        results.Should().Contain(Operation.Null);
        results.Should().Contain(Operation.EqualTo);
        results.Should().Contain(Operation.In);
        results.Should().Contain(Operation.GreaterThan);
        results.Should().Contain(Operation.GreaterThanOrEqualTo);
        results.Should().Contain(Operation.LessThan);
        results.Should().Contain(Operation.LessThanOrEqualTo);
        results.Should().Contain(Operation.Between);
    }
    
    [Test]
    public void Supporting_Text_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Text).ToList();

        results.Should().HaveCount(11);
        results.Should().Contain(Operation.Null);
        results.Should().Contain(Operation.EqualTo);
        results.Should().Contain(Operation.In);
        results.Should().Contain(Operation.StartingWith);
        results.Should().Contain(Operation.EndingWith);
        results.Should().Contain(Operation.Containing);
        results.Should().Contain(Operation.Empty);
        results.Should().Contain(Operation.NullOrEmpty);
        results.Should().Contain(Operation.NullOrWhiteSpace);
        results.Should().Contain(Operation.Match);
        results.Should().Contain(Operation.Like);
    }
    
    [Test]
    public void Supporting_Date_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Date).ToList();

        results.Should().HaveCount(8);
        results.Should().Contain(Operation.Null);
        results.Should().Contain(Operation.EqualTo);
        results.Should().Contain(Operation.In);
        results.Should().Contain(Operation.GreaterThan);
        results.Should().Contain(Operation.GreaterThanOrEqualTo);
        results.Should().Contain(Operation.LessThan);
        results.Should().Contain(Operation.LessThanOrEqualTo);
        results.Should().Contain(Operation.Between);
    }
    
    
}