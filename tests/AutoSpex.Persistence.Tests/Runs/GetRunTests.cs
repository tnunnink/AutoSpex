namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class GetRunTests
{
    [Test]
    public async Task GetRun_NoSpecExists_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetRun(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetRun_SeededSpec_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = Node.NewRun();
        await mediator.Send(new CreateNode(run));

        var result = await mediator.Send(new GetRun(run.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.RunId.Should().Be(run.NodeId);
        result.Value.Sources.Should().BeEmpty();
        result.Value.Specs.Should().BeEmpty();
    }
}