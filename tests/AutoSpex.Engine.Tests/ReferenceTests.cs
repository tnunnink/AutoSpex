namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ReferenceTests
{
    [Test]
    public void New_EmptyString_ShouldNotBeNull()
    {
        var reference = new Reference(string.Empty);

        reference.Should().NotBeNull();
    }

    [Test]
    public void Name_ValidString_ShouldBeExpected()
    {
        var reference = new Reference("TestName");

        reference.Name.Should().Be("TestName");
    }

    [Test]
    public void ToString_ValidString_ShouldBeExpected()
    {
        var reference = new Reference("TestName");

        reference.ToString().Should().Be("@TestName");
    }
}