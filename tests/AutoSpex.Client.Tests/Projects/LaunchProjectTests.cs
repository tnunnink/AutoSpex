using AutoSpex.Client.Features.Projects;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Projects;

[TestFixture]
public class LaunchProjectTests
{
    [Test]
    public async Task Valid_WhenCalled_ShouldReturnNotEmptyResults()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new LaunchProjectRequest(context.ProjectPath);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
}