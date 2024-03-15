using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Nodes;

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
        result.Value.Spec.Should().NotBeNull();
        result.Value.Spec?.Element.Should().Be(Element.Module);
        result.Value.Spec?.Filters.Should().HaveCount(1);
        result.Value.Spec?.Verifications.Should().HaveCount(1);
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

    [Test]
    public async Task GetFullNode_NodeWithParentsAndVariablesAndSpec_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var collection = Node.NewCollection("TestCollection");
        collection.AddVariable(new Variable("Var1", "true"));
        collection.AddVariable(new Variable("Var2", "1000"));
        collection.AddVariable(new Variable("Var3", "TestValue"));
        
        var folder = collection.AddFolder("TestFolder");
        folder.AddVariable(new Variable("Var4", "false"));
        folder.AddVariable(new Variable("Var5", "2000"));
        folder.AddVariable(new Variable("Var6", "TestValue2"));

        var spec = folder.AddSpec("TestSpec", c =>
        {
            c.For(Element.Tag);
            c.Where("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
            c.Verify("Value", Operation.Equal, 100);
        });
        
        await mediator.Send(new SaveNode(collection));
        await mediator.Send(new SaveNode(folder));
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetFullNode(spec.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(spec);
        result.Value.Name.Should().Be("TestSpec");
        result.Value.Variables.Should().HaveCount(0);
        result.Value.Spec.Should().NotBeNull();
        result.Value.Parent.Should().BeEquivalentTo(folder);
        result.Value.Parent?.Variables.Should().HaveCount(3);
        result.Value.Parent?.Parent.Should().BeEquivalentTo(collection);
        result.Value.Parent?.Parent?.Variables.Should().HaveCount(3);
    }

    [Test]
    public async Task GetFullNode_HasFiltersButNoVerification_ShouldReturnSuccessAndExpectedCounts()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var spec = Node.NewSpec("TestSpec", c =>
        {
            c.For(Element.Tag);
            c.Where("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
            c.Where("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
            c.Where("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
        });
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetFullNode(spec.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(spec);
        result.Value.Name.Should().Be("TestSpec");
        result.Value.Spec.Should().NotBeNull();
        result.Value.Spec?.Element.Should().Be(Element.Tag);
        result.Value.Spec?.Filters.Should().HaveCount(3);
        result.Value.Spec?.Verifications.Should().BeEmpty();
    }
    
    [Test]
    public async Task GetFullNode_HasVerificationButNoFilter_ShouldReturnSuccessAndExpectedCounts()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var spec = Node.NewSpec("TestSpec", c =>
        {
            c.For(Element.Tag);
            c.Verify("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
            c.Verify("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
            c.Verify("TagName", Operation.EndsWith, new Variable("Var3", "Something"));
        });
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetFullNode(spec.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(spec);
        result.Value.Name.Should().Be("TestSpec");
        result.Value.Spec.Should().NotBeNull();
        result.Value.Spec?.Element.Should().Be(Element.Tag);
        result.Value.Spec?.Verifications.Should().HaveCount(3);
        result.Value.Spec?.Filters.Should().BeEmpty();
    }
}