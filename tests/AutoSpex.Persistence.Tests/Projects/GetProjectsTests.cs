using AutoSpex.Engine;
using FluentAssertions;
using MediatR;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class GetProjectsTests
{
    [Test]
    public async Task Send_NoProjectFileExists_ShouldReturnIsSuccessAndEmptyCollection()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var request = new ListProjects();

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task Send_ProjectFilesExist_ShouldReturnIsSuccessAndNonEmptyCollection()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var project = new Project(context.ProjectPath);
        var createRequest = new CreateProject(project);
        var createResult = await mediator.Send(createRequest);
        createResult.IsSuccess.Should().BeTrue();
        
        var getRequest = new GetProjectCount();
        var getResult = await mediator.Send(getRequest);
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().Be(1);
    }
}