namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class ListSpecsIsTests
{
    [Test]
    public async Task ListSpecsIn_NonExistent_ShouldBeEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListSpecsIn(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListSpecsIn_SingleSpec_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var root = Node.NewContainer();
        var folder = root.AddContainer();
        var node = folder.AddSpec();
        var spec = new Spec(node).Search(Element.Tag);
        await mediator.Send(new CreateNode(root));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(node));
        await mediator.Send(new SaveSpec(spec));

        var result = await mediator.Send(new ListSpecsIn(spec.SpecId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}