using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class GetSourceTests
{
    [Test]
    public async Task GetSource_NoSource_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetSource(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Source not found");
    }

    [Test]
    public async Task GetSource_SourceNodeExists_ShouldReturnEquivalent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSource();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new GetSource(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.SourceId.Should().Be(node.NodeId);
        result.Value.Name.Should().Be(node.Name);
        result.Value.TargetName.Should().BeEmpty();
        result.Value.TargetType.Should().BeEmpty();
        result.Value.ExportedOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        result.Value.ExportedBy.Should().BeEmpty();
        result.Value.Content.Should().BeEmpty();
    }

    [Test]
    public async Task GetSource_WithSourceContent_ShouldHaveNonNullContent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSource();
        await mediator.Send(new CreateNode(node));
        var source = new Source(node);
        source.Update(L5X.Load(Known.Test), true);
        await mediator.Send(new SaveSource(source));

        var result = await mediator.Send(new GetSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().NotBeNull();
        result.Value.L5X.Should().NotBeNull();
    }
}