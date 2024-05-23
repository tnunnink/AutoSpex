namespace AutoSpex.Engine.Tests;

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

        properties.Should().NotBeEmpty();
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

    [Test]
    public void Getter_PseudoCollectionItemProperty_ShouldNotBeNullAndReturnExpectedValue()
    {
        var itemProperty = new Property(Element.DataType.Type, "Members[1]", typeof(DataTypeMember));

        var instance = new DataType("Test");
        instance.Members.Add(new DataTypeMember{Name = "Child1", DataType = "DINT"});
        instance.Members.Add(new DataTypeMember{Name = "Child2", DataType = "BOOL"});
        instance.Members.Add(new DataTypeMember{Name = "Child3", DataType = "TIMER"});


        var getter = itemProperty.Getter();
        getter.Should().NotBeNull();

        var value = getter(instance);
        value.Should().NotBeNull();
        value.Should().BeOfType<DataTypeMember>();
        value.As<DataTypeMember>().Name.Should().Be("Child2");
        value.As<DataTypeMember>().DataType.Should().Be("BOOL");
    }
    
    [Test]
    public void Getter_NestedCollectionItemProperty_ShouldNotBeNullAndReturnExpectedValue()
    {
        var instance = new DataType("Test");
        instance.Members.Add(new DataTypeMember{Name = "Child1", DataType = "DINT"});
        instance.Members.Add(new DataTypeMember{Name = "Child2", DataType = "BOOL"});
        instance.Members.Add(new DataTypeMember{Name = "Child3", DataType = "TIMER"});
        var itemProperty = new Property(Element.DataType.Type, "Members[1].Name", typeof(string));


        var getter = itemProperty.Getter();
        getter.Should().NotBeNull();

        var value = getter(instance);
        value.Should().NotBeNull();
        value.Should().BeOfType<string>();
        value.Should().Be("Child2");
    }
}