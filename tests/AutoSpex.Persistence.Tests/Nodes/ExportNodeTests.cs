﻿using L5Sharp.Core;
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
            s.Get(Element.Tag);
            s.Where("Name", Operation.Like, "someName");
            s.Validate("Value", Operation.EqualTo, 123);
        });
        collection.AddSpec("Second", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "anotherName");
            s.Validate("Value", Operation.GreaterThan, 456);
        });
        collection.AddSpec("Third", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.EqualTo, "yetAnotherName");
            s.Validate("Value", Negation.Not, Operation.EqualTo, 678);
        });
        await mediator.Send(new CreateNodes(collection.DescendantsAndSelf()));

        var result = await mediator.Send(new ExportNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Collection.Should().BeEquivalentTo(collection);
    }

    [Test]
    public async Task ExportNode_NodeWithMultipleSpecs_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        var spec = collection.AddSpec("Test");
        spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Like, "someName");
            s.Validate("Value", Operation.EqualTo, 123);
        });
        spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "anotherName");
            s.Validate("Value", Operation.GreaterThan, 456);
        });
        spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.EqualTo, "yetAnotherName");
            s.Validate("Value", Negation.Not, Operation.EqualTo, 678);
        });
        await mediator.Send(new CreateNodes(collection.DescendantsAndSelf()));

        var result = await mediator.Send(new ExportNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Collection.Should().BeEquivalentTo(collection);
    }
}