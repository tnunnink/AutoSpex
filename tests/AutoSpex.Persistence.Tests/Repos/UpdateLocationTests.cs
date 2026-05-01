namespace AutoSpex.Persistence.Tests.Repos;

[TestFixture]
public class UpdateLocationTests
{
    [Test]
    public async Task UpdateLocation_NoData_ShouldBeFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new UpdateLocation(Guid.NewGuid(), @"C:\Path\To\Repo"));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task UpdateLocation_SeededRepo_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(@"C:\Does\Not\Matter\Here");
        await mediator.Send(new ConnectRepo(repo));

        var result = await mediator.Send(new UpdateLocation(repo.RepoId, @"C:\Still\Does\Not\Matter"));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task UpdateLocation_RepoWithTargets_ShouldNoLongerHaveTargets()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(Known.Repo);
        await mediator.Send(new ConnectRepo(repo));
        repo.AddTarget(Known.Example);
        await mediator.Send(new UpdateTargets(repo));

        var result = await mediator.Send(new UpdateLocation(repo.RepoId, @"C:\Does\Not\Matter"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Matter");
        result.Value.Targets.Should().BeEmpty();
    }
}