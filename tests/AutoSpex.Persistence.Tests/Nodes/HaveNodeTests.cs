namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class HaveNodeTests
{
    [Test]
    public async Task CollectionExists_NoData_ShouldReturnFalse()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new HaveNode("test", NodeType.Collection));

        result.Should().BeFalse();
    }
    
    [Test]
    public async Task CollectionExists_CollectionExists_ShouldReturnTrue()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewCollection("test")));

        var result = await mediator.Send(new HaveNode("test", NodeType.Collection));

        result.Should().BeTrue();
    }
    
    [Test]
    public async Task CollectionExists_CollectionExistsWithDifferentName_ShouldReturnFalse()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewCollection("testing")));

        var result = await mediator.Send(new HaveNode("test", NodeType.Collection));

        result.Should().BeFalse();
    }
    
    [Test]
    public async Task CollectionExists_MultipleCollectionExistsWithName_ShouldReturnTrue()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewCollection("test")));
        await mediator.Send(new CreateNode(Node.NewCollection("test")));

        var result = await mediator.Send(new HaveNode("test", NodeType.Collection));

        result.Should().BeTrue();
    }
}