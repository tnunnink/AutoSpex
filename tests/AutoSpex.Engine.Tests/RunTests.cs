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
        var container = Node.FeatureRoot(NodeType.Spec).AddContainer();
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
        var container = Node.FeatureRoot(NodeType.Spec).AddContainer();
        var folder = container.AddContainer();
        var first = folder.AddSpec();
        var second = folder.AddSpec();
        var third = folder.AddSpec();
        var nodes = new[] { container, folder, first, second, third };

        run.AddNodes(nodes);

        run.Specs.Should().HaveCount(3);
    }
}