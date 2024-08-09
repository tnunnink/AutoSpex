namespace AutoSpex.Engine.Tests;

[TestFixture]
public class TypeGroupTests
{
    [Test]
    public void Default_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Default;

        group.Name.Should().Be("Default");
        group.Value.Should().Be(0);
    }
    
    [Test]
    public void Boolean_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Boolean;

        group.Name.Should().Be("Boolean");
        group.Value.Should().Be(1);
    }
    
    [Test]
    public void Number_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Number;

        group.Name.Should().Be("Number");
        group.Value.Should().Be(2);
    }
    
    [Test]
    public void Text_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Text;

        group.Name.Should().Be("Text");
        group.Value.Should().Be(3);
    }
     
    [Test]
    public void Date_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Date;

        group.Name.Should().Be("Date");
        group.Value.Should().Be(4);
    }
    
    [Test]
    public void Enum_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Enum;

        group.Name.Should().Be("Enum");
        group.Value.Should().Be(5);
    }
    
    [Test]
    public void Collection_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Collection;

        group.Name.Should().Be("Collection");
        group.Value.Should().Be(6);
    }
    
    [Test]
    public void Element_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Element;

        group.Name.Should().Be("Element");
        group.Value.Should().Be(7);
    }
    
    [Test]
    public void Criterion_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Criterion;

        group.Name.Should().Be("Criterion");
        group.Value.Should().Be(8);
    }
    
    [Test]
    public void Argument_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Argument;

        group.Name.Should().Be("Argument");
        group.Value.Should().Be(9);
    }
    
    [Test]
    public void Variable_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Variable;

        group.Name.Should().Be("Variable");
        group.Value.Should().Be(10);
    }
    
    [Test]
    public void Reference_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Reference;

        group.Name.Should().Be("Reference");
        group.Value.Should().Be(11);
    }
    
    [Test]
    public void Selectable_WhenCalled_ShouldNotBeEmpty()
    {
        var groups = TypeGroup.Selectable;

        groups.Should().NotBeEmpty();
    }
    
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