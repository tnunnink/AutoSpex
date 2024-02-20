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
        var node = Node.NewCollection("Test");
        
        var observer = new NodeObserver(node);

        observer.Should().NotBeNull();
    }
    
    [Test]
    public void New_Null_ShouldThrowException()
    {
        FluentActions.Invoking(() => new NodeObserver(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsChanged_SetName_ShouldBeFalseBecauseWeChangeAndPersistItUponCommand()
    {
        var observer = new NodeObserver(Node.NewSpec("Test"));

        observer.Name = "Updated Name";

        observer.IsChanged.Should().BeFalse("Because this property is changed and persisted using the Rename command.");
    }
}