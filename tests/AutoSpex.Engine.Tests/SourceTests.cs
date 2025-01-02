namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceTests
{
    [Test]
    public void New_DefaultOverload_ShouldBeExpected()
    {
        var source = new Source();

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be("New Source");
        source.IsTarget.Should().BeFalse();
        source.TargetName.Should().BeEmpty();
        source.TargetType.Should().BeEmpty();
        source.ExportedOn.Should().BeEmpty();
        source.ExportedBy.Should().BeEmpty();
        source.Content.Should().BeNull();
        source.Rules.Should().BeEmpty();
    }

    [Test]
    public void New_ValidFile_ShouldBeExpected()
    {
        var file = L5X.Load(Known.Test);
        var source = new Source(file);

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be("TestController");
        source.IsTarget.Should().BeFalse();
        source.TargetName.Should().Be("TestController");
        source.TargetType.Should().Be("Controller");
        source.ExportedOn.Should().NotBeEmpty();
        source.ExportedBy.Should().NotBeEmpty();
        source.Content.Should().NotBeNull();
        source.Rules.Should().BeEmpty();
    }

    [Test]
    public void Update_ValidContent_ShouldBeExpected()
    {
        var source = new Source();

        var file = L5X.Load(Known.Test);
        source.Update(file);

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be("New Source");
        source.IsTarget.Should().BeFalse();
        source.TargetName.Should().Be("TestController");
        source.TargetType.Should().Be("Controller");
        source.ExportedOn.Should().NotBeEmpty();
        source.ExportedBy.Should().NotBeEmpty();
        source.Content.Should().NotBeNull();
        source.Rules.Should().BeEmpty();
    }

    [Test]
    public void SourceIdShouldBeInjectedIntoContentFileUponCreation()
    {
        var source = new Source(L5X.Load(Known.Test));

        var content = source.Content;
        var sourceId = content?.Serialize().Attribute("SourceId")?.Value.Parse<Guid>();

        sourceId.Should().Be(source.SourceId);
    }

    [Test]
    public void SourceIdShouldBeInjectedIntoContentFileUponUpdate()
    {
        var source = new Source();
        source.Update(L5X.Load(Known.Test));

        var content = source.Content;
        var sourceId = content?.Serialize().Attribute("SourceId")?.Value.Parse<Guid>();

        sourceId.Should().Be(source.SourceId);
    }

    [Test]
    public void AddRule_NullRule_ShouldThrowException()
    {
        var source = new Source();

        FluentActions.Invoking(() => source.AddRule(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddRule_OverrideValidSpec_ShouldHaveExpectedCount()
    {
        var source = new Source();
        var spec = Node.NewSpec("test", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.Containing, "Something");
            s.Validate("Value", Operation.EqualTo, 12);
        });

        var rule = Action.Override(spec, "This is a test");
        source.AddRule(rule);

        source.Rules.Should().HaveCount(1);
    }

    [Test]
    public void AddRule_SuppressRule_ShouldHaveExpectedCount()
    {
        var source = new Source();
        var rule = Action.Suppress(Guid.NewGuid(), "This is a test");
        
        source.AddRule(rule);

        source.Rules.Should().HaveCount(1);
    }

    [Test]
    public void RemoveRule_NoOverrideRule_ShouldBeExpectedCount()
    {
        var source = new Source();
        var spec = Node.NewSpec("test", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.Containing, "Something");
            s.Validate("Value", Operation.EqualTo, 12);
        });

        var rule = Action.Override(spec, "this is a test");
        source.RemoveRule(rule);

        source.Rules.Should().BeEmpty();
    }

    [Test]
    public void RemoveRule_OverrideRuleExists_ShouldBeExpectedCount()
    {
        var source = new Source();
        var spec = Node.NewSpec("test", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.Containing, "Something");
            s.Validate("Value", Operation.EqualTo, 12);
        });

        var rule = Action.Override(spec, "this is a test");

        source.AddRule(rule);
        source.Rules.Should().HaveCount(1);

        source.RemoveRule(rule);
        source.Rules.Should().BeEmpty();
    }

    [Test]
    public void RemoveRule_SuppressRuleExists_ShouldBeExpectedCount()
    {
        var source = new Source();

        var rule = Action.Suppress(Guid.NewGuid(), "My Reason");
        source.AddRule(rule);
        source.Rules.Should().HaveCount(1);

        source.RemoveRule(rule);
        source.Rules.Should().BeEmpty();
    }

    [Test]
    public void ClearSuppressions_WhenCalled_ShouldHaveExpectedCount()
    {
        var source = new Source();
        source.AddRule(Action.Suppress(Guid.NewGuid(), "My Reason"));
        source.AddRule(Action.Suppress(Guid.NewGuid(), "My Reason"));

        source.ClearRules();

        source.Rules.Should().BeEmpty();
    }
}