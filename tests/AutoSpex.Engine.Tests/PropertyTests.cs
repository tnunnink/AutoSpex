namespace AutoSpex.Engine.Tests;

[TestFixture]
public class PropertyTests
{
    [Test]
    public void New_ValidPropertyFromTag_ShouldNotBeNull()
    {
        var property = new Property("Name", typeof(string), Element.Tag.This);

        property.Should().NotBeNull();
    }

    [Test]
    public void New_InvalidPropertyInfo_ShouldNotBeNull()
    {
        var property = new Property("Fake", typeof(string), Element.Tag.This);

        property.Should().NotBeNull();
    }

    [Test]
    public void New_ValidPropertyFromString_ShouldhaveExpectedValues()
    {
        var property = new Property("Length", typeof(int), Property.This(typeof(string)));

        property.Key.Should().Be("System.String.Length");
        property.Origin.Should().Be(typeof(string));
        property.Name.Should().Be("Length");
        property.Type.Should().Be(typeof(int));
        property.Path.Should().Be("Length");
    }

    [Test]
    public void New_InvalidPropertyInfo_ShouldhaveExpectedValues()
    {
        var property = new Property("Fake", typeof(string), Element.Tag.This);

        property.Key.Should().Be("L5Sharp.Core.Tag.Fake");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("Fake");
        property.Type.Should().Be(typeof(string));
        property.Path.Should().Be("Fake");
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
        property.DisplayName.Should().Be("string");
        property.Group.Should().Be(TypeGroup.Text);
        property.Options.Should().BeEmpty();
        property.TypeGraph.Should().HaveCount(2);
        property.InnerType.Should().Be(typeof(string));
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
    public void Property_TagIndexer_ShouldNotBeNull()
    {
        var property = Element.Tag.Property("[PRE]");

        property.Should().NotBeNull();
        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(Tag));
    }

    [Test]
    public void GetProperty_InvalidProperty_ShouldRetrunUnknown()
    {
        var origin = Element.Tag.This;

        var property = origin.GetProperty("Fake");

        property.Should().NotBeNull();
        property.Type.Should().Be(typeof(object));
        property.Name.Should().Be("Fake");
        property.Origin.Should().Be(typeof(Tag));
    }

    [Test]
    public void GetProperty_TagSimpleProperty_ShouldReturnExpectedProperty()
    {
        var origin = Element.Tag.This;

        var property = origin.GetProperty("Name");

        property.Should().NotBeNull();
        property.Key.Should().Be("L5Sharp.Core.Tag.Name");
        property.Origin.Should().Be(typeof(Tag));
        property.Path.Should().Be("Name");
        property.Name.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
        property.Group.Should().Be(TypeGroup.Text);
    }

    [Test]
    public void GetProperty_NestedProperty_ShouldReturnExpected()
    {
        var origin = Element.Tag.This;

        var property = origin.GetProperty("Radix.Name");

        property.Should().NotBeNull();
        property.Key.Should().Be("L5Sharp.Core.Tag.Radix.Name");
        property.Origin.Should().Be(typeof(Tag));
        property.Path.Should().Be("Radix.Name");
        property.Name.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
        property.Group.Should().Be(TypeGroup.Text);
    }

    [Test]
    public void GetProperty_CollectionIndexProperty_ShouldReturnExpected()
    {
        var origin = Element.Tag.This;

        var property = origin.GetProperty("Members[1]");

        property.Should().NotBeNull();
        property.Key.Should().Be("L5Sharp.Core.Tag.Members[1]");
        property.Origin.Should().Be(typeof(Tag));
        property.Path.Should().Be("Members[1]");
        property.Name.Should().Be("[1]");
        property.Type.Should().Be(typeof(Tag));
        property.Group.Should().Be(TypeGroup.Element);
    }

    [Test]
    public void GetProperty_CollectionIndexPropertyPartiallyCompleted_ShouldRetrunUnknown()
    {
        var origin = Element.Tag.This;

        var property = origin.GetProperty("Members[");

        property.Should().NotBeNull();
        property.Type.Should().Be(typeof(object));
        property.Name.Should().Be("[");
        property.Parent?.Name.Should().Be("Members");
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

        var value = property.GetValue(tag);

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

        var value = property.GetValue(instance);

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
    public void GetValue_TagIndexerNestedMember_ShouldBeExpectedInstance()
    {
        var instance = new Tag("Test", new TIMER());
        var expected = instance["PRE"];
        var property = Element.Tag.Property("[PRE]");

        var value = property.GetValue(instance);

        value.Should().NotBeNull();
        value.Should().BeOfType<Tag>();
        value.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void This_PrimitiveType_ShouldBeExpected()
    {
        var property = Property.This(typeof(int));

        property.Key.Should().Be("System.Int32.This");
        property.Origin.Should().Be(typeof(int));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(int));
        property.Path.Should().Be("This");
    }

    [Test]
    public void This_ComponentType_ShouldBeExpected()
    {
        var property = Property.This(typeof(Tag));

        property.Key.Should().Be("L5Sharp.Core.Tag.This");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(Tag));
        property.Path.Should().Be("This");
    }

    [Test]
    public void Default_WhenCalled_HasExpectedValues()
    {
        var property = Property.Default;

        property.Key.Should().Be("System.Object.This");
        property.Origin.Should().Be(typeof(object));
        property.Parent.Should().BeNull();
        property.Name.Should().Be("This");
        property.Path.Should().Be("This");
        property.DisplayName.Should().Be("Object");
        property.Group.Should().Be(TypeGroup.Default);
        property.Options.Should().BeEmpty();
        property.Properties.Should().BeEmpty();
        property.InnerType.Should().Be(typeof(object));
        property.TypeGraph.Should().HaveCount(1);
    }

    [Test]
    public void TypeGraph_WhenCalled_ShouldBeExpected()
    {
        var property = Element.Tag.Property("Name");

        var graph = property.TypeGraph;

        graph.Should().NotBeNull();
        graph.Should().HaveCount(2);
        graph[0].Should().Be(typeof(Tag));
        graph[1].Should().Be(typeof(string));
    }
}