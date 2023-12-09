namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ElementTests
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
        
        element.Properties.Should().ContainSingle(p => p.Name == nameof(DataType.Family));
    }

    [Test]
    public void Tag_WhenCalled_ShouldHaveExpectedValues()
    {
        var element = Element.Tag;

        element.Type.Should().Be(typeof(Tag));
        element.Name.Should().Be(nameof(Tag));
        element.Value.Should().Be(nameof(Tag));
        element.Properties.Should().NotBeEmpty();
    }
}