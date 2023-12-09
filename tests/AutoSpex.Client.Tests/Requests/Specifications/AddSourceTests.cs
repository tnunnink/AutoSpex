using AutoSpex.Client.Features.Sources.Requests;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Requests.Specifications;

[TestFixture]
public class AddSourceTests
{
    [Test]
    public async Task AddSource_ValidSourceRequest_ShouldUpdateDatabase()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        
        var request = new AddSourceRequest(new Uri(TestL5X), "Source Test");

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.Name.Should().Be("Source Test");
        result.Value.ParentId.Should().BeNull();
        result.Value.Parent.Should().BeNull();
    }
}