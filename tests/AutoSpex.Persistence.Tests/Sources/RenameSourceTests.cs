namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class RenameSourceTests
{
    [Test]
    public async Task RenameSource_NoData_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();

        var result = await mediator.Send(new RenameSource(source));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task RenameSource_SeededSource_ShouldReturnSuccessAndExpectedName()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        source.Name = "NewName";

        var result = await mediator.Send(new RenameSource(source));
        result.IsSuccess.Should().BeTrue();

        var get = await mediator.Send(new LoadSource(source.SourceId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Name.Should().Be("NewName");
    }
}