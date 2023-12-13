using AutoSpex.Client.Features.Projects;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Projects;

[TestFixture]
public class CreateProjectTests
{
   [Test]
    public async Task Send_ValidRequest_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var request = new CreateProjectRequest(context.ProjectPath);
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Send_InvalidLocation_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var request = new CreateProjectRequest(new Uri(@"D:\Files\Proejcts"));
        var result = await mediator.Send(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
    }
}