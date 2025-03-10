﻿using System.Dynamic;

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

        property.Key.Should().Be("System.String:Length");
        property.Origin.Should().Be(typeof(string));
        property.Name.Should().Be("Length");
        property.Type.Should().Be(typeof(int));
        property.Path.Should().Be("Length");
    }

    [Test]
    public void New_InvalidPropertyInfo_ShouldhaveExpectedValues()
    {
        var property = new Property("Fake", typeof(string), Element.Tag.This);

        property.Key.Should().Be("L5Sharp.Core.Tag:Fake");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("Fake");
        property.Type.Should().Be(typeof(string));
        property.Path.Should().Be("Fake");
    }

    [Test]
    public void New_ValidPropertyInfo_ShouldHaveExpectedValues()
    {
        var property = new Property("Name", typeof(string), Element.Tag.This);

        property.Key.Should().Be("L5Sharp.Core.Tag:Name");
        property.Origin.Should().Be(typeof(Tag));
        property.Parent.Should().BeEquivalentTo(Element.Tag.This);
        property.Path.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Name");
        property.Properties.Should().BeEmpty();
        property.DisplayName.Should().Be("string");
        property.Group.Should().Be(TypeGroup.Text);
        property.TypeGraph.Should().HaveCount(2);
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
    public void Property_TagIndexer_ShouldNotBeNull()
    {
        var property = Element.Tag.GetProperty("[PRE]");

        property.Should().NotBeNull();
        property.Key.Should().Be("L5Sharp.Core.Tag:[PRE]");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("[PRE]");
        property.Type.Should().Be(typeof(Tag));
        property.Parent.Should().BeEquivalentTo(Element.Tag.This);
        property.Path.Should().Be("[PRE]");
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
        property.Key.Should().Be("L5Sharp.Core.Tag:Name");
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
        property.Key.Should().Be("L5Sharp.Core.Tag:Radix.Name");
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
        property.Key.Should().Be("L5Sharp.Core.Tag:Members[1]");
        property.Origin.Should().Be(typeof(Tag));
        property.Path.Should().Be("Members[1]");
        property.Name.Should().Be("[1]");
        property.Type.Should().Be(typeof(Tag));
        property.Group.Should().Be(TypeGroup.Element);
    }

    [Test]
    public void GetProperty_RungInstructionIndexProperty_ShouldReturnExpected()
    {
        var origin = Element.Rung.This;

        var property = origin.GetProperty("Instructions[0]");

        property.Should().NotBeNull();
        property.Key.Should().Be("L5Sharp.Core.Rung:Instructions[0]");
        property.Origin.Should().Be(typeof(Rung));
        property.Path.Should().Be("Instructions[0]");
        property.Name.Should().Be("[0]");
        property.Type.Should().Be(typeof(Instruction));
        property.Group.Should().Be(TypeGroup.Text);
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
    public void GetProperty_ExpandoObject_ShouldBeExpected()
    {
        var property = Property.This(typeof(ExpandoObject));

        property.Should().NotBeNull();
        property.Key.Should().Be("System.Dynamic.ExpandoObject:This");
        property.Origin.Should().Be(typeof(ExpandoObject));
        property.Type.Should().Be(typeof(ExpandoObject));
        property.DisplayName.Should().Be("object");
        property.Path.Should().Be("This");
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(ExpandoObject));
        property.Group.Should().Be(TypeGroup.Element);
    }

    [Test]
    public void GetProperty_ExpandoObjectProperty_ShouldBeExpected()
    {
        var property = Property.This(typeof(ExpandoObject));

        var member = property.GetProperty("Test");

        member.Should().NotBeNull();
        member.Origin.Should().Be(typeof(ExpandoObject));
        member.Key.Should().Be("System.Dynamic.ExpandoObject:Test");
        member.Type.Should().Be(typeof(object));
        member.Path.Should().Be("Test");
        member.Name.Should().Be("Test");
        member.Group.Should().Be(TypeGroup.Default);
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
        var property = Element.Tag.GetProperty("Radix.Name");
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

        var parent = Element.DataType.GetProperty("Members");
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

        var parent = Element.DataType.GetProperty("Members");
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
        var property = Element.Tag.GetProperty("Members");

        var value = property.GetValue(instance);

        value.Should().NotBeNull();
        value.Should().BeOfType<List<Tag>>();
        value.As<List<Tag>>().Should().HaveCount(6);
    }

    [Test]
    public void GetValue_CustomPropertyNestedMember_ShouldBeExpected()
    {
        var instance = new Tag("Test", new TIMER());
        var custom = Element.Tag.GetProperty("Members");
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
        var property = Element.Tag.GetProperty("[PRE]");

        var value = property.GetValue(instance);

        value.Should().NotBeNull();
        value.Should().BeOfType<Tag>();
        value.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void GetValue_ExpandoObjectWithProperty_ShouldReturnExpectedValue()
    {
        dynamic bag = new ExpandoObject();
        bag.Test = "This is a test value";
        var property = Property.This(typeof(ExpandoObject)).GetProperty("Test");

        var value = property.GetValue(bag) as object;

        value.Should().NotBeNull();
        value.Should().BeOfType<string>();
        value.Should().Be("This is a test value");
    }

    [Test]
    public void GetValue_ExpandoNestedPropertyFromTag_ShouldBeExpectedValue()
    {
        var tag = new Tag("MyTag", 123);
        var bag = (IDictionary<string, object?>)new ExpandoObject();
        bag.Add("Member", tag.TagName.Member);

        var element = Element.Dynamic(bag as ExpandoObject);
        
        var property = element.GetProperty("Member");
        property.Should().NotBeNull();

        var value = property.GetValue(bag);
        value.Should().Be(tag.TagName.Member);
        
    }
    
    [Test]
    public void GetValue_NestedPropertyFromExpandoProperty_ShouldBeExpectedValue()
    {
        var bag = (IDictionary<string, object?>)new ExpandoObject();
        bag.Add("Radix", Radix.Decimal);
        var element = Element.Dynamic(bag as ExpandoObject);

        var property = element.This.GetProperty("Radix.Value");
        property.Should().NotBeNull();
        property.Name.Should().Be("Value");
        property.Type.Should().Be(typeof(string));
        property.Origin.Should().Be(typeof(ExpandoObject));
        property.Parent.Should().NotBeNull();

        var value = property.GetValue(bag);
        value.Should().Be(Radix.Decimal.Value);
    }

    [Test]
    public void GetValue_ExpandoObjectWithoutProperty_ShouldThrowException()
    {
        dynamic bag = new ExpandoObject();
        bag.Test = "This is a test value";
        var property = Element.Dynamic(bag).GetProperty("Fake");

        FluentActions.Invoking(() => property.GetValue(bag)).Should()
            .Throw<KeyNotFoundException>().WithMessage("The specified key 'Fake' does not exist in the ExpandoObject.");
    }

    [Test]
    public void This_PrimitiveType_ShouldBeExpected()
    {
        var property = Property.This(typeof(int));

        property.Key.Should().Be("System.Int32:This");
        property.Origin.Should().Be(typeof(int));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(int));
        property.Path.Should().Be("This");
    }

    [Test]
    public void This_ComponentType_ShouldBeExpected()
    {
        var property = Property.This(typeof(Tag));

        property.Key.Should().Be("L5Sharp.Core.Tag:This");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(Tag));
        property.Path.Should().Be("This");
    }

    [Test]
    public void Default_WhenCalled_HasExpectedValues()
    {
        var property = Property.Default;

        property.Key.Should().Be("System.Object");
        property.Origin.Should().Be(typeof(object));
        property.Parent.Should().BeNull();
        property.Name.Should().BeEmpty();
        property.Path.Should().BeEmpty();
        property.DisplayName.Should().Be("Object");
        property.Group.Should().Be(TypeGroup.Default);
        property.Properties.Should().BeEmpty();
        property.InnerType.Should().Be(typeof(object));
        property.TypeGraph.Should().HaveCount(1);
    }

    [Test]
    public void TypeGraph_WhenCalled_ShouldBeExpected()
    {
        var property = Element.Tag.GetProperty("Name");

        var graph = property.TypeGraph;

        graph.Should().NotBeNull();
        graph.Should().HaveCount(2);
        graph[0].Should().Be(typeof(Tag));
        graph[1].Should().Be(typeof(string));
    }

    [Test]
    public void Parse_PrimitiveTypeNoPath_ShouldBeExpected()
    {
        var property = Property.Parse("System.Int32");

        property.Key.Should().Be("System.Int32:This");
        property.Origin.Should().Be(typeof(int));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(int));
        property.Path.Should().Be("This");
    }

    [Test]
    public void Parse_PrimitiveTypeThisPath_ShouldBeExpected()
    {
        var property = Property.Parse("System.Int32:This");

        property.Key.Should().Be("System.Int32:This");
        property.Origin.Should().Be(typeof(int));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(int));
        property.Path.Should().Be("This");
    }

    [Test]
    public void Parse_ElementTypeNoPath_ShouldBeExpected()
    {
        var property = Property.Parse("L5Sharp.Core.Tag");

        property.Key.Should().Be("L5Sharp.Core.Tag:This");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(Tag));
        property.Path.Should().Be("This");
    }

    [Test]
    public void Parse_ElementTypeThisPath_ShouldBeExpected()
    {
        var property = Property.Parse("L5Sharp.Core.Tag:This");

        property.Key.Should().Be("L5Sharp.Core.Tag:This");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("This");
        property.Type.Should().Be(typeof(Tag));
        property.Path.Should().Be("This");
    }

    [Test]
    public void Parse_ElementTypeValidPath_ShouldBeExpected()
    {
        var property = Property.Parse("L5Sharp.Core.Tag:TagName.Member");

        property.Key.Should().Be("L5Sharp.Core.Tag:TagName.Member");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("Member");
        property.Type.Should().Be(typeof(string));
        property.Path.Should().Be("TagName.Member");
    }

    [Test]
    public void Parse_ElementTypeInvalidPath_ShouldBeExpected()
    {
        var property = Property.Parse("L5Sharp.Core.Tag:Fake.Member");

        property.Key.Should().Be("L5Sharp.Core.Tag:Fake.Member");
        property.Origin.Should().Be(typeof(Tag));
        property.Name.Should().Be("Member");
        property.Type.Should().Be(typeof(object));
        property.Path.Should().Be("Fake.Member");
    }
}