using Avalonia;
using FluentAssertions;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class RegistrarTests
{
    [Test]
    public void Build_WhenCalled_ReturnsWithoutError()
    {
        Registrar.Build();
        Assert.Pass();
    }

    [Test]
    public void Resolve_ShellViewMultipleTimes_ShouldReturnSameInstance()
    {
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .SetupWithoutStarting();
        
        Registrar.Build();

        var first = Registrar.Resolve<Shell>();
        var second = Registrar.Resolve<Shell>();

        first.Should().BeSameAs(second);
    }
}