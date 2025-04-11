namespace AutoSpex.Persistence.Tests.Repos;

public class RemoveReposTests
{
    [Test]
    public async Task RemoveRepo_NoneExist_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new RemoveRepos([Repo.Configure("Test")]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task RemoveRepo_Exists_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var repo = Repo.Configure("Test");
        await mediator.Send(new ConnectRepo(repo));

        var result = await mediator.Send(new RemoveRepos([repo]));

        result.IsSuccess.Should().BeTrue();
    }
}