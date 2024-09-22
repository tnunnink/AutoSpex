using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class ExportNodeTests
{
    [Test]
    public async Task ExportNode_NoCollection_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ExportNode(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task ExportNode_EmptyCollection_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        await mediator.Send(new CreateNodes(collection.DescendantsAndSelf()));

        var result = await mediator.Send(new ExportNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Collection.Should().BeEquivalentTo(collection);
    }

    [Test]
    public async Task ExportNode_ExistingCollectionWithSpecs_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        collection.AddSpec("First", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Like, "someName");
            s.Verify("Value", Operation.EqualTo, 123);
        });
        collection.AddSpec("Second", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Containing, "anotherName");
            s.Verify("Value", Operation.GreaterThan, 456);
        });
        collection.AddSpec("Third", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.EqualTo, "yetAnotherName");
            s.Verify("Value", Negation.Not, Operation.EqualTo, 678);
            s.VerificationInclusion = Inclusion.Any;
        });
        await mediator.Send(new CreateNodes(collection.DescendantsAndSelf()));

        var result = await mediator.Send(new ExportNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Collection.Should().BeEquivalentTo(collection);
    }

    [Test]
    public async Task ExportNode_ExistingCollectionWithVariables_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        collection.AddVariable("first", 123);
        collection.AddVariable("second", "Another test");
        collection.AddVariable("third", new Tag("MyTag", new DINT(123)));
        await mediator.Send(new CreateNode(collection));

        var result = await mediator.Send(new ExportNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Collection.Should().BeEquivalentTo(collection);
    }

    [Test]
    public async Task ExportNode_NodeWithMultipleSpecsAndVariables_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        collection.AddVariable("first", 123);
        collection.AddVariable("second", "Another test");
        collection.AddVariable("third", new Tag("MyTag", new DINT(123)));
        var spec = collection.AddSpec("Test");
        spec.AddSpec(s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Like, "someName");
            s.Verify("Value", Operation.EqualTo, 123);
        });
        spec.AddSpec(s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Containing, "anotherName");
            s.Verify("Value", Operation.GreaterThan, 456);
        });
        spec.AddSpec(s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.EqualTo, "yetAnotherName");
            s.Verify("Value", Negation.Not, Operation.EqualTo, 678);
            s.VerificationInclusion = Inclusion.Any;
        });
        await mediator.Send(new CreateNodes(collection.DescendantsAndSelf()));

        var result = await mediator.Send(new ExportNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Collection.Should().BeEquivalentTo(collection);
    }
}