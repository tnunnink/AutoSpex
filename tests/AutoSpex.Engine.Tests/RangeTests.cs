namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RangeTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var range = new Range();

        range.Should().NotBeNull();
        range.Min.Should().BeNull();
        range.Max.Should().BeNull();
    }

    [Test]
    public void Min_SetValue_ShouldBeExpected()
    {
        var range = new Range();

        range.Min = 1.23;

        range.Min.Should().Be(1.23);
        range.Min.Should().BeOfType(typeof(double));
    }
    
    [Test]
    public void Max_SetValue_ShouldBeExpected()
    {
        var range = new Range();

        range.Max = 1.23;

        range.Max.Should().Be(1.23);
        range.Max.Should().BeOfType(typeof(double));
    }
    
    [Test]
    public void InRange_SetValue_ShouldBeExpected()
    {
        var range = new Range(1, 10);

        var result = range.InRange(5);

        result.Should().BeTrue();
    }
}