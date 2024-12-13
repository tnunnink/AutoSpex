using System.Dynamic;

namespace AutoSpex.Engine.Tests.Elements;

[TestFixture]
public class DynamicElementTests
{
    [Test]
    public void New_NoInstance_ShouldBeExpected()
    {
        var element = Element.Dynamic();

        element.Name.Should().Be("Dynamic");
        element.Value.Should().Be("System.Dynamic.ExpandoObject");
        element.Type.Should().Be(typeof(ExpandoObject));
        element.IsComponent.Should().BeFalse();
        element.Properties.Should().BeEmpty();
    }

    [Test]
    public void New_WithProperties_ShouldHaveExpectedProperties()
    {
        var element = Element.Dynamic(["Test", "Another", "Complex"]);

        element.Properties.Should().HaveCount(3);
    }

    [Test]
    public void This_ForEmptyObject_ShouldBeExpected()
    {
        var element = Element.Dynamic();

        var property = element.This;

        property.Should().NotBeNull();
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(ExpandoObject));
        property.Properties.Should().BeEmpty();
    }

    [Test]
    public void This_WithProperties_ShouldBeExpected()
    {
        var element = Element.Dynamic(["First", "Second"]);

        var property = element.This;

        property.Should().NotBeNull();
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(ExpandoObject));
        property.Properties.Should().HaveCount(2);
    }
}