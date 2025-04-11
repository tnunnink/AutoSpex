namespace AutoSpex.Persistence.Tests.Repos;

[TestFixture]
public class GetLastConnectedRepoTests
{
    [Test]
    public async Task GetLastConnectedRepo_NoData_ShouldBeFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetLastConnectedRepo());

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetLastConnectedRepo_SeededRepo_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(@"C:\Does\Not\Matter\Here");
        await mediator.Send(new ConnectRepo(repo));

        var result = await mediator.Send(new GetLastConnectedRepo());

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetLastConnectedRepo_MultipleRepos_ShouldBeExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\First")));
        await Task.Delay(1000);
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\Second")));

        var result = await mediator.Send(new GetLastConnectedRepo());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Second");
    }
    
    [Test]
    public async Task GetLastConnectedRepo_LocationAlreadyExists_ShouldBeTheLastConnected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\First")));
        await Task.Delay(1000);
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\Second")));
        await Task.Delay(1000);
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\First")));

        var result = await mediator.Send(new GetLastConnectedRepo());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("First");
    }
}