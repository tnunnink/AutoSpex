namespace AutoSpex.Engine.Tests;

[TestFixture]
public class PropertyTests
{
    [Test]
    public void New_ValidPropertyInfo_ShouldNotBeNull()
    {
        var property = new Property("Name", typeof(string), Element.Tag.This);
        property.Should().NotBeNull();
    }

    [Test]
    public void New_ValidPropertyInfo_ShouldHaveExpectedValues()
    {
        var property = new Property("Name", typeof(string), Element.Tag.This);

        property.Key.Should().Be("L5Sharp.Core.Tag.Name");
        property.Origin.Should().Be(typeof(Tag));
        property.Parent.Should().BeEquivalentTo(Element.Tag.This);
        property.Path.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Name");
        property.Properties.Should().BeEmpty();
        property.Identifier.Should().Be("string");
        property.Group.Should().Be(TypeGroup.Text);
        property.Options.Should().BeEmpty();
    }

    [Test]
    public void Properties_ForTagProperty_ShouldBeExpectedCount()
    {
        var property = Element.Tag.This;

        var properties = property.Properties;

        properties.Should().NotBeEmpty();
    }

    [Test]
    public void Path_NestedProperty_ShouldBeExpected()
    {
        var property = new Property("ConsumeInfo.Producer", typeof(string), Element.Tag.This);

        property.Path.Should().Be("ConsumeInfo.Producer");
    }

    [Test]
    public void Options_EnumProperty_ShouldNotBeEmpty()
    {
        var property = new Property("ExternalAccess", typeof(ExternalAccess), Element.Tag.This);

        property.Options.Should().HaveCount(3);
    }

    [Test]
    public void GetValue_TagSimpleProperty_ShouldReturnExpectedValue()
    {
        var tag = new Tag("Test", 123);
        var property = new Property("Name", typeof(string), Element.Tag.This);

        var value = property.GetValue(tag);

        value.Should().Be("Test");
    }
    
    [Test]
    public void GetValue_TagNestedProperty_ShouldReturnExpectedValue()
    {
        var tag = new Tag("Test", new DINT(123, Radix.Binary));
        var property = Element.Tag.Property("Radix.Name");
        property.Should().NotBeNull();

        var value = property?.GetValue(tag);

        value.Should().Be(Radix.Binary.Name);
    }

    [Test]
    public void GetValue_CollectionItemProperty_ShouldNotBeNullAndReturnExpectedValue()
    {
        var instance = new DataType("Test");
        instance.Members.Add(new DataTypeMember { Name = "Child1", DataType = "DINT" });
        instance.Members.Add(new DataTypeMember { Name = "Child2", DataType = "BOOL" });
        instance.Members.Add(new DataTypeMember { Name = "Child3", DataType = "TIMER" });

        var parent = Element.DataType.Property("Members");
        var property = new Property("[1]", typeof(DataTypeMember), parent);
        
        var value = property.GetValue(instance);

        value.Should().NotBeNull();
        value.Should().BeOfType<DataTypeMember>();
        value.As<DataTypeMember>().Name.Should().Be("Child2");
        value.As<DataTypeMember>().DataType.Should().Be("BOOL");
    }

    [Test]
    public void GetValue_CollectionItemNestedProperty_ShouldNotBeNullAndReturnExpectedValue()
    {
        var instance = new DataType("Test");
        instance.Members.Add(new DataTypeMember { Name = "Child1", DataType = "DINT" });
        instance.Members.Add(new DataTypeMember { Name = "Child2", DataType = "BOOL" });
        instance.Members.Add(new DataTypeMember { Name = "Child3", DataType = "TIMER" });

        var parent = Element.DataType.Property("Members");
        var item = new Property("[1]", typeof(DataTypeMember), parent);
        var property = new Property("Name", typeof(string), item);
        
        var value = property.GetValue(instance);
        
        value.Should().NotBeNull();
        value.Should().BeOfType<string>();
        value.Should().Be("Child2");
    }

    [Test]
    public void GetValue_ThisProperty_ShouldBeExpected()
    {
        var instance = new Tag("Test", 123);
        var property = Element.Tag.This;
        var value = property.GetValue(instance);
        value.Should().Be(instance);
    }

    [Test]
    public void GetValue_CustomProperty_ShouldBeExpected()
    {
        var instance = new Tag("Test", new TIMER());
        var property = Element.Tag.Property("Members");

        var value = property?.GetValue(instance);

        value.Should().NotBeNull();
        value.Should().BeOfType<List<Tag>>();
        value.As<List<Tag>>().Should().HaveCount(6);
    }
    
    [Test]
    public void GetValue_CustomPropertyNestedMember_ShouldBeExpected()
    {
        var instance = new Tag("Test", new TIMER());
        var custom = Element.Tag.Property("Members");
        var item = new Property("[1]", typeof(Tag), custom);

        var value = item.GetValue(instance);

        value.Should().NotBeNull();
        value.Should().BeOfType<Tag>();
        value.As<Tag>().TagName.Should().Be("Test.PRE");
        value.As<Tag>().DataType.Should().Be("DINT");
    }

    [Test]
    public void TypeGraph_WhenCalled_ShouldBeExpected()
    {
        var property = Element.Tag.Property("Name");

        var graph = property?.TypeGraph;
        
        graph.Should().NotBeNull();
        graph.Should().HaveCount(2);
        graph?[0].Should().Be(typeof(Tag));
        graph?[1].Should().Be(typeof(string));
    }
}