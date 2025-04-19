using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunTests
{
    [Test]
    public void New_ValidNodeAndSource_ShouldHaveExpectedValues()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec");

        var run = new Run(spec, source);

        run.RunId.Should().NotBeEmpty();
        run.Node.NodeId.Should().Be(spec.NodeId);
        run.Node.Name.Should().Be(spec.Name);
        run.Node.Type.Should().Be(spec.Type);
        run.Source.Name.Should().Be("Test");
        run.Source.Location.Should().NotBeEmpty();
        run.Result.Should().Be(ResultState.None);
        run.Duration.Should().Be(0);
        run.Results.Should().BeEmpty();
    }

    [Test]
    public void New_ContainerNodeWithSpec_ShouldHaveExpectedRunChild()
    {
        var source = Source.Create(Known.Test);
        var container = Node.NewCollection();
        container.AddSpec("TestSpec");

        var run = new Run(container, source);

        run.Runs.Should().HaveCount(1);
    }

    [Test]
    public void Runs_ContainerNodeWithSpec_ShouldReturnSameInstanceEachTime()
    {
        var source = Source.Create(Known.Test);
        var container = Node.NewCollection();
        container.AddSpec("TestSpec");
        var run = new Run(container, source);

        var first = run.Runs.First();
        var second = run.Runs.First();

        first.Should().BeSameAs(second);
    }

    [Test]
    public async Task Execute_ValidNodeSpec_ShouldHaveExpectedValues()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(spec, source);

        
        var result = await run.Execute();

        result.Result.Should().Be(ResultState.Passed);
        result.Node.Name.Should().Be("TestSpec");
        result.Source.Name.Should().Be("Test");
        result.Target.Name.Should().Be("TestController");
        result.Duration.Should().NotBe(0);
        result.Total.Should().Be(1);
        result.Passed.Should().Be(1);
        result.Failed.Should().Be(0);
        result.Errored.Should().Be(0);
        result.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        result.RanBy.Should().NotBeEmpty();
    }

    [Test]
    public async Task Execute_SingleSpecNodeInContainingNode_ShouldReturnExpectedVerification()
    {
        var source = Source.Create(Known.Test);
        var container = Node.NewCollection("MyCollection");
        container.AddSpec("Test", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(container, source);

        var result = await run.Execute();

        result.Result.Should().Be(ResultState.Passed);
        result.Node.Name.Should().Be("MyCollection");
        result.Source.Name.Should().Be("Test");
        result.Target.Name.Should().Be("TestController");
        result.Duration.Should().NotBe(0);
        result.Total.Should().Be(1);
        result.Passed.Should().Be(1);
        result.Failed.Should().Be(0);
        result.Errored.Should().Be(0);
        result.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        result.RanBy.Should().NotBeEmpty();
        
        run.Result.Should().Be(ResultState.Passed);
        run.Results.Should().BeEmpty();
        run.Runs.Should().HaveCount(1);
        run.Runs.First().Node.Name.Should().Be("Test");
        run.Runs.First().Node.Type.Should().Be(NodeType.Spec);
        run.Runs.First().Result.Should().Be(ResultState.Passed);
        run.Runs.First().Results.Should().HaveCount(1);
    }

    [Test]
    public async Task Execute_MultipleSpecNodeInContainingNode_ShouldReturnExpectedVerifications()
    {
        var source = Source.Create(Known.Test);
        var container = Node.NewCollection("MyCollection");

        container.AddSpec("First", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });

        container.AddSpec("Second", s =>
        {
            s.Query(Element.Module);
            s.Verify("Inhibited", Operation.EqualTo, false);
        });

        container.AddSpec("Third", s =>
        {
            s.Query(Element.Program);
            s.Verify("Disabled", Operation.EqualTo, false);
        });
        var run = new Run(container, source);

        var result = await run.Execute();

        result.Result.Should().Be(ResultState.Passed);
        result.Node.Name.Should().Be("MyCollection");
        result.Source.Name.Should().Be("Test");
        result.Target.Name.Should().Be("TestController");
        result.Duration.Should().NotBe(0);
        result.Total.Should().Be(3);
        result.Passed.Should().Be(3);
        result.Failed.Should().Be(0);
        result.Errored.Should().Be(0);
        result.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        result.RanBy.Should().NotBeEmpty();
        run.Runs.Should().HaveCount(3);
    }


    [Test]
    public async Task DistinctResults_SingleState_ShouldHaveExpected()
    {
        var source = Source.Create(Known.Test);
        var node = Node.NewSpec("Test", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(node, source);
        await run.Execute();

        var states = run.DistinctResults().ToList();

        states.Should().HaveCount(1);
        states.Should().Contain(ResultState.Passed);
    }

    [Test]
    public async Task DistinctStates_MultipleState_ShouldHaveExpected()
    {
        var source = Source.Create(Known.Test);
        var container = Node.NewCollection("MyCollection");

        container.AddSpec("First", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });

        container.AddSpec("Second", s =>
        {
            s.Query(Element.Module);
            s.Verify("Inhibited", Operation.EqualTo, true);
        });

        container.AddSpec("Third", s =>
        {
            s.Query(Element.Program);
            s.Verify("IsDisabled", Operation.EqualTo, false);
        });
        var run = new Run(container, source);
        await run.Execute();

        var states = run.DistinctResults().ToList();

        states.Should().HaveCount(3);
        states.Should().Contain(ResultState.Passed);
        states.Should().Contain(ResultState.Failed);
        states.Should().Contain(ResultState.Errored);
    }

    [Test]
    public async Task TotalBy_PassedWithOnlyPassed_ShouldHaveExpected()
    {
        var source = Source.Create(Known.Test);
        var node = Node.NewSpec("Test", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(node, source);
        await run.Execute();

        var total = run.TotalBy(ResultState.Passed);

        total.Should().Be(1);
    }

    [Test]
    public async Task TotalBy_FailedWithOnlyPassed_ShouldHaveExpected()
    {
        var source = Source.Create(Known.Test);
        var node = Node.NewSpec("Test", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(node, source);
        await run.Execute();

        var total = run.TotalBy(ResultState.Failed);

        total.Should().Be(0);
    }

    [Test]
    public async Task TotalBy_MultipleState_ShouldHaveExpected()
    {
        var source = Source.Create(Known.Test);
        var container = Node.NewCollection("MyCollection");

        container.AddSpec("First", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });

        container.AddSpec("Second", s =>
        {
            s.Query(Element.Module);
            s.Verify("Inhibited", Operation.EqualTo, true);
        });

        container.AddSpec("Third", s =>
        {
            s.Query(Element.Program);
            s.Verify("IsDisabled", Operation.EqualTo, false);
        });

        var run = new Run(container, source);
        await run.Execute();

        var totalPassed = run.TotalBy(ResultState.Passed);
        totalPassed.Should().Be(1);

        var totalFailed = run.TotalBy(ResultState.Failed);
        totalFailed.Should().Be(1);

        var totalErrored = run.TotalBy(ResultState.Errored);
        totalErrored.Should().Be(1);
    }
}