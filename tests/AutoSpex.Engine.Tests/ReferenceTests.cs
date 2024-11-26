namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ReferenceTests
{
    [Test]
    public void New_TempText_ShouldNotBeNull()
    {
        var reference = new Reference("Test");

        reference.Should().NotBeNull();
    }

    [Test]
    public void New_SimpleTextWithNoSpecialCharacters_ShouldBeExpected()
    {
        var reference = new Reference("SomeText");

        reference.Key.Should().Be("SomeText");
        reference.Property.Should().BeNull();
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void New_ScopedReference_ShouldBeExpected()
    {
        var reference = new Reference("MySource/Tag/SomeTagName");

        reference.Should().NotBeNull();
        reference.Key.Should().Be("MySource/Tag/SomeTagName");
        reference.Property.Should().BeNull();
        reference.Scope.Should().NotBe(Scope.Empty);
        reference.Scope.Controller.Should().Be("MySource");
        reference.Scope.Should().Be("MySource/Tag/SomeTagName");
        reference.IsSource.Should().BeTrue();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void New_ScopedReferenceWithProperty_ShouldBeExpected()
    {
        var reference = new Reference("MySource/Tag/SomeTagName", "Value");

        reference.Should().NotBeNull();
        reference.Key.Should().Be("MySource/Tag/SomeTagName");
        reference.Property.Should().Be("Value");
        reference.Scope.Should().NotBe(Scope.Empty);
        reference.Scope.Controller.Should().Be("MySource");
        reference.Scope.Should().Be("MySource/Tag/SomeTagName");
        reference.IsSource.Should().BeTrue();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void New_SpecialReference_ShouldBeExpected()
    {
        var reference = new Reference("$this");

        reference.Should().NotBeNull();
        reference.Key.Should().Be("$this");
        reference.Property.Should().BeNull();
        reference.Scope.Should().Be(Scope.Empty);
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeTrue();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void New_VariableReference_ShouldBeExpected()
    {
        var reference = new Reference("@MyVariable");

        reference.Key.Should().Be("@MyVariable");
        reference.Property.Should().BeNull();
        reference.Scope.Should().Be(Scope.Empty);
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeTrue();
    }

    [Test]
    public void This_WhenCalled_ShouldBeExpected()
    {
        var reference = Reference.This;

        reference.Should().NotBeNull();
        reference.Key.Should().Be("$this");
        reference.Property.Should().BeNull();
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeTrue();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void Required_WhenCalled_ShouldBeExpected()
    {
        var reference = Reference.Required;

        reference.Should().NotBeNull();
        reference.Key.Should().Be("$required");
        reference.Property.Should().BeNull();
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeTrue();
        reference.IsVariable.Should().BeFalse();
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
        var reference = new Reference("$this");

        reference.ToString().Should().Be("{$this}");
    }

    [Test]
    public void ToString_SpecialReferenceWithProeprty_ShouldBeExpected()
    {
        var reference = new Reference("$this", "Description");

        reference.ToString().Should().Be("{$this}.Description");
    }

    [Test]
    public void ToString_VariableReference_ShouldBeExpected()
    {
        var reference = new Reference("@MyVar");

        reference.ToString().Should().Be("{@MyVar}");
    }

    [Test]
    public void ToString_VariableReferenceWithProperty_ShouldBeExpected()
    {
        var reference = new Reference("@MyVar", "Name");

        reference.ToString().Should().Be("{@MyVar}.Name");
    }

    [Test]
    public void Parse_SimpleText_ShouldBeExpected()
    {
        var reference = Reference.Parse("{something}");

        reference.Key.Should().Be("something");
        reference.Property.Should().BeEmpty();
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void Parse_SourceText_ShouldBeExpected()
    {
        var reference = Reference.Parse("{something/program/tag/MyTag.Member}");

        reference.Key.Should().Be("something/program/tag/MyTag.Member");
        reference.Property.Should().BeEmpty();
        reference.Scope.Should().NotBe(Scope.Empty);
        reference.Scope.Should().Be("something/program/tag/MyTag.Member");
        reference.IsSource.Should().BeTrue();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void Parse_SourceTextWithProperty_ShouldBeExpected()
    {
        var reference = Reference.Parse("{something/program/tag/MyTag.Member}[MyNestedTag.Member.Value]");

        reference.Key.Should().Be("something/program/tag/MyTag.Member");
        reference.Property.Should().Be("[MyNestedTag.Member.Value]");
        reference.Scope.Should().NotBe(Scope.Empty);
        reference.Scope.Should().Be("something/program/tag/MyTag.Member");
        reference.IsSource.Should().BeTrue();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void Parse_SpecialText_ShouldBeExpected()
    {
        var reference = Reference.Parse("{$something}");

        reference.Key.Should().Be("$something");
        reference.Property.Should().BeEmpty();
        reference.Scope.Should().Be(Scope.Empty);
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeTrue();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void Parse_SpecialTextWithProperty_ShouldBeExpected()
    {
        var reference = Reference.Parse("{$something}.PropName");

        reference.Key.Should().Be("$something");
        reference.Property.Should().Be("PropName");
        reference.Scope.Should().Be(Scope.Empty);
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeTrue();
        reference.IsVariable.Should().BeFalse();
    }

    [Test]
    public void Parse_VariableText_ShouldBeExpected()
    {
        var reference = Reference.Parse("{@MyVar}");

        reference.Key.Should().Be("@MyVar");
        reference.Property.Should().BeEmpty();
        reference.Scope.Should().Be(Scope.Empty);
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeTrue();
    }

    [Test]
    public void Parse_VariableTextWithProperty_ShouldBeExpected()
    {
        var reference = Reference.Parse("{@MyVar}.Member.Nested.Value");

        reference.Key.Should().Be("@MyVar");
        reference.Property.Should().Be("Member.Nested.Value");
        reference.Scope.Should().Be(Scope.Empty);
        reference.IsSource.Should().BeFalse();
        reference.IsSpecial.Should().BeFalse();
        reference.IsVariable.Should().BeTrue();
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
    public void Resolve_RequiredReference_ShouldThrowException()
    {
        var reference = Reference.Required;

        FluentActions.Invoking(() => reference.Resolve(null))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Value is required for evaluation. Confiugre an override to replace required reference(s).");
    }

    [Test]
    public void Resolve_WithResolverSet_ShouldReturnExpectedValue()
    {
        var reference = new Reference("@SomeValue");
        reference.ResolveTo(_ => 123);

        var result = reference.Resolve(null);

        result.Should().Be(123);
    }

    [Test]
    public void ResolveTo_SourceReference_ShouldReturnExpectedValue()
    {
        var source = L5X.Load(Known.Test);
        var reference = new Reference("/Tag/TestComplexTag");
        reference.ResolveTo(x =>
        {
            if (x is not Reference r)
                throw new InvalidOperationException("Expecting reference objec input");

            return source.Get(r.Scope);
        });
        
        var result = reference.Resolve("this doesn't matter and I'm proving it");
        
        result.Should().NotBeNull();
        result.Should().BeOfType<Tag>();
        result.As<Tag>().Name.Should().Be("TestComplexTag");
    }
    
    [Test]
    public void ResolveTo_SourceReferenceWithProperty_ShouldReturnExpectedValue()
    {
        var source = L5X.Load(Known.Test);
        var reference = new Reference("/Tag/TestComplexTag", "Description");
        reference.ResolveTo(x =>
        {
            if (x is not Reference r)
                throw new InvalidOperationException("Expecting reference objec input");

            return source.Get(r.Scope);
        });
        
        var result = reference.Resolve("this doesn't matter and I'm proving it");
        
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
        result.Should().Be("Base");
    }
}