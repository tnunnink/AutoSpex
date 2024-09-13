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
    public void AddSource_ValidUri_ShouldHaveExpectedCount()
    {
        var environment = new Environment();

        var source = environment.Add(new Uri(Known.Test));

        environment.Sources.Should().HaveCount(1);
        source.Should().NotBeNull();
    }

    [Test]
    public void AddSource_Null_ShouldThrowException()
    {
        var environment = new Environment();

        FluentActions.Invoking(() => environment.Add(default!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Serialize_WhenCalled_ShouldNotBeEmpty()
    {
        var environment = new Environment();

        var data = environment.Serialize();

        data.Should().NotBeEmpty();
    }

    [Test]
    public void Serialize_Configured_ShouldNotBeEmpty()
    {
        var environment = new Environment
        {
            Name = "My Config"
        };
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
        var environment = new Environment
        {
            Name = "MyConfig"
        };
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