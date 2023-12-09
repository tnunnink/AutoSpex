using AutoSpex.Client.Features.Projects;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Requests.Projects;

[TestFixture]
public class GetProjectsTests
{
    [Test]
    public async Task Valid_WhenCalled_ShouldReturnNotEmptyResults()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var request = new GetProjectsRequest();

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_NoProjectFileExists_IsFailedShouldBeTrue()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var request = new GetProjectsRequest();

        var result = await mediator.Send(request);

        result.IsFailed.Should().BeTrue();
    }
}