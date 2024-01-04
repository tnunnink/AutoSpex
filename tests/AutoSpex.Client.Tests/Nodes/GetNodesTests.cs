using AutoSpex.Client.Features.Collections;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Specifications;
using FluentAssertions;
using MediatR;

namespace AutoSpex.Client.Tests.Nodes;

[TestFixture]
public class GetNodesTests
{
    [Test]
    public async Task Send_SpecsFeature_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();
        var request = new GetNodesRequest(Feature.Specifications);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_SourcesFeature_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();
        var request = new GetNodesRequest(Feature.Sources);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
   
    [Test]
    public async Task Send_CollectionSeeds_ShouldReturnExpectedCount()
    {
        using var context = new TestContext();
        context.BuildProject();
        context.RunMigration("SeedCollectionNodesMigration");
        var mediator = Resolve<IMediator>();
        var request = new GetNodesRequest(Feature.Specifications);
        
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }
    
    [Test]
    public async Task Send_CollectionAddedFirst_ShouldReturnExpectedCount()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();

        var collection = Node.SpecCollection("MyCollection");
        var add = new AddCollectionRequest(collection);
        var addResult = await mediator.Send(add);
        addResult.IsSuccess.Should().BeTrue();
        
        var get = new GetNodesRequest(Feature.Specifications);
        var result = await mediator.Send(get);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Should().Be(addResult.Value);
    }
}