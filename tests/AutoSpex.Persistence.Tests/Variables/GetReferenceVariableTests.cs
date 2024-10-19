using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Variables;

[TestFixture]
public class GetReferenceVariableTests
{
    [Test]
    public async Task GetReferenceVariable_NoSeededData_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetReferenceVariable(new Reference("Test")));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetReferenceVariable_SingleSeededVariableWithChildSpec_ShouldReturnSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var node = Node.NewContainer();
        node.AddVariable("MyVar", "MyTagName");
        await mediator.Send(new CreateNode(node));

        var reference = new Reference("MyVar");
        var spec = node.AddSpec("Test",
            s => s.Verify("TagName", Operation.Containing, reference)
        );
        await mediator.Send(new CreateNode(spec));

        var result = await mediator.Send(new GetReferenceVariable(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("MyVar");
        result.Value.Value.Should().Be("MyTagName");
    }

    [Test]
    public async Task GetReferenceVariable_SeededVariablesForManyNodes_ShouldReturnSuccessAndExpectedVariable()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        collection.AddVariable("CollectionVar", 123);
        var folder = collection.AddContainer();
        folder.AddVariable("FolderVar", "Test Value");
        var spec = folder.AddSpec();
        spec.AddVariable("SpecVar", TagType.Base);
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));

        var reference = new Reference("SpecVar");
        spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Filter("Name", Operation.EqualTo, "SomeName");
            c.Verify("TagType", Operation.EqualTo, reference);
        });
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetReferenceVariable(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.VariableId.Should().NotBeEmpty();
        result.Value.Name.Should().Be("SpecVar");
        result.Value.Value.Should().Be(TagType.Base);
        result.Value.Type.Should().Be(typeof(TagType));
        result.Value.Group.Should().Be(TypeGroup.Enum);
    }

    [Test]
    public async Task GetReferenceVariable_SeededSameNameVariableForDifferentNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        collection.AddVariable("MyVar01", 123);
        var folder = collection.AddContainer();
        folder.AddVariable("MyVar01", "Test Value");
        var spec = folder.AddSpec();
        spec.AddVariable("MyVar01", TagType.Base);
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));

        var reference = new Reference("MyVar01");
        spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Filter("Name", Operation.EqualTo, "SomeName");
            c.Verify("TagType", Operation.EqualTo, reference);
        });
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetReferenceVariable(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.VariableId.Should().NotBeEmpty();
        result.Value.Name.Should().Be("MyVar01");
        result.Value.Value.Should().Be(TagType.Base);
        result.Value.Type.Should().Be(typeof(TagType));
        result.Value.Group.Should().Be(TypeGroup.Enum);
    }
}