// ReSharper disable UseObjectOrCollectionInitializer

using System.Dynamic;
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

        step.Selections.Should().BeEmpty();
    }

    [Test]
    public void New_WithInstance_ShouldBeExpected()
    {
        var step = new Select(new Selection());

        step.Selections.Should().HaveCount(1);
    }

    [Test]
    public void Property_SingleProperty_ShouldBeExpected()
    {
        var step = new Select();

        step.Selections.Add(new Selection("Value"));

        step.Selections.Should().HaveCount(1);
    }

    [Test]
    public void Returns_NoProperties_ShouldBeExpected()
    {
        var step = new Select();

        var result = step.Returns(Element.Tag.This);

        result.Should().BeEquivalentTo(Property.This(typeof(Tag)));
    }

    [Test]
    public void Returns_SingleSimpleProperty_ShouldBeExpected()
    {
        var step = new Select("TagName");

        var result = step.Returns(Element.Tag.This);

        result.Should().BeEquivalentTo(Property.This(typeof(TagName)));
    }

    [Test]
    public void Returns_SingleComplexProperty_ShouldBeExpected()
    {
        var step = new Select("Security");

        var result = step.Returns(Element.Controller.This);

        result.Should().BeEquivalentTo(Property.This(typeof(Security)));
    }

    [Test]
    public void Returns_SingleCollectionProperty_ShouldBeExpected()
    {
        var step = new Select("Tags");

        var result = step.Returns(Element.Program.This);

        result.Should().BeEquivalentTo(Property.This(typeof(Tag)));
    }

    [Test]
    public void Returns_MultipleSimpleProperties_ShouldBeExpected()
    {
        var step = new Select("TagName", "Description", "Value");

        var result = step.Returns(Element.Tag.This);

        result.Name.Should().Be("This");
        result.Type.Should().Be(typeof(ExpandoObject));
        result.Properties.Should().HaveCount(3);
    }

    [Test]
    public void Returns_MultipleNestedProperties_ShouldBeExpected()
    {
        var step = new Select("TagName.Member", "[PRE].Value", "Parent.Parent.Name");

        var result = step.Returns(Element.Tag.This);

        result.Name.Should().Be("This");
        result.Type.Should().Be(typeof(ExpandoObject));
        result.Properties.Should().HaveCount(3);
    }

    [Test]
    public void Returns_MultipleProperties_PropertiesShouldHaveExpectedInstances()
    {
        var step = new Select("TagName", "Description", "Value");

        var result = step.Returns(Element.Tag.This);

        var properties = result.Properties.ToList();

        properties[0].Name.Should().Be("TagName");
        properties[0].Type.Should().Be(typeof(TagName));
        properties[0].Group.Should().Be(TypeGroup.Text);

        properties[1].Name.Should().Be("Description");
        properties[1].Type.Should().Be(typeof(string));
        properties[1].Group.Should().Be(TypeGroup.Text);

        properties[2].Name.Should().Be("Value");
        properties[2].Type.Should().Be(typeof(LogixData));
        properties[2].Group.Should().Be(TypeGroup.Number);
    }

    [Test]
    public void Returns_MultipleNestedProperties_PropertiesShouldHaveExpectedInstances()
    {
        var step = new Select("TagName.Member", "Members[0].Description", "[PRE].Value");

        var result = step.Returns(Element.Tag.This);

        var properties = result.Properties.ToList();

        properties[0].Name.Should().Be("Member");
        properties[0].Type.Should().Be(typeof(string));
        properties[0].Group.Should().Be(TypeGroup.Text);

        properties[1].Name.Should().Be("Description");
        properties[1].Type.Should().Be(typeof(string));
        properties[1].Group.Should().Be(TypeGroup.Text);

        properties[2].Name.Should().Be("Value");
        properties[2].Type.Should().Be(typeof(LogixData));
        properties[2].Group.Should().Be(TypeGroup.Number);
    }

    [Test]
    public void Process_Default_ShouldBeExpected()
    {
        var step = new Select();
        var input = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", new STRING("This is a value"))
        };

        var results = step.Process(input).ToList();

        results.Should().HaveCount(3);
        results.Should().AllBeOfType<Tag>();
        results.Should().BeEquivalentTo(input);
    }

    [Test]
    public void Process_SimpleProperty_ShouldBeExpected()
    {
        var step = new Select("Value");
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
        var step = new Select("Parent");
        var input = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", new STRING("This is a value"))
        }.SelectMany(t => t.Members());

        var results = step.Process(input).ToList();

        results.Should().HaveCount(8);
        results.Where(t => t is not null).Cast<Tag>().Should().AllSatisfy(t => t.Name.Should().Be("AnotherTag"));
    }

    [Test]
    public void Process_MembersProperty_ShouldBeExpected()
    {
        var step = new Select("Members");
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
        var step = new Select("TagName.Operand");
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
    public void Process_MultipleTagSimpleProperties_ShouldReturnExpectedValues()
    {
        var step = new Select("TagName", "Description", "Value");
        var input = new List<Tag>
        {
            new("TestTag", 123) { Description = "This is a simple test" },
            new("AnotherTag", new TIMER()) { Description = "This is a complex test" },
            new("MyTestTag", new STRING("This is a value")) { Description = "This is a string test" }
        };

        var result = step.Process(input).ToList();

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<ExpandoObject>();

        foreach (var item in result)
        {
            item.As<IDictionary<string, object>>().Keys.Should().ContainInOrder(["TagName", "Description", "Value"]);
        }
    }

    [Test]
    public void Process_SuccessiveSelectSteps_ShouldReturnExpectedValues()
    {
        var first = new Select("Members");
        var second = new Select("TagName");
        var input = new List<Tag>
        {
            new("TestTag", 123) { Description = "This is a simple test" },
            new("AnotherTag", new TIMER()) { Description = "This is a complex test" },
            new("MyTestTag", new STRING("This is a value")) { Description = "This is a string test" }
        };

        var members = first.Process(input).ToList();

        members.Should().AllBeOfType<Tag>();

        var names = second.Process(members).ToList();
        names.Should().AllBeOfType<TagName>();

        names.Should().Contain("TestTag");
        names.Should().Contain("AnotherTag");
        names.Should().Contain("AnotherTag.DN");
        names.Should().Contain("AnotherTag.PRE");
        names.Should().Contain("AnotherTag.ACC");
        names.Should().Contain("MyTestTag");
    }

    [Test]
    public void Process_MultipleTagComplexProperties_ShouldReturnExpectedValues()
    {
        var step = new Select("TagName.Member", "Members[0].Description", "[PRE].Value");
        var input = new List<Tag>
        {
            new("MyTagName", new TIMER { PRE = 1234 }) { Description = "This is a complex test" },
        };

        var result = step.Process(input).ToList();

        result.Should().HaveCount(1);
        result.Should().AllBeOfType<ExpandoObject>();
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
        var step = new Select("Radix.Value");

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_MultiplePropertis_ShouldBeVerified()
    {
        var step = new Select("TagName", "Radix", "Description", "Parent.Parent.Value", "Members");

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ProeprtiesWithAlias_ShouldBeVerified()
    {
        var step = new Select();
        step.Selections.Add(new Selection("TagName.Member", "MemberName"));
        step.Selections.Add(new Selection("Parent.Parent.Value", "Testing"));
        step.Selections.Add(new Selection("[PRE].Value", "Preset"));

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
        var step = new Select("MainRoutineName");
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Select>(json);

        result.Should().BeEquivalentTo(step);
    }

    [Test]
    public void Deserialize_ConfiguredAsStep_ShouldBeExpected()
    {
        var expected = new Select("Rate");
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Select>(json);

        result.Should().BeEquivalentTo(expected);
    }
}