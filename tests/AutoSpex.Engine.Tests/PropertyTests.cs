namespace AutoSpex.Engine.Tests;

[TestFixture]
public class PropertyTests
{
    [Test]
    public void New_ValidPropertyInfo_ShouldNotBeNull()
    {
        var property = new Property(typeof(Tag), "Name", typeof(string));

        property.Should().NotBeNull();
    }

    [Test]
    public void New_ValidPropertyInfo_ShouldHaveExpectedValues()
    {
        var property = new Property(typeof(Tag), "Test", typeof(string));

        property.Origin.Should().Be(typeof(Tag));
        property.Path.Should().Be("Test");
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Test");
        property.Properties.Should().NotBeEmpty();
        property.Identifier.Should().Be("string");
        property.Group.Should().Be(TypeGroup.Text);
        property.Options.Should().BeEmpty();
    }

    [Test]
    public void Properties_ForTagProperty_ShouldBeExpectedCount()
    {
        var property = new Property(typeof(Tag), "Test", typeof(Tag));

        var properties = property.Properties;

        properties.Should().HaveCount(23);
    }
}