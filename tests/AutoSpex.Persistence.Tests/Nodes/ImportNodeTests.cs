namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class ImportNodeTests
{
    [Test]
    public async Task ImportNode_SimpleCollectionNoConflict_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var collection = Node.NewCollection();
        var package = new Package(collection, 10000);
        
        var result = await mediator.Send(new ImportNode(package, ImportAction.None));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task ImportNode_SimpleCollectionCopyAction_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var collection = Node.NewCollection();
        var package = new Package(collection, 10000);
        
        var result = await mediator.Send(new ImportNode(package, ImportAction.Copy));
        result.IsSuccess.Should().BeTrue();

        var export = await mediator.Send(new ExportNode(result.Value.NodeId));
        export.Value.Collection.Should().NotBeNull();
    }

    [Test]
    public async Task ImportNode_ConfiguredCollectionNoConflict_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        collection.AddVariable("Test1", 123);
        collection.AddVariable("Test1", "This is a variale");
        collection.AddSpec("Test", s =>
        {
            s.Find(Element.Tag);
            s.Filter("TagName", Operation.EqualTo, "TestTag");
            s.Verify("Value", Operation.GreaterThan, "TestTag");
        });
        var package = new Package(collection, 10000);
        
        var result = await mediator.Send(new ImportNode(package, ImportAction.None));
        result.IsSuccess.Should().BeTrue();
        
        var export = await mediator.Send(new ExportNode(collection.NodeId));
        export.Value.Collection.Should().BeEquivalentTo(collection);
    }
}