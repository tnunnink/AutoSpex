using Avalonia;
using FluentAssertions;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class ContainerTests
{
    [Test]
    public void Build_WhenCalled_ReturnsWithoutError()
    {
        Container.Build();
        Assert.Pass();
    }

    [Test]
    public void Resolve_ShellViewMultipleTimes_ShouldReturnSameInstance()
    {
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .SetupWithoutStarting();
        
        Container.Build();

        var first = Container.Resolve<Shell>();
        var second = Container.Resolve<Shell>();

        first.Should().BeSameAs(second);
    }
}