namespace AutoSpex.Engine.Tests.ElementTests;

[TestFixture]
public class GeneralTests
{
    [Test]
    public void Property_Nested_ShouldNotBeNull()
    {
        var element = Element.Controller;

        var property = element.Property("RedundancyInfo.KeepTestEditsOnSwitchOver");
        
        property.Should().NotBeNull();
        property?.Path.Should().Be("KeepTestEditsOnSwitchOver");
        property?.Type.Should().Be(typeof(bool));
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
}