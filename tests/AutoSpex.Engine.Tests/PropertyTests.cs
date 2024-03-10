﻿namespace AutoSpex.Engine.Tests;

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
        property.Properties.Should().BeEmpty();
        property.Identifier.Should().Be("string");
        property.Group.Should().Be(TypeGroup.Text);
        property.Options.Should().BeEmpty();
    }

    [Test]
    public void Properties_ForTagProperty_ShouldBeExpectedCount()
    {
        var property = new Property(typeof(Tag), "Test", typeof(Tag));

        var properties = property.Properties;

        properties.Should().HaveCount(24);
    }

    [Test]
    public void Path_NestedProperty_ShouldBeExpected()
    {
        var property = new Property(typeof(Tag), "ConsumeInfo.Producer", typeof(string));

        property.Path.Should().Be("ConsumeInfo.Producer");
    }
    
    [Test]
    public void Options_EnumProperty_ShouldNotBeEmpty()
    {
        var property = new Property(typeof(Tag), "ExternalAccess", typeof(ExternalAccess));

        property.Options.Should().HaveCount(3);
    }

    [Test]
    public void Getter_CollectionType_ShouldWork()
    {
        
    }
}