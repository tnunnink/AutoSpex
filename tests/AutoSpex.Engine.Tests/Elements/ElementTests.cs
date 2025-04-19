namespace AutoSpex.Engine.Tests.Elements;

[TestFixture]
public class ElementTests
{
    [Test]
    public void Property_Nested_ShouldNotBeNull()
    {
        var element = Element.Controller;

        var property = element.GetProperty("RedundancyInfo.KeepTestEditsOnSwitchOver");

        property.Should().NotBeNull();
        property.Origin.Should().Be(typeof(Controller));
        property.Type.Should().Be(typeof(bool));
        property.Path.Should().Be("RedundancyInfo.KeepTestEditsOnSwitchOver");
        property.Name.Should().Be("KeepTestEditsOnSwitchOver");
    }


    [Test]
    public void IsComponent_ForComponent_ShouldBeTrue()
    {
        var element = Element.Tag;

        var result = element.IsComponent;

        result.Should().BeTrue();
    }

    [Test]
    public void DifferentObjects_ShouldBeTheSameInstance_JustMakingSureWeAreNotActuallyCreatingNewObjectsEachTime()
    {
        var first = Element.FromName("DataType");
        var second = Element.FromName("DataType");

        first.Should().BeSameAs(second);
    }

    [Test]
    public void Selectable_WhenEnumerated_ShouldHaveExpectedCount()
    {
        var result = Element.Selectable.ToList();

        result.Should().NotBeEmpty();
    }

    
}