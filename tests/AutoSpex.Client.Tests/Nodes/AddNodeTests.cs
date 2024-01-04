using AutoSpex.Client.Features.Collections;
using AutoSpex.Client.Features.Nodes;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Nodes;

[TestFixture]
public class AddNodeTests
{
    [Test]
    public async Task AddNodeRequest_SpecCollectionNode_ShouldReturnExpectedResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();
        
        var collection = Node.SpecCollection("MyCollection");
        
        var result = await mediator.Send(new AddNodeRequest(collection));

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeEmpty();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("MyCollection");
        result.Value.Feature.Should().Be(Feature.Specifications);
        result.Value.NodeType.Should().Be(NodeType.Collection);
        result.Value.Depth.Should().Be(0);
        result.Value.Ordinal.Should().Be(0);
    }

    [Test]
    public async Task AddNodeRequest_SpecNode_ShouldReturnExpectedResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();

        var collection = Node.SpecCollection("MyCollection");
        await mediator.Send(new AddNodeRequest(collection));
        
        var spec = collection.NewSpec("TestSpec");
        var result = await mediator.Send(new AddNodeRequest(spec));

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().NotBeEmpty();
        result.Value.Parent.Should().Be(collection);
        result.Value.Name.Should().Be("TestSpec");
        result.Value.Feature.Should().Be(Feature.Specifications);
        result.Value.NodeType.Should().Be(NodeType.Spec);
        result.Value.Depth.Should().Be(1);
        result.Value.Ordinal.Should().Be(0);
    }
}