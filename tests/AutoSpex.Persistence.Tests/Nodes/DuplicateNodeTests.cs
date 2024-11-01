namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class DuplicateNodeTests
{
    [Test]
    public async Task DuplicateNode_NodeNotFound_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new DuplicateNode(Guid.NewGuid(), "NewNode"));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task DuplicateNode_WithSeededSingleNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var expected = Node.NewCollection();
        await mediator.Send(new CreateNode(expected));

        var result = await mediator.Send(new DuplicateNode(expected.NodeId, "Duplicate Node"));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DuplicateNode_WithSeededSingleNode_ShouldReturnExpectedNode()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var expected = Node.NewCollection();
        await mediator.Send(new CreateNode(expected));

        var result = await mediator.Send(new DuplicateNode(expected.NodeId, "Duplicate Node"));

        result.Value.Name.Should().Be("Duplicate Node");
        result.Value.NodeId.Should().NotBe(expected.NodeId);
        result.Value.ParentId.Should().Be(expected.ParentId);
        result.Value.Spec.Should().NotBeNull();
    }

    [Test]
    public async Task DuplicateNode_WithSeededSingleNode_ShouldHaveExpectedNodeCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var expected = Node.NewCollection();
        await mediator.Send(new CreateNode(expected));

        await mediator.Send(new DuplicateNode(expected.NodeId, "Duplicate Node"));

        var nodes = await mediator.Send(new ListNodes());
        nodes.Should().HaveCount(2);
    }

    [Test]
    public async Task DuplicateNode_SingleSpecConfigured_ShouldHaveExpectedConfig()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var expected = Node.NewSpec("Test", s =>
        {
            s.Query(Element.Tag);
            s.Filter("TagName", Operation.EqualTo, "Something");
            s.Verify("Value", Operation.GreaterThan, 123);
        });
        await mediator.Send(new CreateNode(expected));

        var result = await mediator.Send(new DuplicateNode(expected.NodeId, "Duplicate Spec"));

        var config = result.Value.Spec;
        config.SpecId.Should().NotBe(expected.Spec.SpecId);
        config.Filters.Should().BeEquivalentTo(expected.Spec.Filters);
        config.Verifications.Should().BeEquivalentTo(expected.Spec.Verifications);
        config.Element.Should().BeEquivalentTo(expected.Spec.Element);
        config.FilterInclusion.Should().Be(expected.Spec.FilterInclusion);
        config.VerificationInclusion.Should().Be(expected.Spec.VerificationInclusion);
    }
    
    [Test]
    public async Task DuplicateNode_MultipleNestedSpecsInCollection_ShouldHaveExpectedNodeCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        var spec01 = collection.AddSpec();
        var spec02 = collection.AddSpec();
        var spec03 = collection.AddSpec();
        await mediator.Send(new CreateNodes([collection, spec01, spec02, spec03]));

        var result = await mediator.Send(new DuplicateNode(collection.NodeId, "Duplicate"));
        result.IsSuccess.Should().BeTrue();

        var nodes = await mediator.Send(new ListNodes());
        nodes.SelectMany(n => n.DescendantsAndSelf()).Should().HaveCount(8);
    }
}