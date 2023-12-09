using AutoSpex.Client.Features.Nodes;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Requests.Nodes;

[TestFixture]
public class GetNodesTests
{
    [Test]
    public async Task Send_SpecTypeNoData_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new GetNodesRequest(NodeType.Spec);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_CollectionTypeNoData_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new GetNodesRequest(NodeType.Collection);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_FolderTypeNoData_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new GetNodesRequest(NodeType.Folder);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_SourceTypeNoData_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = context.Resolve<IMediator>();
        var request = new GetNodesRequest(NodeType.Source);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Send_SourceWithSeededNodes_ShouldReturnExpectedCount()
    {
        using var context = new TestContext();
        context.BuildProject();
        context.RunMigration("SeedSourceNodesMigration");
        var mediator = context.Resolve<IMediator>();
        
        var request = new GetNodesRequest(NodeType.Source);
        
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }
}