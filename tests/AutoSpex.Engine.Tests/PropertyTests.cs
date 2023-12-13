namespace AutoSpex.Engine.Tests;

[TestFixture]
public class PropertyTests
{
    [Test]
    public void New_ValidPropertyInfo_ShouldNotBeNull()
    {
        var property = new Property("Test", typeof(string));

        property.Should().NotBeNull();
    }

    [Test]
    public void New_ValidPropertyInfo_ShouldHaveExpectedValues()
    {
        var property = new Property("Test", typeof(string));

        property.Name.Should().Be("Test");
        property.Type.Should().Be(typeof(string));
        property.Properties.Should().BeEmpty();
    }

    [Test]
    public void Properties_ForTagProperty_ShouldBeExpectedCount()
    {
        var property = new Property("Test", typeof(Tag));

        var properties = property.Properties;

        properties.Should().HaveCount(20);
    }
}