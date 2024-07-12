namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RangeTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var range = new Range();

        range.Enabled.Should().BeFalse();
        range.Criterion.Should().NotBeNull();
        range.Criterion.Property.Origin.Should().Be(typeof(List<LogixElement>));
        range.Criterion.Property.Name.Should().Be("Count");
        range.Criterion.Property.Type.Should().Be(typeof(int));
        range.Criterion.Operation.Should().Be(Operation.GreaterThan);
        range.Criterion.Arguments.Should().HaveCount(1);
        range.Criterion.Arguments[0].Value.Should().Be(0);
    }

    [Test]
    public void New_Configured_ShouldBeExpected()
    {
        var range = new Range();

        range.Enabled = true;
        range.Criterion.Operation = Operation.LessThan;
        range.Criterion.Arguments[0].Value = 10;

        range.Enabled.Should().BeTrue();
        range.Criterion.Operation.Should().Be(Operation.LessThan);
        range.Criterion.Arguments[0].Value.Should().Be(10);
    }
}