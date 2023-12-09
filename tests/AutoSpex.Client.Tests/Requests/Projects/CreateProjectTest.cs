using AutoSpex.Client.Features.Projects;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Requests.Projects;

[TestFixture]
public class CreateProjectTest
{
   [Test]
    public async Task Valid_WhenCalled_ShouldReturnNotEmptyResults()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var request = new CreateProjectRequest(context.ProjectPath);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
}