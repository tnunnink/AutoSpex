namespace AutoSpex.Engine.Tests;

[TestFixture]
public class EnvironmentTests
{
    [Test]
    public void New_Default_ShouldNotBeNull()
    {
        var environment = new Environment();

        environment.Should().NotBeNull();
    }

    [Test]
    public void Default_WhenCalled_ShouldNotBeNull()
    {
        var environment = Environment.Default;

        environment.Should().NotBeNull();
    }

    [Test]
    public void Default_ShouldHaveExpected()
    {
        var environment = Environment.Default;

        environment.EnvironmentId.Should().NotBeEmpty();
        environment.Name.Should().Be("Default");
        environment.Comment.Should().NotBeEmpty();
        environment.IsTarget.Should().BeTrue();
        environment.Sources.Should().BeEmpty();
    }

    [Test]
    public void AddSource_ValidUri_ShouldHaveExpectedCount()
    {
        var environment = Environment.Default;

        var source = environment.Add(new Uri(Known.Test));

        environment.Sources.Should().HaveCount(1);
        source.Should().NotBeNull();
    }

    [Test]
    public void AddSource_Null_ShouldThrowException()
    {
        var environment = Environment.Default;

        FluentActions.Invoking(() => environment.Add((Uri)default!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Serialize_WhenCalled_ShouldNotBeEmpty()
    {
        var environment = Environment.Default;

        var data = environment.Serialize();

        data.Should().NotBeEmpty();
    }

    [Test]
    public void Serialize_Configured_ShouldNotBeEmpty()
    {
        var environment = Environment.Default;
        environment.Name = "My Config";
        var source = new Source(new Uri(Known.Test));
        source.Add(new Variable("TestVar", 123));
        source.Add(new Variable("AnotherVar", Radix.Decimal));
        source.Add(new Variable("ComplexVar", new Tag("Test", new TIMER())));
        environment.Sources.Add(source);
        
        var data = environment.Serialize();

        data.Should().NotBeEmpty();
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldHaveExpected()
    {
        var environment = Environment.Default;
        environment.Name = "MyConfig";
        var source = new Source(new Uri(Known.Test));
        source.Add(new Variable("TestVar", 123));
        source.Add(new Variable("AnotherVar", Radix.Decimal));
        source.Add(new Variable("ComplexVar", new Tag("Test", new TIMER())));
        environment.Sources.Add(source);
        var data = environment.Serialize();

        var result = Environment.Deserialize(data);

        result.EnvironmentId.Should().NotBeEmpty();
        result.Name.Should().Be("MyConfig");
        result.Sources.Should().HaveCount(1);
        result.Sources.First().Overrides.Should().HaveCount(3);
    }
}