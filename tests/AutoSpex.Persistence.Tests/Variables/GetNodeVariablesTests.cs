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
        var var01 = new Variable("Var01", "Test");
        var var02 = new Variable("Var02", "Test");
        var var03 = new Variable("Var03", "Test");
        await mediator.Send(new SaveVariables(node.NodeId, new[] { var01, var02, var03 }));

        var result = await mediator.Send(new GetNodeVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().AllSatisfy(v => v.VariableId.Should().NotBeEmpty());
        result.Value.Should().AllSatisfy(v => v.Name.Should().Contain("Var"));
        result.Value.Should().AllSatisfy(v => v.Type.Should().Be(typeof(string)));
        result.Value.Should().AllSatisfy(v => v.Value.Should().Be("Test"));
    }

    [Test]
    public async Task GetNodeVariables_LogixElementData_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));
        var tag = new Tag { Name = "Test", Value = 100 };
        var variable = new Variable("TagVariable", tag);
        await mediator.Send(new SaveVariables(node.NodeId, new[] { variable }));

        var result = await mediator.Send(new GetNodeVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().AllSatisfy(v => v.VariableId.Should().NotBeEmpty());
        result.Value.Should().AllSatisfy(v => v.Name.Should().Contain("TagVariable"));
        result.Value.Should().AllSatisfy(v => v.Type.Should().Be(typeof(Tag)));
        result.Value.Should().AllSatisfy(v => v.Value.Should().BeOfType<Tag>());
        result.Value.Should().AllSatisfy(v => v.Value.Should().BeEquivalentTo(tag));
    }
}