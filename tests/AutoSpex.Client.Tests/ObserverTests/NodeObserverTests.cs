using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using FluentAssertions;

namespace AutoSpex.Client.Tests.ObserverTests;

[TestFixture]
public class NodeObserverTests
{
    private TestContext? _context;

    [SetUp]
    public void Setup()
    {
        _context = new TestContext();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    public void New_ValidModel_ShouldNotBeNull()
    {
        var node = Node.NewContainer("Test");
        
        var observer = new NodeObserver(node);

        observer.Should().NotBeNull();
    }
    
    [Test]
    public void New_Null_ShouldThrowException()
    {
        FluentActions.Invoking(() => new NodeObserver(null!)).Should().Throw<ArgumentNullException>();
    }
}