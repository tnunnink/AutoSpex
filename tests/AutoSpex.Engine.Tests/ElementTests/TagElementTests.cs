namespace AutoSpex.Engine.Tests.ElementTests;

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
    public void This_WhenCalled_ShouldNotBeExpectedType()
    {
        var element = Element.Tag;

        var result = element.This;

        result.Origin.Should().Be(typeof(Tag));
        result.Type.Should().Be(typeof(Tag));
        result.Name.Should().Be("This");
        result.Path.Should().Be("This");
        result.Group.Should().Be(TypeGroup.Element);
        result.Identifier.Should().Be("Tag");
        result.Options.Should().BeEmpty();
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

        var property = element.Property("Name");
        
        property?.Origin.Should().Be(typeof(Tag));
        property?.Type.Should().Be(typeof(string));
        property?.Name.Should().Be("Name");
        property?.Path.Should().Be("Name");
        property?.Group.Should().Be(TypeGroup.Text);
        property?.Identifier.Should().Be("string");
        property?.Options.Should().BeEmpty();
        property?.Properties.Should().NotBeEmpty();
    }
    
    [Test]
    public void Property_NestedEnumProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.Property("Radix.Name.Length");
        
        property?.Origin.Should().Be(typeof(Tag));
        property?.Type.Should().Be(typeof(int));
        property?.Name.Should().Be("Length");
        property?.Path.Should().Be("Radix.Name.Length");
        property?.Group.Should().Be(TypeGroup.Number);
        property?.Identifier.Should().Be("int");
        property?.Options.Should().BeEmpty();
        property?.Properties.Should().BeEmpty();
    }
    
    [Test]
    public void Property_NestedElementProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.Property("Root.Root.Parent");
        
        property?.Origin.Should().Be(typeof(Tag));
        property?.Type.Should().Be(typeof(Tag));
        property?.Name.Should().Be("Parent");
        property?.Path.Should().Be("Root.Root.Parent");
        property?.Group.Should().Be(TypeGroup.Element);
        property?.Identifier.Should().Be("Tag");
        property?.Options.Should().BeEmpty();
        property?.Properties.Should().NotBeEmpty();
    }
}