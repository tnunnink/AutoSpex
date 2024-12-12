// ReSharper disable UseObjectOrCollectionInitializer

using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class QueryTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var query = new Query();

        query.Element.Should().Be(Element.Default);
        query.Steps.Should().BeEmpty();
    }

    [Test]
    public void New_Element_ShouldBeExpected()
    {
        var query = new Query(Element.Task);

        query.Element.Should().Be(Element.Task);
        query.Returns.Should().Be(Element.Task.This);
    }

    [Test]
    public void New_ElementAndSteps_ShouldBeExpected()
    {
        var query = new Query(Element.Tag, [new Filter("Name", Operation.Containing, "Test")]);

        query.Element.Should().Be(Element.Tag);
        query.Steps.Should().ContainSingle().Which.Should().BeOfType<Filter>();
    }

    [Test]
    public void Element_SetValue_ShouldBeExpected()
    {
        var query = new Query();

        query.Element = Element.Controller;

        query.Element.Should().Be(Element.Controller);
    }

    [Test]
    public void Returns_WithMultipleSteps_ShouldBeExpected()
    {
        var query = new Query(Element.Tag);
        query.Steps.Add(new Filter());
        query.Steps.Add(new Select("TagName"));
        query.Steps.Add(new Filter());

        var returns = query.Returns;

        returns.Should().Be(Property.This(typeof(TagName)));
    }

    [Test]
    public void AddStep_ValidStep_ShouldBeExpected()
    {
        var query = new Query(Element.Tag);

        query.Steps.Add(new Filter());

        query.Steps.Should().HaveCount(1);
    }

    [Test]
    public void Execute_TagElement_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query(Element.Tag);

        var result = query.Execute(content).ToList();

        result.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_TagElementWithFilterStep_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query(Element.Tag);
        var filter = new Filter("Name", Operation.Containing, "Test");
        query.Steps.Add(filter);

        var result = query.Execute(content).ToList();

        result.Should().NotBeEmpty();
        result.Should().AllBeOfType<Tag>();
        result.Cast<Tag>().Should().AllSatisfy(t => t.Name.Should().Contain("Test"));
    }

    [Test]
    public void Execute_ProgramElement_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query(Element.Program);

        var result = query.Execute(content).ToList();

        result.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_RungElement_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query(Element.Rung);

        var result = query.Execute(content).ToList();

        result.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_TagWithManySteps_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query(Element.Tag);
        query.Steps.Add(new Filter("Name", Operation.Containing, "Test"));
        query.Steps.Add(new Select("Members"));
        query.Steps.Add(new Select("TagName"));
        query.Steps.Add(new Filter("Depth", Operation.GreaterThan, 2));

        var result = query.Execute(content).ToList();

        result.Should().NotBeEmpty();
        result.Should().AllBeOfType<TagName>();
        result.Cast<TagName>().Should().AllSatisfy(t => t.ToString().Should().Contain("Test"));
    }

    [Test]
    public void ExecuteTo_TagWithManySteps_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query(Element.Tag);
        query.Steps.Add(new Filter("Name", Operation.Containing, "Test"));
        query.Steps.Add(new Select("Members"));
        query.Steps.Add(new Select("TagName"));
        query.Steps.Add(new Filter("Depth", Operation.GreaterThan, 2));

        var result = query.ExecuteTo(content, 2).ToList();

        result.Should().NotBeEmpty();
        result.Should().AllBeOfType<Tag>();
        result.Cast<Tag>().Should().AllSatisfy(t => t.TagName.ToString().Should().Contain("Test"));
    }

    [Test]
    public Task Serialize_DefaultInstance_ShouldBeVerified()
    {
        var query = new Query();

        var json = JsonSerializer.Serialize(query);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredWithElement_ShouldBeVerified()
    {
        var query = new Query();
        query.Element = Element.Controller;

        var json = JsonSerializer.Serialize(query);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredWithFilterStep_ShouldBeVerified()
    {
        var query = new Query(Element.Tag);
        var filter = new Filter("Name", Operation.Containing, "Test");
        query.Steps.Add(filter);

        var json = JsonSerializer.Serialize(query);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredWithSteps_ShouldBeVerified()
    {
        var query = new Query(Element.Tag);
        var filter = new Filter("Name", Operation.Containing, "Test");
        query.Steps.Add(filter);
        query.Steps.Add(new Select("Description"));

        var json = JsonSerializer.Serialize(query);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_Default_ShouldBeExpected()
    {
        var query = new Query();
        var json = JsonSerializer.Serialize(query);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(query);
    }

    [Test]
    public void Deserialize_Configured_ShouldBeExpected()
    {
        var query = new Query(Element.Controller);
        var json = JsonSerializer.Serialize(query);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(query);
    }

    [Test]
    public void Deserialize_ConfiguredFilterStep_ShouldBeVerified()
    {
        var query = new Query(Element.Tag);
        var filter = new Filter("Name", Operation.Containing, "Test");
        query.Steps.Add(filter);
        var json = JsonSerializer.Serialize(query);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(query);
    }

    [Test]
    public void Deserialize_ConfiguredWithSteps_ShouldBeVerified()
    {
        var query = new Query(Element.Tag);
        var filter = new Filter("Name", Operation.Containing, "Test");
        query.Steps.Add(filter);
        query.Steps.Add(new Select("Description"));
        var json = JsonSerializer.Serialize(query);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(query);
    }
}