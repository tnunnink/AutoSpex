using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class CreateNodeTests
{
    [Test]
    public async Task CreateNode_CollectionNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection("Test");

        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNode_ContainerNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("Test");

        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNode_SpecNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("MySpec");

        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNode_MultipleNodes_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();

        var collectionResult = await mediator.Send(new CreateNode(collection));
        collectionResult.IsSuccess.Should().BeTrue();

        var folderResult = await mediator.Send(new CreateNode(folder));
        folderResult.IsSuccess.Should().BeTrue();

        var specResult = await mediator.Send(new CreateNode(spec));
        specResult.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNode_SpecConfigured_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("MySpec", c =>
        {
            c.Find(Element.Tag);
            c.Filter("TagName", Operation.Containing, "Test");
            c.Verify("Value", Operation.EqualTo, 123);
            c.FilterInclusion = Inclusion.Any;
        });

        var result = await mediator.Send(new CreateNode(node));
        result.IsSuccess.Should().BeTrue();
        
        var created = await mediator.Send(new LoadNode(node.NodeId));
        created.IsSuccess.Should().BeTrue();
        created.Value.Specs.Should().HaveCount(1);
    }

    [Test]
    public async Task CreateNode_WithVariables_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        node.AddVariable("Value1", 123);
        node.AddVariable("Value2", Radix.Decimal);
        node.AddVariable("Value3", "This is a test");

        var result = await mediator.Send(new CreateNode(node));
        result.IsSuccess.Should().BeTrue();

        var created = await mediator.Send(new LoadNode(node.NodeId));
        created.IsSuccess.Should().BeTrue();
        created.Value.Should().NotBeNull();
        created.Value.Variables.Should().HaveCount(3);
    }
}