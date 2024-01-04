using AutoSpex.Client.Features.Projects;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Projects;

[TestFixture]
public class GetProjectsTests
{
    [Test]
    public async Task Send_NoProjectFileExists_ShouldReturnIsSuccessAndEmptyCollection()
    {
        using var context = new TestContext();
        var mediator = Resolve<IMediator>();
        var request = new GetProjectsRequest();

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task Send_ProjectFilesExist_ShouldReturnIsSuccessAndNonEmptyCollection()
    {
        using var context = new TestContext();
        var mediator = Resolve<IMediator>();

        var project = new Project(context.ProjectPath);
        var createRequest = new CreateProjectRequest(project);
        var createResult = await mediator.Send(createRequest);
        createResult.IsSuccess.Should().BeTrue();
        
        var getRequest = new GetProjectsRequest();
        var getResult = await mediator.Send(getRequest);

        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().HaveCount(1);
    }
}