namespace AutoSpex.Persistence.Tests.Repos;

[TestFixture]
public class GetLastConnectedTests
{
    [Test]
    public async Task GetLastConnected_NoData_ShouldBeFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetLastConnected());

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetLastConnected_SeededRepo_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(@"C:\Does\Not\Matter\Here");
        await mediator.Send(new ConnectRepo(repo));

        var result = await mediator.Send(new GetLastConnected());

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetLastConnected_MultipleRepos_ShouldBeExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\First")));
        await Task.Delay(1000);
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\Second")));

        var result = await mediator.Send(new GetLastConnected());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Second");
    }
    
    [Test]
    public async Task GetLastConnected_LocationAlreadyExists_ShouldBeTheLastConnected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\First")));
        await Task.Delay(1000);
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\Second")));
        await Task.Delay(1000);
        await mediator.Send(new ConnectRepo(Repo.Configure(@"C:\First")));

        var result = await mediator.Send(new GetLastConnected());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("First");
    }
}