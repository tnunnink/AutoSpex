using MediatR;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Projects;

[TestFixture]
public class LaunchProjectTests
{
    [Test]
    public async Task Valid_WhenCalled_ShouldReturnNotEmptyResults()
    {
        /*using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();
        var request = new LaunchProjectRequest(context.ProjectPath);

        var result = await mediator.Send(request);

        result.IsSuccess.Should().BeTrue();*/
    }
}