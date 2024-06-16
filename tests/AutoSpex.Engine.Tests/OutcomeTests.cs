namespace AutoSpex.Engine.Tests;

[TestFixture]
public class OutcomeTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var outcome = new Outcome();

        outcome.OutcomeId.Should().NotBeEmpty();
        outcome.Spec.Should().BeNull();
        outcome.Source.Should().BeNull();
        outcome.Result.Should().Be(ResultState.None);
        outcome.Duration.Should().Be(0);
        outcome.Evaluations.Should().BeEmpty();
    }

    [Test]
    public void New_SingleSpecNode_ShouldBeExpected()
    {
        var node = Node.NewSpec();
        var outcome = new Outcome(node);
        
        outcome.Spec.Should().BeEquivalentTo(node);
        outcome.Source.Should().BeNull();
    }
    
    [Test]
    public void New_SingleSourceNode_ShouldBeExpected()
    {
        var node = Node.NewSource();
        var outcome = new Outcome(node);
        
        outcome.Source.Should().BeEquivalentTo(node);
        outcome.Spec.Should().BeNull();
    }

    [Test]
    public void New_SpecAndSourceNode_ShouldBeExpected()
    {
        var spec = Node.NewSpec();
        var source = Node.NewSource();
        var outcome = new Outcome(spec, source);
        
        outcome.Spec.Should().BeEquivalentTo(spec);
        outcome.Source.Should().BeEquivalentTo(source);
    }

    [Test]
    public void ConfigureSpec_ValidSpec_ShouldBeEquivalent()
    {
        var spec = Node.NewSpec();
        var outcome = new Outcome();

        outcome.ConfigureSpec(spec);

        outcome.Spec.Should().BeEquivalentTo(spec);
    }
    
    [Test]
    public void ConfigureSpec_NullSpec_ShouldBeNull()
    {
        var spec = Node.NewSpec();
        var outcome = new Outcome(spec);

        outcome.ConfigureSpec(null);

        outcome.Spec.Should().BeNull();
    }
    
    [Test]
    public void ConfigureSource_ValidSource_ShouldBeEquivalent()
    {
        var source = Node.NewSource();
        var outcome = new Outcome();

        outcome.ConfigureSource(source);

        outcome.Source.Should().BeEquivalentTo(source);
    }
    
    [Test]
    public void ConfigureSource_NullSource_ShouldBeNull()
    {
        var source = Node.NewSource();
        var outcome = new Outcome(source);

        outcome.ConfigureSource(null);

        outcome.Source.Should().BeNull();
    }

    [Test]
    public void METHOD()
    {
        
    }
}