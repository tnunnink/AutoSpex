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

        run.Nodes.Should().HaveCount(1);
        run.Specs.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
        run.Sources.Should().BeEmpty();
    }

    [Test]
    public void AddNode_SingleSourceNode_ShouldHaveExpectedNode()
    {
        var run = new Run();
        var node = Node.NewSource();

        run.AddNode(node);

        run.Nodes.Should().HaveCount(1);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
        run.Specs.Should().BeEmpty();
    }

    [Test]
    public void AddNodes_ManySpecNodes_ShouldHaveExpectedNode()
    {
        var run = new Run();
        var nodes = new[] { Node.NewSpec(), Node.NewSpec(), Node.NewSpec() };

        run.AddNodes(nodes);

        run.Nodes.Should().HaveCount(3);
        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_ContainerWithChildSpecs_ShouldHaveExpectedCount()
    {
        var container = Node.Root(NodeType.Spec).AddContainer();
        var folder = container.AddContainer();
        folder.AddSpec();
        folder.AddSpec();
        folder.AddSpec();
        var run = new Run();

        run.AddNode(container);

        run.Specs.Should().HaveCount(3);
        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_ContainerWithChildSources_ShouldHaveExpectedCount()
    {
        var container = Node.Root(NodeType.Source).AddContainer();
        var folder = container.AddContainer();
        folder.AddSource();
        folder.AddSource();
        folder.AddSource();
        var run = new Run();

        run.AddNode(container);

        run.Sources.Should().HaveCount(3);
        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_SpecThenSource_ShouldHaveExpectedCountAndUpdatedOutcome()
    {
        var spec = Node.NewSpec();
        var source = Node.NewSource();
        var run = new Run();

        run.AddNode(spec);
        run.AddNode(source);

        run.Nodes.Should().HaveCount(2);
        run.Specs.Should().HaveCount(1);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
        run.Outcomes.First().Spec?.NodeId.Should().Be(spec.NodeId);
        run.Outcomes.First().Spec?.Name.Should().Be(spec.Name);
        run.Outcomes.First().Source?.NodeId.Should().Be(source.NodeId);
        run.Outcomes.First().Source?.Name.Should().Be(source.Name);
    }

    [Test]
    public void AddNode_ManySpecsThenSource_ShouldHaveExpectedCounts()
    {
        var run = new Run();

        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSource());

        run.Nodes.Should().HaveCount(4);
        run.Specs.Should().HaveCount(3);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_ManySpecsThenManySources_ShouldHaveExpectedCounts()
    {
        var run = new Run();

        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());

        run.Nodes.Should().HaveCount(6);
        run.Specs.Should().HaveCount(3);
        run.Sources.Should().HaveCount(3);
        run.Outcomes.Should().HaveCount(9);
    }

    [Test]
    public void AddNode_ManySourcesThenManySpecs_ShouldHaveExpectedCounts()
    {
        var run = new Run();

        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());

        run.Nodes.Should().HaveCount(6);
        run.Specs.Should().HaveCount(3);
        run.Sources.Should().HaveCount(3);
        run.Outcomes.Should().HaveCount(9);
    }

    [Test]
    public void AddNode_SameNodeMultipleTimes_ShouldHaveExpectedCounts()
    {
        var run = new Run();

        var spec = Node.NewSpec();
        var source = Node.NewSource();
        run.AddNode(spec);
        run.AddNode(spec);
        run.AddNode(source);
        run.AddNode(source);

        run.Nodes.Should().HaveCount(2);
        run.Specs.Should().HaveCount(1);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
    }

    [Test]
    public void AddNode_ManySpecsAndSources_ShouldHaveExpectedPairs()
    {
        var run = new Run();

        run.AddNode(Node.NewSpec("Spec 1"));
        run.AddNode(Node.NewSpec("Spec 2"));
        run.AddNode(Node.NewSpec("Spec 3"));
        run.AddNode(Node.NewSource("Source 1"));
        run.AddNode(Node.NewSource("Source 2"));

        var outcomes = run.Outcomes.ToList();

        outcomes[0].Spec?.Name.Should().Be("Spec 1");
        outcomes[0].Source?.Name.Should().Be("Source 1");
        outcomes[1].Spec?.Name.Should().Be("Spec 2");
        outcomes[1].Source?.Name.Should().Be("Source 1");
        outcomes[2].Spec?.Name.Should().Be("Spec 3");
        outcomes[2].Source?.Name.Should().Be("Source 1");
        outcomes[3].Spec?.Name.Should().Be("Spec 1");
        outcomes[3].Source?.Name.Should().Be("Source 2");
        outcomes[4].Spec?.Name.Should().Be("Spec 2");
        outcomes[4].Source?.Name.Should().Be("Source 2");
        outcomes[5].Spec?.Name.Should().Be("Spec 3");
        outcomes[5].Source?.Name.Should().Be("Source 2");
    }

    [Test]
    public void AddNodes_ManyDifferentSpecFeatureNodes_ShouldHaveExpectedCount()
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
        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNodes_SpecAndSourceNode_ShouldHaveExpectedCounts()
    {
        var run = new Run();

        var nodes = new List<Node>
        {
            Node.NewSpec(), Node.NewSpec(), Node.NewSpec(), Node.NewSource(), Node.NewSource(), Node.NewSource(),
        };

        run.AddNodes(nodes);

        run.Nodes.Should().HaveCount(6);
        run.Specs.Should().HaveCount(3);
        run.Sources.Should().HaveCount(3);
        run.Outcomes.Should().HaveCount(9);
    }

    [Test]
    public void AddOutcome_SingleSpecAndSourceConfigured_ShouldHaveExpectedCounts()
    {
        var run = new Run();

        run.AddOutcome(new Outcome(Node.NewSpec(), Node.NewSource()));

        run.Nodes.Should().HaveCount(2);
        run.Specs.Should().HaveCount(1);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
    }

    [Test]
    public void AddOutcome_OnlySpecNodeConfigured_ShouldHaveExpectedCount()
    {
        var run = new Run();

        run.AddOutcome(new Outcome(Node.NewSpec()));

        run.Nodes.Should().HaveCount(1);
        run.Specs.Should().HaveCount(1);
        run.Sources.Should().HaveCount(0);
        run.Outcomes.Should().HaveCount(1);
    }

    [Test]
    public void AddOutcome_ManyTimes_ShouldHaveExpectedCount()
    {
        var run = new Run();

        var outcome = new Outcome(Node.NewSpec(), Node.NewSource());
        run.AddOutcome(outcome);
        run.AddOutcome(outcome);
        run.AddOutcome(outcome);

        run.Nodes.Should().HaveCount(2);
        run.Specs.Should().HaveCount(1);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
    }

    [Test]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    [TestCase(100000)]
    public void AddOutcome_ManyManyTime_ShouldNotTakeForever(int outcomes)
    {
        var run = new Run();

        for (var i = 0; i < outcomes; i++)
        {
            run.AddOutcome(new Outcome(Node.NewSpec(), Node.NewSource()));
        }

        run.Outcomes.Should().HaveCount(outcomes);
    }

    [Test]
    public void AddOutcome_ThenAddSourceNode_ShouldHaveExpectedCountAndUpdatedSource()
    {
        var run = new Run();

        run.AddOutcome(new Outcome(Node.NewSpec()));
        run.AddNode(Node.NewSource());

        run.Nodes.Should().HaveCount(2);
        run.Specs.Should().HaveCount(1);
        run.Sources.Should().HaveCount(1);
        run.Outcomes.Should().HaveCount(1);
        run.Outcomes.First().Source.Should().NotBeNull();
    }
}