using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class OutcomeTests
{
    [Test]
    public void New_NodeOverload_ShouldBeExpected()
    {
        var collection = Node.NewCollection();
        var container = collection.AddContainer();
        var spec = container.AddSpec("MySpec");
        var outcome = new Outcome(spec);

        outcome.SpecId.Should().Be(spec.NodeId);
        outcome.Name.Should().Be("MySpec");
        outcome.Result.Should().Be(ResultState.None);
        outcome.Duration.Should().Be(0);
        outcome.Verifications.Should().BeEmpty();
    }

    [Test]
    public Task Serialize_WithNode_ShouldBeVerified()
    {
        var collection = Node.NewCollection();
        var container = collection.AddContainer();
        var spec = container.AddSpec("MySpec");
        var outcome = new Outcome(spec);

        var data = JsonSerializer.Serialize(outcome);

        return VerifyJson(data);
    }
}