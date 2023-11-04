using L5Spex.Client.Requests;
using L5Spex.Client.Requests.Nodes;
using Lamar;
using MediatR;

namespace L5Spex.Client.Tests.Requests;

[TestFixture]
public class GetNodeTreeTests
{
    private IContainer _container = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _container = SetupEnvironment();
    }
    
    [Test]
    public async Task SendRequest_WhenCalled_ShouldReturnSuccess()
    {
        var mediator = _container.GetInstance<IMediator>();
        var request = new GetNodeTreeRequest();

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SendRequest_WhenCalled_ShouldNotBeEmptyResults()
    {
        var mediator = _container.GetInstance<IMediator>();
        var request = new GetNodeTreeRequest();

        var result = await mediator.Send(request);

        result.IfSucc(records =>
        {
            records.Should().NotBeEmpty();
        });
    }
}