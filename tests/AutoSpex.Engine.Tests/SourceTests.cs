﻿namespace AutoSpex.Engine.Tests;

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
    public void AddOverride_ValidVariable_ShouldHaveExpectedCount()
    {
        var source = new Source();

        source.AddOverride(new Variable("TestVar", 123));

        source.Overrides.Should().HaveCount(1);
    }

    [Test]
    public void AddOverride_Null_ShouldThrowException()
    {
        var source = new Source();

        FluentActions.Invoking(() => source.AddOverride(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RemoveOverrid_NoOverrides_ShouldBeExpectedCount()
    {
        var source = new Source();

        source.RemoveOverride(new Variable("Test", 123));

        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void RemoveOverrid_ExistingOverrides_ShouldBeExpectedCount()
    {
        var source = new Source();

        var variable = new Variable("Test", 123);
        source.AddOverride(variable);
        source.Overrides.Should().HaveCount(1);

        source.RemoveOverride(variable);
        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void ClearOverrides_WhenCalled_ShouldHaveExpectedCount()
    {
        var source = new Source();

        source.AddOverride(new Variable("Test", 123));
        source.AddOverride(new Variable("Another", 123));

        source.ClearOverrides();

        source.Overrides.Should().BeEmpty();
    }
}