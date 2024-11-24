// ReSharper disable UseObjectOrCollectionInitializer

using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests.Steps;

[TestFixture]
public class QueryTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var step = new Query();

        step.Element.Should().Be(Element.Default);
        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Element_SetValue_ShouldBeExpected()
    {
        var step = new Query();
        
        step.Element = Element.Controller;

        step.Element.Should().Be(Element.Controller);
    }

    [Test]
    public void Process_TagElement_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query { Element = Element.Tag };
        
        var result = query.Process(new List<object> { content }).ToList();
        
        result.Should().NotBeEmpty();
    }
    
    [Test]
    public void Process_ProgramElement_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query { Element = Element.Program };
        
        var result = query.Process(new List<object> { content }).ToList();
        
        result.Should().NotBeEmpty();
    }
    
    [Test]
    public void Process_RungElement_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var query = new Query { Element = Element.Rung };
        
        var result = query.Process(new List<object> { content }).ToList();
        
        result.Should().NotBeEmpty();
    }
    
    [Test]
    public Task Serialize_DefaultInstance_ShouldBeVerified()
    {
        var step = new Query();

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_Configured_ShouldBeVerified()
    {
        var step = new Query();
        step.Element = Element.Controller;

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_Default_ShouldBeExpected()
    {
        var step = new Query();
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(step);
    }

    [Test]
    public void Deserialize_Configured_ShouldBeExpected()
    {
        var step = new Query();
        step.Element = Element.Controller;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(step);
    }

    [Test]
    public void Deserialize_ConfiguredAsStep_ShouldBeExpected()
    {
        var expected = new Query();
        expected.Element = Element.Controller;
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Query>(json);

        result.Should().BeEquivalentTo(expected);
    }
}