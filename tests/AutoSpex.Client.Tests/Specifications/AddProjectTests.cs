using AutoSpex.Client.Features.Specifications;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Specifications;

[TestFixture]
public class AddProjectTests
{
    [Test]
    public async Task Valid_WhenCalled_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddCollectionRequest("Test Project");

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
}