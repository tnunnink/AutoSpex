namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class OpenProjectTests
{
    [Test]
    public async Task OpenProject_DoesNotExist_ShouldReturnSuccessAndBeInDatabase()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var project = new Project(context.ProjectPath);
        
        var result = await mediator.Send(new OpenProject(project));
        result.IsSuccess.Should().BeTrue();

        var expected = await mediator.Send(new ListProjects());
        expected.Value.Should().HaveCount(1);
    }

    [Test]
    public async Task OpenProject_DoesExist_ShouldReturnSuccessAndHaveUpdatedOpenedOn()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var project = new Project(context.ProjectPath);
        
        var result = await mediator.Send(new OpenProject(project));

        result.IsSuccess.Should().BeTrue();
        project.OpenedOn.Should().BeWithin(TimeSpan.FromSeconds(1));
    }
}