using L5Spex.Client.Requests;
using Lamar;
using MediatR;

namespace L5Spex.Client.Tests.Requests;

[TestFixture]
public class AddNodeTests
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
        
        /*var request = new AddNodeRequest();

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();*/
    }
}