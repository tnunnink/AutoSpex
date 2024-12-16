using System.Dynamic;

namespace AutoSpex.Engine.Tests.Elements;

[TestFixture]
public class DynamicElementTests
{
    [Test]
    public void Dynamic_NoInstance_ShouldBeExpected()
    {
        var element = Element.Dynamic();

        element.Name.Should().Be("Dynamic");
        element.Value.Should().Be("System.Dynamic.ExpandoObject");
        element.Type.Should().Be(typeof(ExpandoObject));
        element.IsComponent.Should().BeFalse();
        element.Properties.Should().BeEmpty();
    }

    [Test]
    public void Dynamic_WithProperties_ShouldHaveExpectedProperties()
    {
        var element = Element.Dynamic(Element.Tag.This,
            [Selection.Select("Name"), Selection.Select("Description"), Selection.Select("Value")]
        );

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
    public void This_WithFakeProperties_ShouldBeExpectedCount()
    {
        var element = Element.Dynamic(Element.Default.This, [Selection.Select("First"), Selection.Select("Second")]);

        var property = element.This;

        property.Should().NotBeNull();
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(ExpandoObject));
        property.Properties.Should().HaveCount(2);
    }

    [Test]
    public void GetProperty_ValidPropertyFromTag_ShouldBeExpected()
    {
        var element = Element.Dynamic(Element.Tag.This,
            [Selection.Select("Name"), Selection.Select("Description"), Selection.Select("Value")]
        );

        var property = element.GetProperty("Name");

        property.Name.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
        property.Origin.Should().Be(typeof(ExpandoObject));
    }

    [Test]
    public void GetValue_FromValidProperty_ShouldBeExpected()
    {
        dynamic tag = new ExpandoObject();
        tag.TagName = "MyTagName";
        var element = Element.Dynamic(tag);

        var value = element.GetProperty("TagName").GetValue(tag) as object;

        value.Should().Be("MyTagName");
    }
}