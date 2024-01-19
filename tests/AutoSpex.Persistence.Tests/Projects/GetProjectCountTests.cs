namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class GetProjectCountTests
{
    [Test]
    public async Task GetProjectCount_NoProjectFileExists_ShouldReturnIsSuccessAndZero()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetProjectCount());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Test]
    public async Task GetProjectCount_ProjectFilesExist_ShouldReturnIsSuccessAndGreaterThanZero()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var project = new Project(context.ProjectPath);
        await mediator.Send(new CreateProject(project));
        
        var result = await mediator.Send(new GetProjectCount());
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }
}