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

        var property = element.Property("Name");

        property.Should().NotBeNull();
        property.Path.Should().Be("Name");
        property.Type.Should().Be(typeof(string));
    }
    
    [Test]
    public void Property_Description_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.Property("Description");

        property.Should().NotBeNull();
        property.Path.Should().Be("Description");
        property.Type.Should().Be(typeof(string));
    }
    
    [Test]
    public void Property_Family_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.Property("Family");

        property.Should().NotBeNull();
    }
    
    [Test]
    public void Property_Class_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.Property("Class");

        property.Should().NotBeNull();
        property.Path.Should().Be("Class");
        property.Type.Should().Be(typeof(DataTypeClass));
    }
    
    [Test]
    public void Property_Members_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.Property("Members");

        property.Should().NotBeNull();
        property.Path.Should().Be("Members");
        property.Type.Should().Be(typeof(LogixContainer<DataTypeMember>));
        property.DisplayName.Should().Be("DataTypeMember[]");
        property.Options.Should().BeEmpty();
        property.Group.Should().Be(TypeGroup.Collection);
    }
    
    [Test]
    public void Property_Scope_ShouldNotBeNull()
    {
        var element = Element.DataType;

        var property = element.Property("Scope");

        property.Should().NotBeNull();
        property.Path.Should().Be("Scope");
        property.Type.Should().Be(typeof(Scope));
    }

    [Test]
    public void Property_NestedProperty_ShouldBeExpected()
    {
        var element = Element.DataType;

        var property = element.Property("Family.Name");

        property.Should().NotBeNull();
    }
}