using AutoSpex.Client.Features.Nodes;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Requests.Nodes;

[TestFixture]
public class AddNodeTests
{
    [Test]
    public async Task Send_ValidCollectionNodeType_ShouldReturnSuccessAndValidValue()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddNodeRequest("Test", NodeType.Collection);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeNull();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("Test");
        result.Value.NodeType.Should().Be(NodeType.Collection);
        result.Value.Ordinal.Should().Be(0);
        result.Value.Description.Should().BeEmpty();
    }
    
    [Test]
    public async Task Send_ValidFolderNodeType_ShouldReturnSuccessAndValidValue()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddNodeRequest("Test", NodeType.Folder);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeNull();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("Test");
        result.Value.NodeType.Should().Be(NodeType.Folder);
        result.Value.Depth.Should().Be(0);
        result.Value.Ordinal.Should().Be(0);
        result.Value.Description.Should().BeEmpty();
    }
    
    [Test]
    public async Task Send_ValidSpecificationNodeType_ShouldReturnSuccessAndValidValue()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddNodeRequest("Test", NodeType.Spec);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeNull();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("Test");
        result.Value.NodeType.Should().Be(NodeType.Spec);
        result.Value.Depth.Should().Be(0);
        result.Value.Ordinal.Should().Be(0);
        result.Value.Description.Should().BeEmpty();
    }
    
    [Test]
    public async Task Send_ValidSourceNodeType_ShouldReturnSuccessAndValidValue()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new AddNodeRequest("Test", NodeType.Source);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeNull();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("Test");
        result.Value.NodeType.Should().Be(NodeType.Source);
        result.Value.Depth.Should().Be(0);
        result.Value.Ordinal.Should().Be(0);
        result.Value.Description.Should().BeEmpty();
    }
    
    [Test]
    public async Task AddCollectionThenFolderThenSpec_ShouldAllHaveCorrectParents()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        
        var addCollection = new AddNodeRequest("Collection", NodeType.Collection);
        var collectionResult = await mediator.Send(addCollection);
        
        var addFolder = new AddNodeRequest("Folder", NodeType.Folder, collectionResult.Value.NodeId);
        var folderResult = await mediator.Send(addFolder);
        
        var addSpec = new AddNodeRequest("Spec", NodeType.Spec, folderResult.Value.NodeId);
        var specResult = await mediator.Send(addSpec);
        

        collectionResult.IsSuccess.Should().BeTrue();
        folderResult.IsSuccess.Should().BeTrue();
        specResult.IsSuccess.Should().BeTrue();
        
        collectionResult.Value.ParentId.Should().BeNull();
        folderResult.Value.ParentId.Should().Be(collectionResult.Value.NodeId);
        specResult.Value.ParentId.Should().Be(folderResult.Value.NodeId);

        folderResult.Value.Parent.Should().NotBeNull();
        specResult.Value.Parent.Should().NotBeNull();
        
        collectionResult.Value.Name.Should().Be("Collection");
        folderResult.Value.Name.Should().Be("Folder");
        specResult.Value.Name.Should().Be("Spec");
        
        collectionResult.Value.NodeType.Should().Be(NodeType.Collection);
        folderResult.Value.NodeType.Should().Be(NodeType.Folder);
        specResult.Value.NodeType.Should().Be(NodeType.Spec);
        
        collectionResult.Value.Depth.Should().Be(0);
        folderResult.Value.Depth.Should().Be(1);
        specResult.Value.Depth.Should().Be(2);
        
        collectionResult.Value.Ordinal.Should().Be(0);
        folderResult.Value.Ordinal.Should().Be(0);
        specResult.Value.Ordinal.Should().Be(0);
    }
}