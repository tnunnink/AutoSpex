using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Specifications;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Specifications;

[TestFixture]
public class AddCollectionTests
{
    [Test]
    public async Task Valid_WhenCalled_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddCollectionRequest("MyCollection");
        
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeEmpty();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("MyCollection");
        result.Value.Feature.Should().Be(Feature.Specifications);
        result.Value.NodeType.Should().Be(NodeType.Collection);
        result.Value.Depth.Should().Be(0);
        result.Value.Ordinal.Should().Be(0);
        result.Value.Description.Should().BeEmpty();
    }
}