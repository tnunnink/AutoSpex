namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunTests
{
    [Test]
    public void New_Default_ShouldBeValid()
    {
        var run = new Run();

        run.RunId.Should().NotBeEmpty();
        run.Name.Should().Be("Run");
        run.Specs.Should().BeEmpty();
        run.Sources.Should().BeEmpty();
    }

    [Test]
    public void New_Overload_ShouldBeValid()
    {
        var run = new Run("MyRun");

        run.Name.Should().Be("MyRun");
    }

    [Test]
    public void AddNode_SingleSpecNode_ShouldHaveExpectedNode()
    {
        var run = new Run();
        var node = Node.NewSpec();

        run.AddNode(node);

        run.Specs.Should().HaveCount(1);
        run.Sources.Should().BeEmpty();
    }

    [Test]
    public void AddNode_SingleSourceNode_ShouldHaveExpectedNode()
    {
        var run = new Run();
        var node = Node.NewSource();

        run.AddNode(node);

        run.Sources.Should().HaveCount(1);
        run.Specs.Should().BeEmpty();
    }

    [Test]
    public void AddNodes_ManySpecNodes_ShouldHaveExpectedNode()
    {
        var run = new Run();
        var nodes = new[] { Node.NewSpec(), Node.NewSpec(), Node.NewSource() };

        run.AddNodes(nodes);

        run.Specs.Should().HaveCount(2);
        run.Sources.Should().HaveCount(1);
    }

    [Test]
    public void AddNode_CollectionWithChildNodes_ShouldHaveExpectedCount()
    {
        var container = Node.Root(NodeType.Spec).AddContainer();
        var folder = container.AddContainer();
        folder.AddSpec();
        folder.AddSpec();
        folder.AddSpec();
        var run = new Run();

        run.AddNode(container);

        run.Specs.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_ManyDifferentNodes_ShouldHaveExpectedOutcomesCount()
    {
        var run = new Run();
        var container = Node.Root(NodeType.Spec).AddContainer();
        var folder = container.AddContainer();
        var first = folder.AddSpec();
        var second = folder.AddSpec();
        var third = folder.AddSpec();
        var nodes = new[] { container, folder, first, second, third };

        run.AddNodes(nodes);

        run.Specs.Should().HaveCount(3);
    }

    [Test]
    public void GetOutcomes()
    {
        var run = new Run();
        var specs = Node.Root(NodeType.Spec).AddContainer();
        specs.AddSpec("Spec 1");
        specs.AddSpec("Spec 2");
        specs.AddSpec("Spec 3");
        var sources = Node.Root(NodeType.Source).AddContainer();
        sources.AddSource("Source 1");
        sources.AddSource("Source 2");

        run.AddNode(specs);
        run.AddNode(sources);

        var outcomes = run.Outcomes.ToList();

        outcomes.Should().HaveCount(6);
    }
}