namespace AutoSpex.Engine.Tests.Elements;

[TestFixture]
public class DataTypeElementTests
{
    [Test]
    public void DataType_WhenCalled_ShouldNotBeNull()
    {
        var element = Element.DataType;

        element.Should().NotBeNull();
    }

    [Test]
    public void DataType_WhenCalled_ShouldHaveExpectedValues()
    {
        var element = Element.DataType;

        element.Type.Should().Be(typeof(DataType));
        element.Name.Should().Be(nameof(DataType));
        element.Value.Should().Be(nameof(DataType));
        element.Properties.Should().NotBeEmpty();
    }

    [Test]
    public void DataType_FamilyProperty_ShouldExist()
    {
        var element = Element.DataType;

        element.Properties.Should().ContainSingle(p => p.Path == nameof(DataType.Family));
    }

    [Test]
    public void Property_Name_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Name");

        property.Should().NotBeNull();
        property.Path.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
    }

    [Test]
    public void Property_Description_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Description");

        property.Should().NotBeNull();
        property.Path.Should().Be("Description");
        property.Type.Should().Be(typeof(string));
    }

    [Test]
    public void Property_Family_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Family");

        property.Should().NotBeNull();
    }

    [Test]
    public void Property_Class_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Class");

        property.Should().NotBeNull();
        property.Path.Should().Be("Class");
        property.Type.Should().Be(typeof(DataTypeClass));
    }

    [Test]
    public void Property_Members_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Members");

        property.Should().NotBeNull();
        property.Path.Should().Be("Members");
        property.Type.Should().Be(typeof(LogixContainer<DataTypeMember>));
        property.DisplayName.Should().Be("DataTypeMember[]");
        property.Group.Should().Be(TypeGroup.Collection);
    }

    [Test]
    public void Property_Scope_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Scope");

        property.Should().NotBeNull();
        property.Path.Should().Be("Scope");
        property.Type.Should().Be(typeof(Scope));
    }

    [Test]
    public void Property_NestedProperty_ShouldBeExpected()
    {
        var element = Element.DataType;

        var property = element.GetProperty("Family.Name");

        property.Should().NotBeNull();
    }

    [Test]
    public void IndexValues_KnownTest_ShouldNotBeEmpty()
    {
        var content = L5X.Load(Known.Test);
        var element = Element.DataType;

        var paris = element.IndexValues(content).ToList();

        paris.Should().NotBeEmpty();
    }

    [Test]
    public void IndexValues_KnownTest_ShouldContainExpectedPropertiesAtLeastOnce()
    {
        var content = L5X.Load(Known.Test);
        var element = Element.DataType;

        var properties = element.IndexValues(content).Select(x => x.Key).Distinct().ToList();

        properties.Should().HaveCount(3);
        properties.Should().Contain(p => p.Name == "Name");
        properties.Should().Contain(p => p.Name == "Description");
        properties.Should().Contain(p => p.Name == "Scope");
    }

    [Test]
    public void IndexValue_KnownTest_ShouldContainKnownValues()
    {
        var content = L5X.Load(Known.Test);
        var element = Element.DataType;

        var values = element.IndexValues(content).Select(x => x.Value).ToList();
        
        values.Should().Contain("AlarmType");
        values.Should().Contain("ArrayType");
        values.Should().Contain("ComplexType");
        values.Should().Contain("SimpleType");
        values.Should().Contain("This is a test");
    }
}