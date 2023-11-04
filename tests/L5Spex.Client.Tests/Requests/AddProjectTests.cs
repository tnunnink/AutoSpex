using L5Spex.Client.Requests;
using L5Spex.Client.Requests.Nodes;
using Lamar;
using MediatR;

namespace L5Spex.Client.Tests.Requests;

[TestFixture]
public class AddProjectTests
{
    private IContainer _container = null!;

    [SetUp]
    public void Setup()
    {
        _container = SetupEnvironment();
    }
    
    [Test]
    public async Task Valid_WhenCalled_ShouldReturnSuccess()
    {
        var mediator = _container.GetInstance<IMediator>();
        var request = new AddProjectRequest("Test Project");

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }
}