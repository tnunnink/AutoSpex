﻿namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class GetNodesTests
{
    [Test]
    public async Task Send_NoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetNodes());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task Send_WithSeededNodes_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewCollection()));
        await mediator.Send(new CreateNode(Node.NewFolder()));
        await mediator.Send(new CreateNode(Node.NewSpec()));

        var result = await mediator.Send(new GetNodes());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task Send_WithSeededHierarchy_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(collection.AddFolder()));
        await mediator.Send(new CreateNode(collection.AddSpec()));

        var result = await mediator.Send(new GetNodes());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().NodeType.Should().Be(NodeType.Collection);
        result.Value.First().Nodes.Should().HaveCount(2);
    }
}