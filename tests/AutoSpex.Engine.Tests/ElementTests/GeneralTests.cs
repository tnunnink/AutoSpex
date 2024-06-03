﻿namespace AutoSpex.Engine.Tests.ElementTests;

[TestFixture]
public class GeneralTests
{
    [Test]
    public void Property_Nested_ShouldNotBeNull()
    {
        var element = Element.Controller;

        var property = element.Property("RedundancyInfo.KeepTestEditsOnSwitchOver");
        
        property.Should().NotBeNull();
        property?.Origin.Should().Be(typeof(Controller));
        property?.Type.Should().Be(typeof(bool));
        property?.Path.Should().Be("RedundancyInfo.KeepTestEditsOnSwitchOver");
        property?.Name.Should().Be("KeepTestEditsOnSwitchOver");
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

        result.Should().HaveCount(15);
    }

    [Test]
    public void Components_WhenCalled_ShouldNotBeEmpty()
    {
        var result = Element.Components.ToList();
        result.Should().NotBeEmpty();
    }

    [Test]
    public void Property_This_ShouldReturnExpected()
    {
        var element = Element.Tag;

        var result = element.Property("This");

        result.Should().NotBeNull();
        result?.Name.Should().Be("This");
        result?.Path.Should().BeEmpty();
        result?.Type.Should().Be(typeof(Tag));
        result?.Group.Should().Be(TypeGroup.Element);
        result?.Origin.Should().Be(typeof(Tag));
        result?.Properties.Should().NotBeEmpty();
    }
}