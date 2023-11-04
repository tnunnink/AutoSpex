using L5Spex.Client.Requests;
using Lamar;
using MediatR;

namespace L5Spex.Client.Tests.Requests;

[TestFixture]
public class AddSourceTests
{
    private IContainer _container = null!;

    [SetUp]
    public void Setup()
    {
        _container = SetupEnvironment();
    }

    [Test]
    public async Task AddSource_ValidSourceRequest_ShouldUpdateDatabase()
    {
        var mediator = _container.GetInstance<IMediator>();
        var request = new AddSourceRequest("TestPath");

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetSources_NonExist_ShouldBeEmpty()
    {
        var mediator = _container.GetInstance<IMediator>();
        var request = new GetSourcesRequest();

        var result = await mediator.Send(request);

        result.IfSucc(x =>
        {
            x.Should().NotBeEmpty();
        });
    }
}