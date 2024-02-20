namespace AutoSpex.Engine.Tests;

[TestFixture]
public class TypeGroupTests
{
    [Test]
    public void FromType_bool_ShouldBeBoolean()
    {
        var group = TypeGroup.FromType(typeof(bool));

        group.Should().Be(TypeGroup.Boolean);
    }
    
    [Test]
    public void FromType_IEnumerable_ShouldBeCollection()
    {
        var group = TypeGroup.FromType(typeof(IEnumerable<>));

        group.Should().Be(TypeGroup.Collection);
    }
    
    [Test]
    public void FromType_IEnumerableOfString_ShouldBeCollection()
    {
        var group = TypeGroup.FromType(typeof(IEnumerable<string>));

        group.Should().Be(TypeGroup.Collection);
    }
    
    [Test]
    public void FromType_LogixContainer_ShouldBeCollection()
    {
        var group = TypeGroup.FromType(typeof(LogixContainer<Tag>));

        group.Should().Be(TypeGroup.Collection);
    }
}