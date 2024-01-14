namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunnerTests
{
    [Test]
    public void New_Overload_ShouldBeValid()
    {
        var runner = new Runner
        {
            Name = "MyRunner",
            Description = "This is setup to run some set of nodes"
        };

        runner.RunnerId.Should().NotBeEmpty();
        runner.Name.Should().Be("MyRunner");
        runner.Description.Should().Be("This is setup to run some set of nodes");
    }

    [Test]
    public void AddNode_SingleNode_ShouldHaveExpectedNode()
    {
        var runner = new Runner();
        
        runner.AddNode(Node.NewCollection());
        
        runner.Collections.Should().HaveCount(1);
    }

    [Test]
    public void AddNode_CollectionWithChildNodes_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        folder.AddSpec();
        var runner = new Runner();
        
        runner.AddNode(collection);

        runner.Collections.Should().HaveCount(1);
        runner.Collections.First().Descendents().Should().HaveCount(2);
    }

    [Test]
    public void AddNode_SpecWithParentNodes_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();
        var runner = new Runner();
        
        runner.AddNode(spec);
        
        runner.Collections.Should().HaveCount(1);
        runner.Collections.First().Descendents().Should().HaveCount(2);
        runner.Collections.First().Should().NotBeSameAs(collection);
        runner.Collections.First().NodeId.Should().Be(collection.NodeId);
    }
}