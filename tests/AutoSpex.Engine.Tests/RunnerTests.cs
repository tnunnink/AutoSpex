using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunnerTests
{
    /*[Test]
    public void New_Overload_ShouldBeValid()
    {
        var runner = new Runner
        {
            Name = "MyRunner",
            Documentation = "this is a test"
        };

        runner.RunnerId.Should().NotBeEmpty();
        runner.Name.Should().Be("MyRunner");
        runner.Documentation.Should().Be("this is a test");
        runner.Nodes.Should().BeEmpty();
    }

    [Test]
    public void AddNode_SingleNode_ShouldHaveExpectedNode()
    {
        var runner = new Runner();
        var node = Node.NewSpec();

        runner.AddNode(node);

        runner.Nodes.Should().HaveCount(1);
    }

    [Test]
    public void AddNode_CollectionWithChildNodes_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        folder.AddSpec();
        folder.AddSpec();
        folder.AddSpec();
        var runner = new Runner();

        runner.AddNode(collection);

        runner.Nodes.Should().HaveCount(5);
        runner.Collections.Should().HaveCount(1);
        runner.Specs.Should().HaveCount(3);
    }

    [Test]
    public void AddNodes_ManySpecNodes_ShouldHaveExpectedNode()
    {
        var runner = new Runner();
        var nodes = new[] {Node.NewSpec(), Node.NewSpec(), Node.NewSpec()};

        runner.AddNodes(nodes);

        runner.Nodes.Should().HaveCount(3);
        runner.Specs.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_ManyDifferentNodes_ShouldHaveExpectedOutcomesCount()
    {
        var runner = new Runner();
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var first = folder.AddSpec();
        var second = folder.AddSpec();
        var third = folder.AddSpec();
        var nodes = new[] {collection, folder, first, second, third};

        runner.AddNodes(nodes);

        runner.Nodes.Should().HaveCount(5);
    }

    [Test]
    public async Task Run_ValidSpecNode_ShouldReturnExpectedOutcomes()
    {
        var source = new Source(L5X.Load(Known.Test));
        var runner = new Runner {Source = source};
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Program);
            c.Verify(nameof(Program.Disabled), Operation.Equal, false);
        });

        var run = await runner.Run(spec);

        run.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public async Task Run_NoAFIExists_ShouldBePassedIfThatIsTrue()
    {
        var source = new Source(L5X.Load(Known.Test));
        var runner = new Runner {Source = source};
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Rung);
            c.Where(nameof(Rung.Text), Operation.Contains, "AFI");
            c.Settings.CountOperation = Operation.Equal;
            c.Settings.CountValue = 0;
        });

        var run = await runner.Run(spec);

        run.Result.Should().Be(ResultState.Passed);
    }*/
}