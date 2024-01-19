using Task = System.Threading.Tasks.Task;

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

    [Test]
    public async Task Run_ValidSpecNode_ShouldReturnExpectedOutcomes()
    {
        var source = new Source(L5X.Load(Known.Test));
        var runner = new Runner();
        var collection = Node.NewCollection();
        var spec = collection.AddSpec("AllProgramsShouldNotBeDisabled", c =>
        {
            c.Element = Element.Program;
            c.Verify(nameof(Program.Disabled), Operation.Equal, false);
        });
        runner.AddNode(spec);
        
        var run = await runner.Run(source);

        run.Result.Should().Be(ResultState.Passed);
        run.Outcomes.Should().NotBeEmpty();
    }
    
    [Test]
    public async Task Run_NotAFIExists_ShouldBePassedIfThatIsTrue()
    {
        var source = new Source(L5X.Load(Known.Test));
        var runner = new Runner();
        var collection = Node.NewCollection();
        var spec = collection.AddSpec("No_AFIs_Should_Exist", c =>
        {
            c.Element = Element.Rung;
            c.Where(nameof(Rung.Text), Operation.Contains, "AFI");
            c.Settings.CountOperation = Operation.Equal;
            c.Settings.CountValue = 0;
        });
        runner.AddNode(spec);
        
        var run = await runner.Run(source);

        run.Result.Should().Be(ResultState.Passed);
        run.Outcomes.Should().NotBeEmpty();
    }
}