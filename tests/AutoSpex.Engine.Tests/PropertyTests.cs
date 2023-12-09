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
        property.Group.Should().Be(TypeGroup.Text);
        property.IsCollection.Should().BeFalse();
        property.IsLogixElement.Should().BeFalse();
        property.IsLogixType.Should().BeFalse();
    }

    [Test]
    public void SubProperty_ValidProperty_ShouldReturnExpectedProperty()
    {
        var property = new Property("Test", typeof(string));

        var result = property.Nested("Length");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Length");
        result.Type.Should().Be(typeof(int));
    }

    [Test]
    public void Nested_ControllerType_ShouldBeExpected()
    {
        var property = new Property("Test", typeof(Controller));

        var result = property.Nested("Revision.Major");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Major");
        result.Type.Should().Be(typeof(string));
    }
}