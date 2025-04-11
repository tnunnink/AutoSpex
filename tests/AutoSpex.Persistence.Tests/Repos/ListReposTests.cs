namespace AutoSpex.Persistence.Tests.Repos;

[TestFixture]
public class ListReposTests
{
    [Test]
    public async Task ListRepos_NoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListRepos());

        result.Should().BeEmpty();
    }

    [Test]
    public async Task ListRepos_WithSeededData_ShouldHaveExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new ConnectRepo(Repo.Configure("First")));
        await mediator.Send(new ConnectRepo(Repo.Configure("Second")));
        await mediator.Send(new ConnectRepo(Repo.Configure("Third")));

        var result = await mediator.Send(new ListRepos());

        result.Should().HaveCount(3);
    }
    
    [Test]
    public async Task ListRepos_WithSeededData_ShouldHaveExpectedValues()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new ConnectRepo(Repo.Configure("First")));
        await mediator.Send(new ConnectRepo(Repo.Configure("Second")));
        await mediator.Send(new ConnectRepo(Repo.Configure("Third")));

        var result = (await mediator.Send(new ListRepos())).ToArray();

        result[0].RepoId.Should().NotBeEmpty();
        result[0].Location.Should().Be("First");
        result[0].Name.Should().Be("First");
        
        result[1].RepoId.Should().NotBeEmpty();
        result[1].Location.Should().Be("Second");
        result[1].Name.Should().Be("Second");
        
        result[2].RepoId.Should().NotBeEmpty();
        result[2].Location.Should().Be("Third");
        result[2].Name.Should().Be("Third");
    }
}