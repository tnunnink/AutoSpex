using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class CreateSourceTests
{
    [Test]
    public async Task CreateSource_ValidSourceNoParent_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));

        var result = await mediator.Send(new CreateSource(source));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateSource_ValidSourceNoParent_GetSourceShouldBeExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var expected = new Source(L5X.Load(Known.Test));

        await mediator.Send(new CreateSource(expected));

        var result = await mediator.Send(new GetSource(expected.SourceId));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected, o => o.Excluding(s => s.L5X));
    }

    [Test]
    public async Task CreateSource_ValidSourceWithParentNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("SourceContainer");
        await mediator.Send(new CreateNode(node, NodeType.Source));
        var source = new Source(L5X.Load(Known.Test));

        var result = await mediator.Send(new CreateSource(source, node.NodeId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateSource_ConflictingSourceId_ShouldReturnFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSource("TestSource");
        await mediator.Send(new CreateNode(node));

        var source = new Source(node.NodeId);

        var result = await mediator.Send(new CreateSource(source));
        result.IsFailed.Should().BeTrue();
    }
}