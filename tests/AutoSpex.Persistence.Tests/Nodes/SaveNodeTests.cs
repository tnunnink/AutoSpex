using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class SaveNodeTests
{
    [Test]
    public async Task SaveNode_NoExistingNode_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();

        var result = await mediator.Send(new SaveNode(node));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveNode_SeededNodeNoChanges_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new SaveNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveNode_ConfiguredSpec_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.Configure(c =>
        {
            c.Query(Element.Program);
            c.Filter("Name", Operation.EqualTo, "SomeName");
            c.Verify("Disabled", Operation.False);
        });


        var result = await mediator.Send(new SaveNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveNode_SpecWithReferences_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.Configure(c =>
        {
            c.Query(Element.Program);
            c.Filter("Name", Operation.EqualTo, "SomeName");
            c.Verify("Disabled", Operation.False);
        });

        var result = await mediator.Send(new SaveNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveNode_SpecWithVariables_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.AddVariable(new Variable("bool", true));
        node.AddVariable(new Variable("number", 123));
        node.AddVariable(new Variable("enum", Radix.Decimal));

        var result = await mediator.Send(new SaveNode(node));

        result.IsSuccess.Should().BeTrue();
    }
}