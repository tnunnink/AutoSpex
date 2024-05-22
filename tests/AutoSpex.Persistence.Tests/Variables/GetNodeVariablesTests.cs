using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Variables;

[TestFixture]
public class GetNodeVariablesTests
{
    [Test]
    public async Task GetNodeVariables_NoExistingVariablesForNode_ShouldBeSuccessAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetNodeVariables(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetNodeVariables_ExistingVariablesForNode_ShouldBeSuccessAndNotEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));
        var var01 = new Variable(node.NodeId, "Var01", "Test", "This is a test");
        var var02 = new Variable(node.NodeId, "Var02", "Test", "This is a test");
        var var03 = new Variable(node.NodeId, "Var03", "Test", "This is a test");
        await mediator.Send(new SaveVariables(new[] {var01, var02, var03}));

        var result = await mediator.Send(new GetNodeVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().AllSatisfy(v => v.VariableId.Should().NotBeEmpty());
        result.Value.Should().AllSatisfy(v => v.NodeId.Should().Be(node.NodeId));
        result.Value.Should().AllSatisfy(v => v.Name.Should().Contain("Var"));
        result.Value.Should().AllSatisfy(v => v.Type.Should().Be(typeof(string)));
        result.Value.Should().AllSatisfy(v => v.Value.Should().Be("Test"));
        result.Value.Should().AllSatisfy(v => v.Data.Should().Be("Test"));
        result.Value.Should().AllSatisfy(v => v.Description.Should().Be("This is a test"));
    }

    [Test]
    public async Task GetNodeVariables_LogixElementData_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));
        var tag = new Tag {Name = "Test", Value = 100};
        var variable = new Variable(node.NodeId, "TagVariable", tag, "This is a logix component test");
        await mediator.Send(new SaveVariables(new[] {variable}));

        var result = await mediator.Send(new GetNodeVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().AllSatisfy(v => v.VariableId.Should().NotBeEmpty());
        result.Value.Should().AllSatisfy(v => v.NodeId.Should().Be(node.NodeId));
        result.Value.Should().AllSatisfy(v => v.Name.Should().Contain("TagVariable"));
        result.Value.Should().AllSatisfy(v => v.Type.Should().Be(typeof(Tag)));
        result.Value.Should().AllSatisfy(v => v.Value.Should().BeOfType<Tag>());
        result.Value.Should().AllSatisfy(v => v.Value.Should().BeEquivalentTo(tag));
        result.Value.Should().AllSatisfy(v => v.Data.Should().Be(tag.Serialize().ToString()));
        result.Value.Should().AllSatisfy(v => v.Description.Should().Be("This is a logix component test"));
    }
}