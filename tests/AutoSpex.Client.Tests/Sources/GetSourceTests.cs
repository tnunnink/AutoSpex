using AutoSpex.Client.Features.Sources.Requests;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Sources;

[TestFixture]
public class GetSourceTests
{
    [Test]
    public async Task Send_ValidSourceExists_ShouldReturnValidResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var addRequest = new AddSourceRequest(new Uri(TestL5X), "MySource");
        var addResult = await mediator.Send(addRequest);

        var request = new GetSourceRequest(addResult.Value.NodeId);
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().NotBeNull();
    }
}