using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Sources.Requests;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Sources;

[TestFixture]
public class AddSourceTests
{
    [Test]
    public async Task Send_ValidParameters_ShouldReturnSuccessAndValidValue()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddSourceRequest(new Uri(TestL5X), "MySource");

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeEmpty();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("MySource");
        result.Value.NodeType.Should().Be(NodeType.Source);
        result.Value.Ordinal.Should().Be(0);
        result.Value.Description.Should().BeEmpty();
    }
}