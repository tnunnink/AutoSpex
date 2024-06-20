namespace AutoSpex.Engine.Tests.OperationTests;

[TestFixture]
public class OperationSupportingTests
{
    [Test]
    public void Supporting_Boolean_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Boolean).ToList();

        results.Should().HaveCount(4);
        results.Should().Contain(Operation.IsNull);
        results.Should().Contain(Operation.Equal);
        results.Should().Contain(Operation.IsTrue);
        results.Should().Contain(Operation.IsFalse);
    }
    
    [Test]
    public void Supporting_Number_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Number).ToList();

        results.Should().HaveCount(8);
        results.Should().Contain(Operation.IsNull);
        results.Should().Contain(Operation.Equal);
        results.Should().Contain(Operation.In);
        results.Should().Contain(Operation.GreaterThan);
        results.Should().Contain(Operation.GreaterThanOrEqual);
        results.Should().Contain(Operation.LessThan);
        results.Should().Contain(Operation.LessThanOrEqual);
        results.Should().Contain(Operation.Between);
    }
    
    [Test]
    public void Supporting_Text_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Text).ToList();

        results.Should().HaveCount(11);
        results.Should().Contain(Operation.IsNull);
        results.Should().Contain(Operation.Equal);
        results.Should().Contain(Operation.In);
        results.Should().Contain(Operation.StartsWith);
        results.Should().Contain(Operation.EndsWith);
        results.Should().Contain(Operation.Contains);
        results.Should().Contain(Operation.IsEmpty);
        results.Should().Contain(Operation.IsNullOrEmpty);
        results.Should().Contain(Operation.IsNullOrWhiteSpace);
        results.Should().Contain(Operation.IsMatch);
        results.Should().Contain(Operation.Like);
    }
    
    [Test]
    public void Supporting_Date_ShouldBeExpected()
    {
        var results = Operation.Supporting(TypeGroup.Date).ToList();

        results.Should().HaveCount(8);
        results.Should().Contain(Operation.IsNull);
        results.Should().Contain(Operation.Equal);
        results.Should().Contain(Operation.In);
        results.Should().Contain(Operation.GreaterThan);
        results.Should().Contain(Operation.GreaterThanOrEqual);
        results.Should().Contain(Operation.LessThan);
        results.Should().Contain(Operation.LessThanOrEqual);
        results.Should().Contain(Operation.Between);
    }
    
    
}