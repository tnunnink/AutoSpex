using AutoSpex.Engine;
using FluentAssertions;
using MediatR;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class RemoveProjectTests
{
    [Test]
    public async Task Send_NoProjects_ShouldReturnIsFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var project = new Project(context.ProjectPath);
        var request = new RemoveProject(project);
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Successes.First().Message.Should().Be("Removed 0 project(s) from application store.");
    }

    [Test]
    public async Task Send_InvalidLocation_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var project = new Project(context.ProjectPath);
        var createRequest = new CreateProject(project);
        var createResult = await mediator.Send(createRequest);
        createResult.IsSuccess.Should().BeTrue();
        
        var removeRequest = new RemoveProject(project);
        var removeResult = await mediator.Send(removeRequest);

        removeResult.IsSuccess.Should().BeTrue();
        removeResult.Successes.First().Message.Should().Be("Removed 1 project(s) from application store.");
    }
}