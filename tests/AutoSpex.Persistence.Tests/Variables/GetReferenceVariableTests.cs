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
        var spec = node.AddSpec();
        await mediator.Send(new CreateNode(node));
        await mediator.Send(new CreateNode(spec));
        var variable = new Variable("MyVar", "MyTagName");
        await mediator.Send(new SaveVariables(node.NodeId, [variable]));
        var specification = new Spec(spec);
        var reference = variable.Reference();
        specification.ShouldHave(Element.Tag.Property("TagName"), Operation.Containing, reference);
        await mediator.Send(new SaveSpec(specification));

        var result = await mediator.Send(new GetReferenceVariable(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(variable);
    }

    [Test]
    public async Task GetReferenceVariable_SeededVariablesForManyNodes_ShouldReturnSuccessAndExpectedVariable()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));
        await mediator.Send(new SaveVariables(collection.NodeId, [new Variable("CollectionVar", 123)]));
        await mediator.Send(new SaveVariables(folder.NodeId, [new Variable("FolderVar", "Test Value")]));
        await mediator.Send(new SaveVariables(spec.NodeId, [new Variable("SpecVar", TagType.Base)]));
        var specification = new Spec(spec);
        var reference = new Reference("SpecVar");
        specification.ShouldHave(Element.Tag.Property("TagType"), Operation.EqualTo, reference);
        await mediator.Send(new SaveSpec(specification));

        var result = await mediator.Send(new GetReferenceVariable(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.VariableId.Should().NotBeEmpty();
        result.Value.NodeId.Should().Be(spec.NodeId);
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
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));
        await mediator.Send(new SaveVariables(collection.NodeId, [new Variable("MyVar01", 123)]));
        await mediator.Send(new SaveVariables(folder.NodeId, [new Variable("MyVar01", "Test Value")]));
        await mediator.Send(new SaveVariables(spec.NodeId, [new Variable("MyVar01", TagType.Base)]));
        var specification = new Spec(spec);
        var reference = new Reference("MyVar01");
        specification.ShouldHave(Element.Tag.Property("TagType"), Operation.EqualTo, reference);
        await mediator.Send(new SaveSpec(specification));

        var result = await mediator.Send(new GetReferenceVariable(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.VariableId.Should().NotBeEmpty();
        result.Value.NodeId.Should().Be(spec.NodeId);
        result.Value.Name.Should().Be("MyVar01");
        result.Value.Value.Should().Be(TagType.Base);
        result.Value.Type.Should().Be(typeof(TagType));
        result.Value.Group.Should().Be(TypeGroup.Enum);
    }
}