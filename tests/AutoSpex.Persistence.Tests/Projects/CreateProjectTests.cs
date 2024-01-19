using AutoSpex.Engine;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class CreateProjectTests
{
    [Test]
    public async Task CreateProject_ValidRequest_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var project = new Project(context.ProjectPath);
        
        var result = await mediator.Send(new CreateProject(project));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateProject_InvalidLocation_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var project = new Project(new Uri(@"D:\Files\Proejcts"));
        
        var result = await mediator.Send(new CreateProject(project));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
    }
}