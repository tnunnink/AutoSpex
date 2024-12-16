namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SelectionTests
{
    [Test]
    public void Default_WhenCalled_ShouldBeExpected()
    {
        var selection = new Selection();

        selection.Property.Should().BeEmpty();
        selection.Alias.Should().BeEmpty();
    }

    [Test]
    public void New_NullProperty_ShouldThrowException()
    {
        FluentActions.Invoking(() => new Selection(null!)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void New_SimpleProperty_ShouldBeExpected()
    {
        var selection = new Selection("Value");

        selection.Alias.Should().Be("Value");
        selection.Property.Should().Be("Value");
    }

    [Test]
    public void New_PropertyInstance_ShouldBeExpected()
    {
        var selection = new Selection(Element.Tag.This.GetProperty("DataType"));

        selection.Alias.Should().Be("DataType");
        selection.Property.Should().Be("DataType");
    }

    [Test]
    public void New_NestedPropertyNoAlias_ShouldBeExpected()
    {
        var selection = new Selection("TagName.Member");

        selection.Alias.Should().Be("Member");
        selection.Property.Should().Be("TagName.Member");
    }

    [Test]
    public void New_IndexPropertyNoAlias_ShouldBeExpected()
    {
        var selection = new Selection("Members[0]");

        selection.Alias.Should().Be("Members[0]");
        selection.Property.Should().Be("Members[0]");
    }

    [Test]
    public void New_IndexPropertySubMemberNoAlias_ShouldBeExpected()
    {
        var selection = new Selection("Member[0].TagName");

        selection.Alias.Should().Be("TagName");
        selection.Property.Should().Be("Member[0].TagName");
    }

    [Test]
    public void Select_ValidProperty_ShouldBeExpected()
    {
        var selection = Selection.Select("ExternalAccess.Value");

        selection.Alias.Should().Be("Value");
        selection.Property.Should().Be("ExternalAccess.Value");
    }

    [Test]
    public void As_ValidAlias_ShouldBeExpected()
    {
        var selection = Selection.Select("ExternalAccess.Value").As("Access");

        selection.Alias.Should().Be("Access");
        selection.Property.Should().Be("ExternalAccess.Value");
    }
}