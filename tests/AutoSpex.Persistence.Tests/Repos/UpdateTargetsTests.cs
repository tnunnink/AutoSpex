namespace AutoSpex.Persistence.Tests.Repos;

[TestFixture]
public class UpdateTargetsTests
{
    [Test]
    public async Task UpdateTargets_NoData_ShouldBeFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(Known.Repo);

        var result = await mediator.Send(new UpdateTargets(repo));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task UpdateTargets_SeededRepoNoTargets_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(Known.Repo);
        await mediator.Send(new ConnectRepo(repo));

        var result = await mediator.Send(new UpdateTargets(repo));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task UpdateTargets_SeededRepoWithSingleTarget_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(Known.Repo);
        await mediator.Send(new ConnectRepo(repo));
        repo.AddTarget(Known.Example);
        
        var result = await mediator.Send(new UpdateTargets(repo));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task UpdateTargets_SeededRepoMultipleCalls_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(Known.Repo);
        await mediator.Send(new ConnectRepo(repo));
        repo.AddTarget(Known.Example);
        
        var first = await mediator.Send(new UpdateTargets(repo));
        first.IsSuccess.Should().BeTrue();
        
        var second = await mediator.Send(new UpdateTargets(repo));
        second.IsSuccess.Should().BeTrue();
    }
}