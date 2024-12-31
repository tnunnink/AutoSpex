using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using L5Sharp.Core;
using Action = System.Action;

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
    
    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    [Test]
    public void CheckForMemeoryLeaksAgainstNodeObserver()
    {
        var isolator = new Action(() =>
        {
            var node = Node.NewContainer("Test");
            var observer = new NodeObserver(node);
            observer.Should().NotBeNull();
        });

        isolator();

        GC.Collect();
        GC.WaitForFullGCComplete();

        // Assert L5X is removed from memory
        dotMemory.Check(memory => memory.GetObjects(where => where.Type.Is<NodeObserver>()).ObjectsCount.Should().Be(0));
    }
}