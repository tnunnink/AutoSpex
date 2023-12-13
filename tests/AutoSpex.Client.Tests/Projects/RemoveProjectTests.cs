using AutoSpex.Client.Features.Projects;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Projects;

[TestFixture]
public class RemoveProjectTests
{
    [Test]
    public async Task Send_NoProjects_ShouldReturnIsFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var request = new RemoveProjectRequest(context.ProjectPath);
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Successes.First().Message.Should().Be("Removed 0 project(s) from application store.");
    }

    [Test]
    public async Task Send_InvalidLocation_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var createRequest = new CreateProjectRequest(context.ProjectPath);
        var createResult = await mediator.Send(createRequest);
        createResult.IsSuccess.Should().BeTrue();
        
        var removeRequest = new RemoveProjectRequest(context.ProjectPath);
        var removeResult = await mediator.Send(removeRequest);

        removeResult.IsSuccess.Should().BeTrue();
        removeResult.Successes.First().Message.Should().Be("Removed 1 project(s) from application store.");
    }
}