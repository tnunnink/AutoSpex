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
        source.Content.Should().NotBeNull();
        source.Overrides.Should().BeEmpty();
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
        source.Overrides.Should().BeEmpty();
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
        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void SourceIdShouldBeInjectedIntoContentFileUponCreation()
    {
        var source = new Source(L5X.Load(Known.Test));

        var content = source.Content;
        var sourceId = content.Serialize().Attribute("SourceId")?.Value.Parse<Guid>();

        sourceId.Should().Be(source.SourceId);
    }

    [Test]
    public void SourceIdShouldBeInjectedIntoContentFileUponUpdate()
    {
        var source = new Source();
        source.Update(L5X.Load(Known.Test));

        var content = source.Content;
        var sourceId = content.Serialize().Attribute("SourceId")?.Value.Parse<Guid>();

        sourceId.Should().Be(source.SourceId);
    }

    [Test]
    public void AddOverride_ValidSpec_ShouldHaveExpectedCount()
    {
        var source = new Source();
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Filter("TagName", Operation.Containing, "Something");
            s.Verify("Value", Operation.EqualTo, 12);
        });

        source.AddOverride(spec);

        source.Overrides.Should().HaveCount(1);
        source.Overrides.First().Should().NotBeSameAs(spec);
    }

    [Test]
    public void AddOverride_NullSpec_ShouldThrowException()
    {
        var source = new Source();

        FluentActions.Invoking(() => source.AddOverride(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RemoveOverride_NoOverrides_ShouldBeExpectedCount()
    {
        var source = new Source();
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Filter("TagName", Operation.Containing, "Something");
            s.Verify("Value", Operation.EqualTo, 12);
        });

        source.RemoveOverride(spec);

        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void RemoveOverride_ExistingOverrides_ShouldBeExpectedCount()
    {
        var source = new Source();
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Filter("TagName", Operation.Containing, "Something");
            s.Verify("Value", Operation.EqualTo, 12);
        });

        source.AddOverride(spec);
        source.Overrides.Should().HaveCount(1);

        source.RemoveOverride(spec);
        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void ClearOverrides_WhenCalled_ShouldHaveExpectedCount()
    {
        var source = new Source();
        source.AddOverride(Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Filter("TagName", Operation.Containing, "Something");
            s.Verify("Value", Operation.EqualTo, 12);
        }));
        source.AddOverride(Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Filter("TagName", Operation.Containing, "Something");
            s.Verify("Value", Operation.EqualTo, 12);
        }));

        source.ClearOverrides();

        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void AddSuppression_ValidGuidAndMessage_ShouldHaveExpectedCount()
    {
        var source = new Source();

        source.AddSuppression(Guid.NewGuid(), "This is a test");

        source.Suppressions.Should().HaveCount(1);
    }

    [Test]
    public void AddSuppression_EmptyGuid_ShouldHaveExpectedCount()
    {
        var source = new Source();

        FluentActions.Invoking(() => source.AddSuppression(Guid.Empty, "This is a test")).Should()
            .Throw<ArgumentException>();
    }

    [Test]
    public void AddSuppression_EmptyReason_ShouldHaveExpectedCount()
    {
        var source = new Source();

        FluentActions.Invoking(() => source.AddSuppression(Guid.NewGuid(), "")).Should()
            .Throw<ArgumentException>();
    }

    [Test]
    public void AddSuppression_Null_ShouldThrowException()
    {
        var source = new Source();

        FluentActions.Invoking(() => source.AddSuppression(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RemoveSuppression_NoSuppression_ShouldBeExpectedCount()
    {
        var source = new Source();

        source.RemoveSuppression(new Suppression(Guid.NewGuid(), "My Reason"));

        source.Suppressions.Should().BeEmpty();
    }

    [Test]
    public void RemoveSuppression_ExistingOverrides_ShouldBeExpectedCount()
    {
        var source = new Source();

        var suppression = new Suppression(Guid.NewGuid(), "My Reason");
        source.AddSuppression(suppression);
        source.Suppressions.Should().HaveCount(1);

        source.RemoveSuppression(suppression);
        source.Suppressions.Should().BeEmpty();
    }

    [Test]
    public void ClearSuppressions_WhenCalled_ShouldHaveExpectedCount()
    {
        var source = new Source();

        source.AddSuppression(new Suppression(Guid.NewGuid(), "My Reason"));
        source.AddSuppression(new Suppression(Guid.NewGuid(), "My Reason"));

        source.ClearSuppressions();

        source.Suppressions.Should().BeEmpty();
    }
}