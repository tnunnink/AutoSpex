namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class RenameNodeTests
{
    [Test]
    public async Task Send_NoData_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new RenameNode(Node.NewCollection()));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_HasRecord_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.Name = "Updated Name";
        var result = await mediator.Send(new RenameNode(node));

        result.IsSuccess.Should().BeTrue();
    }
}