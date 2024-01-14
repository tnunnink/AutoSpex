namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class SaveNodeTests
{
    [Test]
    public async Task SaveName_NodeWithVariablesNoSpec_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection("TestCollection");
        node.AddVariable(new Variable("Var1", "true"));
        node.AddVariable(new Variable("Var2", "1000"));
        node.AddVariable(new Variable("Var3", "TestValue"));

        var result = await mediator.Send(new SaveNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveName_SpecNodeWithVariables_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("Test");
        
        node.Configure(s =>
        {
            s.Element = Element.Controller;
            s.Where("Name", Operation.Equal, "MyController");
            s.Verify("Processor", Operation.Contains, "L7");
        });
        
        node.AddVariable(new Variable("Var1", "true"));
        node.AddVariable(new Variable("Var2", "1000"));
        node.AddVariable(new Variable("Var3", "TestValue"));

        var result = await mediator.Send(new SaveNode(node));

        result.IsSuccess.Should().BeTrue();
    }
}