using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class GetFullNodeTests
{
    [Test]
    public async Task GetFullNode_EmptyGuid_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetFullNode(Guid.Empty));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetFullNode_NoRecordForGuid_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetFullNode(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetFullNode_CollectionNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var seed = Node.NewCollection("TestCollection");
        await mediator.Send(new CreateNode(seed));

        var result = await mediator.Send(new GetFullNode(seed.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(seed);
    }

    [Test]
    public async Task GetFullNode_FolderNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var seed = Node.NewFolder("Test");
        await mediator.Send(new CreateNode(seed));

        var result = await mediator.Send(new GetFullNode(seed.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(seed);
    }

    [Test]
    public async Task GetFullNode_SpecNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var seed = Node.NewSpec("Test");
        await mediator.Send(new CreateNode(seed));

        var result = await mediator.Send(new GetFullNode(seed.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(seed);
    }

    [Test]
    public async Task GetFullNode_ConfiguredSpecNode_ShouldReturnSuccessAndExpectedValues()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var seed = Node.NewSpec("Test");
        seed.Configure(s =>
        {
            s.For(Element.Module); 
            s.Where(nameof(Module.ParentModule), Operation.Contains, "someName");
            s.Verify(nameof(Module.Name), Operation.Equal, "Test");
        });
        
        await mediator.Send(new CreateNode(seed));

        var result = await mediator.Send(new GetFullNode(seed.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(seed);
    }

    [Test]
    public async Task GetFullNode_NodeWithVariables_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var seed = Node.NewCollection("TestCollection");
        seed.AddVariable(new Variable("Var1", "true"));
        seed.AddVariable(new Variable("Var2", "1000"));
        seed.AddVariable(new Variable("Var3", "TestValue"));
        await mediator.Send(new SaveNode(seed));

        var result = await mediator.Send(new GetFullNode(seed.NodeId));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(seed);
        result.Value.Name.Should().Be("TestCollection");
        result.Value.Variables.Should().HaveCount(3);
    }
}