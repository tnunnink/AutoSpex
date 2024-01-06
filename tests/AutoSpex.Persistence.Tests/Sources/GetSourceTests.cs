using MediatR;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class GetSourceTests
{
    [Test]
    public async Task Send_ValidSourceExists_ShouldReturnValidResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();
        var addRequest = new AddSourceRequest(new Uri(TestL5X), "MySource");
        var addResult = await mediator.Send(addRequest);

        var request = new GetSourceRequest(addResult.Value.NodeId);
        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().NotBeNull();
    }
}