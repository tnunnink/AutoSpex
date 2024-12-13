namespace AutoSpex.Engine.Tests.Elements;

[TestFixture]
public class TagElementTests
{
    [Test]
    public void Properties_WhenCalled_ShouldNotBeEmpty()
    {
        var element = Element.Tag;

        var properties = element.Properties.ToList();

        properties.Should().NotBeEmpty();
    }

    [Test]
    public void This_WhenCalled_ShouldNotBeNull()
    {
        var element = Element.Tag;

        var result = element.This;

        result.Should().NotBeNull();
    }

    [Test]
    public void This_WhenCalled_ShouldExpectedValues()
    {
        var element = Element.Tag;

        var result = element.This;

        result.Origin.Should().Be(typeof(Tag));
        result.Type.Should().Be(typeof(Tag));
        result.Name.Should().Be("This");
        result.Path.Should().Be("This");
        result.Group.Should().Be(TypeGroup.Element);
        result.DisplayName.Should().Be("Tag");
        result.Properties.Should().NotBeEmpty();
    }

    [Test]
    public void IsComponent_WhenCalled_ShouldBeTrue()
    {
        var element = Element.Tag;

        var result = element.IsComponent;

        result.Should().BeTrue();
    }

    [Test]
    public void Property_SimpleProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.GetProperty("Name");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Name");
        property.Path.Should().Be("Name");
        property.Group.Should().Be(TypeGroup.Text);
        property.DisplayName.Should().Be("string");
        property.Properties.Should().BeEmpty();
    }

    [Test]
    public void Property_NestedEnumProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.GetProperty("Radix.Name");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Name");
        property.Path.Should().Be("Radix.Name");
        property.Group.Should().Be(TypeGroup.Text);
        property.DisplayName.Should().Be("string");
        property.Properties.Should().BeEmpty();
    }

    [Test]
    public void Property_NestedElementProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.GetProperty("Root.Root.Parent");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(Tag));
        property.Name.Should().Be("Parent");
        property.Path.Should().Be("Root.Root.Parent");
        property.Group.Should().Be(TypeGroup.Element);
        property.DisplayName.Should().Be("Tag");
        property.Properties.Should().NotBeEmpty();
    }

    [Test]
    public void Property_References_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.GetProperty("References");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(List<CrossReference>));
        property.Name.Should().Be("References");
        property.Path.Should().Be("References");
        property.Group.Should().Be(TypeGroup.Collection);
        property.DisplayName.Should().Be("CrossReference[]");
    }
}