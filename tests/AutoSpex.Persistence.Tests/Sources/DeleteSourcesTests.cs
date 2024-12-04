namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class DeleteSourcesTests
{
    [Test]
    public async Task DeleteSources_NonExistingSources_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();

        var result = await mediator.Send(new DeleteSources([source.SourceId]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteSources_SeededSources_ShouldReturnSuccessAndGettingShouldFail()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var first = new Source("First");
        var second = new Source("Second");
        var third = new Source("Thrid");
        await mediator.Send(new CreateSource(first));
        await mediator.Send(new CreateSource(second));
        await mediator.Send(new CreateSource(third));

        var deleted = await mediator.Send(new DeleteSources(
        [
            first.SourceId, second.SourceId, third.SourceId
        ]));

        deleted.IsSuccess.Should().BeTrue();
    }
}