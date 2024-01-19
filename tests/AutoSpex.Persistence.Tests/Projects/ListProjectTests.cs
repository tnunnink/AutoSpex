namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class ListProjectTests
{
    [Test]
    public async Task ListProjects_NoProjects_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListProjects());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
    
    [Test]
    public async Task ListProjects_SeededProjects_ShouldReturnSuccessAndNotEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateProject(new Project(context.ProjectPath)));

        var result = await mediator.Send(new ListProjects());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}