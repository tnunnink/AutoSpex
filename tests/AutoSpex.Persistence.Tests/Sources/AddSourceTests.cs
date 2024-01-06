using MediatR;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class AddSourceTests
{
    [Test]
    public async Task Send_ValidParameters_ShouldReturnSuccessAndValidValue()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();
        var request = new AddSourceRequest(new Uri(TestL5X), "MySource");

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeId.Should().NotBeEmpty();
        result.Value.ParentId.Should().BeEmpty();
        result.Value.Parent.Should().BeNull();
        result.Value.Name.Should().Be("MySource");
        result.Value.NodeType.Should().Be(NodeType.Source);
        result.Value.Ordinal.Should().Be(0);
    }
}