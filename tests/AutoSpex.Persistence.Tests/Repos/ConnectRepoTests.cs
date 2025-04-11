namespace AutoSpex.Persistence.Tests.Repos;

[TestFixture]
public class ConnectRepoTests
{
    [Test]
    public async Task ConnectRepo_ValidInstance_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(@"C:\Does\Not\Matter\Here");

        var result = await mediator.Send(new ConnectRepo(repo));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task ConnectRepo_LocationAlreadyExists_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure(@"C:\Does\Not\Matter\Here");

        await mediator.Send(new ConnectRepo(repo));
        var result = await mediator.Send(new ConnectRepo(repo));

        result.IsSuccess.Should().BeTrue();
    }
}