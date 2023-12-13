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
        property.Name.Should().Be("KeepTestEditsOnSwitchOver");
        property.Type.Should().Be(typeof(bool));
    }

    [Test]
    public void Getter_Nested_ShouldNotBeNull()
    {
        var element = Element.Controller;

        var getter = element.Getter("RedundancyInfo");
        
        getter.Should().NotBeNull();
    }
    
    [Test]
    public void DifferentObjects_ShouldBeTheSameInstance_JustMakingSureWeAreNotActuallyCreatingNewObjectsEachTime()
    {
        var first = Element.FromName("DataType");
        var second = Element.FromName("DataType");

        first.Should().BeSameAs(second);
    }
}