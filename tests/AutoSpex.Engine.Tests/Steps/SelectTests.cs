// ReSharper disable UseObjectOrCollectionInitializer

using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests.Steps;

[TestFixture]
public class SelectTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var step = new Select();

        step.Property.Should().BeEmpty();
    }

    [Test]
    public void Property_SetValue_ShouldBeExpected()
    {
        var step = new Select();

        step.Property = "Value";

        step.Property.Should().Be("Value");
    }

    [Test]
    public void Process_SimpleProperty_ShouldBeExpected()
    {
        var step = new Select();
        step.Property = "Value";

        var input = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", new STRING("This is a value"))
        };


        var results = step.Process(input).ToList();

        results.Should().HaveCount(3);
        results.Should().AllBeAssignableTo<LogixData>();
        results[0].Should().Be(123);
        results[1].Should().BeEquivalentTo(new TIMER());
        results[2]?.ToString().Should().Be("This is a value");
    }

    [Test]
    public void Process_TagParentProperty_ShouldBeExpected()
    {
        var step = new Select();
        step.Property = "Parent";

        var input = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", new STRING("This is a value"))
        }.SelectMany(t => t.Members());

        var results = step.Process(input).ToList();

        results.Should().HaveCount(5);
        results.Should().AllBeOfType<Tag>();
        results.Cast<Tag>().Should().AllSatisfy(t => t.Name.Should().Be("AnotherTag"));
    }

    [Test]
    public void Process_MembersProperty_ShouldBeExpected()
    {
        var step = new Select();
        step.Property = "Members";

        var input = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", new STRING("This is a value"))
        };

        var results = step.Process(input).ToList();

        results.Should().HaveCountGreaterThan(3);
        results.Should().AllBeAssignableTo<Tag>();
    }

    [Test]
    public void Process_NestedProperty_ShouldBeExpected()
    {
        var step = new Select();
        step.Property = "TagName.Operand";

        var input = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", new STRING("This is a value"))
        }.SelectMany(t => t.Members());

        var results = step.Process(input).ToList();

        results.Should().HaveCountGreaterThan(3);
        results.Should().AllBeOfType<string>();
    }

    [Test]
    public Task Serialize_DefaultInstance_ShouldBeVerified()
    {
        var step = new Select();

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_Configured_ShouldBeVerified()
    {
        var step = new Select();
        step.Property = "Radix.Value";

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_Default_ShouldBeExpected()
    {
        var step = new Select();
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Select>(json);

        result.Should().BeEquivalentTo(step);
    }

    [Test]
    public void Deserialize_Configured_ShouldBeExpected()
    {
        var step = new Select();
        step.Property = "MainRoutineName";
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Select>(json);

        result.Should().BeEquivalentTo(step);
    }

    [Test]
    public void Deserialize_ConfiguredAsStep_ShouldBeExpected()
    {
        var expected = new Select();
        expected.Property = "Rate";
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Select>(json);

        result.Should().BeEquivalentTo(expected);
    }
}