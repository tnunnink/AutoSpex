namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ReferenceTests
{
    [Test]
    public void New_TempText_ShouldHaveExpectedValues()
    {
        var reference = new Reference("Test");

        reference.Should().NotBeNull();
        reference.Key.Should().Be("Test");
        reference.Property.Should().BeNull();
    }

    [Test]
    public void New_ScopedReference_ShouldBeExpected()
    {
        var reference = new Reference("MySource/Tag/SomeTagName");

        reference.Should().NotBeNull();
        reference.Key.Should().Be("MySource/Tag/SomeTagName");
        reference.Property.Should().BeNull();
    }

    [Test]
    public void New_ScopedReferenceWithProperty_ShouldBeExpected()
    {
        var reference = new Reference("MySource/Tag/SomeTagName", "Value");

        reference.Should().NotBeNull();
        reference.Key.Should().Be("MySource/Tag/SomeTagName");
        reference.Property.Should().Be("Value");
    }

    [Test]
    public void This_WhenCalled_ShouldBeExpected()
    {
        var reference = Reference.This;

        reference.Should().NotBeNull();
        reference.Key.Should().Be("$this");
        reference.Property.Should().BeNull();
    }

    [Test]
    public void ToString_SimpleText_ShouldBeExpected()
    {
        var reference = new Reference("Testing");

        reference.ToString().Should().Be("{Testing}");
    }

    [Test]
    public void ToString_ScopedReference_ShouldBeExpected()
    {
        var reference = new Reference("MySource/Tag/SomeTagName");

        reference.ToString().Should().Be("{MySource/Tag/SomeTagName}");
    }

    [Test]
    public void ToString_ScopedReferenceWithProperty_ShouldBeExpected()
    {
        var reference = new Reference("MySource/Tag/SomeTagName", "Value");

        reference.ToString().Should().Be("{MySource/Tag/SomeTagName}.Value");
    }

    [Test]
    public void ToString_SpecialReference_ShouldBeExpected()
    {
        var reference = Reference.This;

        reference.ToString().Should().Be("{$this}");
    }

    [Test]
    public void Parse_SimpleText_ShouldBeExpected()
    {
        var reference = Reference.Parse("{something}");

        reference.Key.Should().Be("something");
        reference.Property.Should().BeEmpty();
    }

    [Test]
    public void Parse_SourceText_ShouldBeExpected()
    {
        var reference = Reference.Parse("{something/program/tag/MyTag.Member}");

        reference.Key.Should().Be("something/program/tag/MyTag.Member");
        reference.Property.Should().BeEmpty();
    }

    [Test]
    public void Parse_SourceTextWithProperty_ShouldBeExpected()
    {
        var reference = Reference.Parse("{something/program/tag/MyTag.Member}[MyNestedTag.Member.Value]");

        reference.Key.Should().Be("something/program/tag/MyTag.Member");
        reference.Property.Should().Be("[MyNestedTag.Member.Value]");
    }

    [Test]
    public void Resolve_ThisReference_ShouldBeExpected()
    {
        const int expected = 123;
        var reference = Reference.This;

        var result = reference.Resolve(expected);

        result.Should().Be(expected);
    }

    [Test]
    public void ResolveUsing_SimpleResolver_ShouldReturnExpectedValue()
    {
        var reference = new Reference("@SomeValue");

        reference.ResolveTo(new Variable("Test", 123));

        var result = reference.Resolve(null);
        result.Should().Be(123);
    }
}